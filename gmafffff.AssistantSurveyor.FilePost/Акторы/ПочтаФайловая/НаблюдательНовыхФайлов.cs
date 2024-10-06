using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Event;
using gmafffff.AssistantSurveyor.FilePost.Конфигурация;
using gmafffff.AssistantSurveyor.FilePost.НаблюдательФС;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace gmafffff.AssistantSurveyor.FilePost.Акторы.ПочтаФайловая;

public partial class НаблюдательНовыхФайлов : ReceiveActor, ILogReceive {
    private Option<НаблюдайЗа> _командаНаблюдения = None;
    private Option<IDisposable> _подпискаНаФайловыйНаблюдатель = None;

    public НаблюдательНовыхФайлов(IServiceProvider sp) {
        #region Инициализация

        _scope = sp.CreateScope();

        #endregion

        Запуск();
    }

    private bool _активенЛи => _подпискаНаФайловыйНаблюдатель.IsSome;

    private void Запуск() {
        //Todo: Обработчик при запущенном процессе
        Receive<НаблюдайЗа>(НачниНаблюдение);
    }

    #region Обработчики

    private void НачниНаблюдение(НаблюдайЗа команда) {
        if (!команда.Ориентировка.Каталог.Exists) {
            _log.Warning("Наблюдаемый каталог: {каталог}. Событие: {событие}", команда.Ориентировка.Каталог,
                "Каталог не существует");
            Sender.Tell(new НаблюдениеНевозможно(команда));
            return;
        }

        var наблюдатель = СоздатьФайловыйНаблюдатель(команда.Ориентировка, _scope);
        var подписка = ПодписатьсяНаФайловогоНаблюдателя(наблюдатель, Context.Self);

        _подпискаНаФайловыйНаблюдатель = Some(подписка);
        _командаНаблюдения = команда;

        Sender.Tell(new ПриступилКНаблюдению(команда));

        _log.Debug("Наблюдаемый каталог: {каталог}. Событие: {событие}", команда.Ориентировка.Каталог,
            "Подписан на события создания новых файлов");
    }

    #endregion

    #region DI

    private readonly IServiceScope _scope;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    #endregion

    #region Потроха команды «начни наблюдение»

    public static ИFileSystemWatcherОбёртка НастроитьFileSystemWatcher(Ориентировка ориентировка,
        ИFileSystemWatcherОбёртка fsw) {
        fsw.Path = ориентировка.Каталог.FullName;
        fsw.Filter = ориентировка.Фильтр;
        fsw.IncludeSubdirectories = ориентировка.ОтслеживатьПодкаталоги;
        // При изменении файла нас интересуют только события, связанные с последней записью на диск
        fsw.NotifyFilter = NotifyFilters.LastWrite;
        return fsw;
    }

    public static IObservable<FileSystemEventArgs> FileSystemWatcher8Observable(TimeSpan периодБездействия,
        ИПоставляетПланировщики поставщикПланировщиков, ИFileSystemWatcherОбёртка fs)
        => fs.ToObservable()
            // Переименование заменяем 2 событиями: создание и удаление
            .SelectMany(fse => fse switch {
                RenamedEventArgs rfse => new[] {
                    new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetPathRoot(rfse.FullPath)!, rfse.Name),
                    new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetPathRoot(rfse.OldFullPath)!,
                        rfse.OldName)
                },
                _ => [fse]
            })
            // Группируем поток событий по файлам
            .GroupBy(fse => fse.FullPath)
            //Обрабатываем события, касающиеся отдельного файла
            .SelectMany(gfse =>
                // Такая фильтрация возможна, т.к. IGroupedObservable<Tkey,TElem> — наследник IObservable<TElem>
                gfse.OfType<FileSystemEventArgs>()
                    // Группируем файловые события до тех пор пока они происходят, но в рамках периодБездействия
                    .Quiescent(периодБездействия, поставщикПланировщиков.Default)
                    .Select(fse =>
                        // Выбираем единственное актуальное событие
                        fse.Reduce((пред, след) =>
                            след switch {
                                // Если файл удалён, то предыдущее событие не важно
                                { ChangeType: WatcherChangeTypes.Deleted } => след,
                                // Изменение файла говорит нам только о том, что его создание не завершено
                                { ChangeType: WatcherChangeTypes.Changed } =>
                                    пред.ChangeType is WatcherChangeTypes.Created
                                        ? пред
                                        // Если мы начали наблюдать за файлом в момент когда в него ведётся запись, то у нас не было события Create
                                        : new FileSystemEventArgs(WatcherChangeTypes.Created,
                                            Path.GetPathRoot(след.FullPath)!, след.Name),
                                // Неважно как раньше назывался файл
                                RenamedEventArgs r => throw new InvalidOperationException(
                                    "RenamedEventArgs не должно было до сюда дойти"),
                                // На всякий случай
                                _ => throw new InvalidOperationException(
                                    $"Неучтенный путь выполнения пред: {пред}, след: {след}")
                            }
                        )
                    )
            );

    public static IEnumerable<string> НайдиСуществующиеФайлы(Ориентировка ориентировка)
        => Directory.EnumerateFiles(ориентировка.Каталог.FullName, ориентировка.Фильтр,
                ориентировка.ОтслеживатьПодкаталоги ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(имя => File.GetCreationTimeUtc(имя) >= ориентировка.СозданПосле);

    public static IObservable<FileSystemEventArgs> СуществующиеФайлыКакСобытияСоздания(
        IEnumerable<string> файлыСуществующие)
        => файлыСуществующие
            .Select(имя => new FileSystemEventArgs(WatcherChangeTypes.Created,
                Path.GetPathRoot(имя) ?? throw new InvalidOperationException("Должен был быть каталог"),
                Path.GetFileName(имя) ?? throw new InvalidOperationException("У файла должно было быть имя")))
            .ToObservable();

    public static IObservable<IList<FileSystemEventArgs>> ОбъединитьСуществующиеФайлыССобытиямиФайлов(
        TimeSpan периодГруппировки, ИПоставляетПланировщики поставщикПланировщиков,
        IObservable<FileSystemEventArgs> существующие, IObservable<FileSystemEventArgs> события)
        => существующие
            .Concat(события)
            .BufferWhenAvailable(периодГруппировки, поставщикПланировщиков.Default);

    public static IDisposable ПодписатьсяНаФайловогоНаблюдателя(IObservable<IList<FileSystemEventArgs>> fse,
        IActorRef sender)
        => fse.Subscribe(
            next => sender.Tell(new ВозниклиФайловыеСобытия(next)),
            error => sender.Tell(new ОшибкаНаблюдения(error))
        );

    public static IObservable<IList<FileSystemEventArgs>> СоздатьФайловыйНаблюдатель(Ориентировка ориентировка,
        IServiceScope scope) {
        var настройки = scope.ServiceProvider
            .GetService<IOptionsSnapshot<ПочтаФайловаяОпции>>()
            ?.Value ?? new ПочтаФайловаяОпции();
        var поставщикПланировщиковRx = scope.ServiceProvider
            .GetService<ИПоставляетПланировщики>() ?? new ПоставляетПланировщики();
        var дайFileSystemWatcher = (IServiceScope scope) => scope.ServiceProvider
            .GetRequiredService<ИFileSystemWatcherОбёртка>();

        var стартServiceScope = identity<IServiceScope>;
        var стартОриентировка = identity<Ориентировка>;
        var настроитьFSW = НастроитьFileSystemWatcher;
        var получитьObservable = FileSystemWatcher8Observable;
        var объединитьСуществующиеФайлыССобытиями = ОбъединитьСуществующиеФайлыССобытиямиФайлов;

        var получитьРанееСозданныеФайлы = compose(
            стартОриентировка,
            НайдиСуществующиеФайлы,
            СуществующиеФайлыКакСобытияСоздания
        );

        var подготовитьНаблюдателяФС = compose(
            стартServiceScope,
            дайFileSystemWatcher,
            par(настроитьFSW, ориентировка),
            par(получитьObservable, настройки.ОжидаемоеВремяЗаписиНаДиск, поставщикПланировщиковRx)
        );

        return объединитьСуществующиеФайлыССобытиями(
            настройки.ОжидаемоеВремяЗаписиНаДиск * 3,
            поставщикПланировщиковRx,
            получитьРанееСозданныеФайлы(ориентировка),
            подготовитьНаблюдателяФС(scope));
    }

    #endregion

    #region Akka

    public static Props Props(int magicNumber) {
        return Akka.Actor.Props.Create<НаблюдательНовыхФайлов>();
    }

    protected override void PostStop() {
        _scope.Dispose();
    }

    #endregion
}