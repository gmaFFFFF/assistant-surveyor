using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Event;
using gmafffff.AssistantSurveyor.FilePost.Конфигурация;
using gmafffff.AssistantSurveyor.FilePost.НаблюдательФС;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace gmafffff.AssistantSurveyor.FilePost.Акторы.ПочтаФайловая;

public partial class НаблюдательНовыхФайлов : ReceiveActor, IWithTimers, ILogReceive {
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IServiceScope _scope;

    private Option<НаблюдайЗа> _командаНаблюдения = None;
    private Option<IDisposable> _подпискаНаФайловыйНаблюдатель = None;
    private Option<IActorRef> _получатель = None;

    public НаблюдательНовыхФайлов(IServiceProvider sp) {
        _scope = sp.CreateScope();

        //Todo: Обработчик при запущенном процессе
        Receive<НаблюдайЗа>(НачниНаблюдение);
        Receive<ПрекратиНаблюдение>(ОстановиНаблюдение);
        Receive<ВозниклиФайловыеСобытия>(ОбработайФайловыеСобытия);
        Receive<ОпустошиОчередьЗасечённыхФайлов>(ОпустошиЗасечённыеФайлы);
        Receive<ВозниклаОшибкаНаблюдения>(ОбработайОшибки);
    }

    public bool АктивенЛи => _подпискаНаФайловыйНаблюдатель.IsSome;
    public HashMap<string, DateTime> ЗасечённыеФайлы { get; private set; } = HashMap<string, DateTime>();
    public ITimerScheduler Timers { get; set; }

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
        _получатель = Some(Sender);

        Sender.Tell(new ПриступилКНаблюдению(команда));

        _log.Debug("Наблюдаемый каталог: {каталог}. Событие: {событие}", команда.Ориентировка.Каталог,
            "Подписан на события создания новых файлов");
    }

    private void ОстановиНаблюдение(ПрекратиНаблюдение команда) {
        if (!АктивенЛи) Sender.Tell(new НаблюдениеПрекращено(команда));
        //Todo: Добавить сброс накопленных файловых событий

        _командаНаблюдения = None;
        _подпискаНаФайловыйНаблюдатель.IfSome(подписка => подписка.Dispose());
        _подпискаНаФайловыйНаблюдатель = None;
        _получатель = None;
        Sender.Tell(new НаблюдениеПрекращено(команда));
    }

    private void ОбработайФайловыеСобытия(ВозниклиФайловыеСобытия уведомление) {
        var настройки = _scope.ServiceProvider
            .GetService<IOptionsSnapshot<ПочтаФайловаяОпции>>()
            ?.Value ?? new ПочтаФайловаяОпции();
        var задержка = настройки.ЗадержкаПередОтправкойФайла;
        var сейчас = DateTime.UtcNow;

        foreach (var событие in уведомление.События)
            ЗасечённыеФайлы = событие switch {
                { ChangeType: WatcherChangeTypes.Created }
                    => ЗасечённыеФайлы.AddOrUpdate(событие.FullPath, сейчас),
                { ChangeType: WatcherChangeTypes.Deleted }
                    => ЗасечённыеФайлы.Filter((путь, _) => путь != событие.FullPath),
                _ => throw new InvalidOperationException(
                    "Сюда должны были прийти только события создания и удаления файлов")
            };

        Timers.StartSingleTimer(сейчас, new ОпустошиОчередьЗасечённыхФайлов(сейчас), задержка);
    }

    private void ОпустошиЗасечённыеФайлы(ОпустошиОчередьЗасечённыхФайлов событие) {
        var вылежались = ЗасечённыеФайлы.Filter((_, время) => время <= событие.моментДо);
        ЗасечённыеФайлы = ЗасечённыеФайлы.Except(вылежались);
        var файлы = вылежались.Keys
            .Where(File.Exists)
            .ToSeq();

        // Самый короткий способ выполнить действие с несколькими монадами одновременно, который я сейчас знаю…
        (_получатель, _командаНаблюдения).Apply((получатель, команда) => {
            var сообщение = new ЗасеченыНовыеФайлы(команда, файлы);
            получатель.Tell(сообщение, Self);
            return unit;
        }).As().IfNone(() => throw new InvalidOperationException("Все объекты должны были быть определены"));
    }

    private void ОбработайОшибки(ВозниклаОшибкаНаблюдения событие) { }

    #endregion

    #region Потроха команды «Начни наблюдение»

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
        ИПоставляетПланировщики поставщикПланировщиков, ИFileSystemWatcherОбёртка fs) {
        return fs.ToObservable()
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
    }

    public static IEnumerable<string> НайдиСуществующиеФайлы(Ориентировка ориентировка) {
        return Directory.EnumerateFiles(ориентировка.Каталог.FullName, ориентировка.Фильтр,
                ориентировка.ОтслеживатьПодкаталоги ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(имя => File.GetCreationTimeUtc(имя) >= ориентировка.СозданПосле);
    }

    public static IObservable<FileSystemEventArgs> СуществующиеФайлыКакСобытияСоздания(
        IEnumerable<string> файлыСуществующие) {
        return файлыСуществующие
            .Select(имя => new FileSystemEventArgs(WatcherChangeTypes.Created,
                Path.GetPathRoot(имя) ?? throw new InvalidOperationException("Должен был быть каталог"),
                Path.GetFileName(имя) ?? throw new InvalidOperationException("У файла должно было быть имя")))
            .ToObservable();
    }

    public static IObservable<IList<FileSystemEventArgs>> ОбъединитьСуществующиеФайлыССобытиямиФайлов(
        TimeSpan периодГруппировки, ИПоставляетПланировщики поставщикПланировщиков,
        IObservable<FileSystemEventArgs> существующие, IObservable<FileSystemEventArgs> события) {
        return существующие
            .Concat(события)
            .BufferWhenAvailable(периодГруппировки, поставщикПланировщиков.Default);
    }

    public static IDisposable ПодписатьсяНаФайловогоНаблюдателя(IObservable<IList<FileSystemEventArgs>> fse,
        IActorRef sender) {
        return fse.Subscribe(
            next => sender.Tell(new ВозниклиФайловыеСобытия(next)),
            error => sender.Tell(new ВозниклаОшибкаНаблюдения(error))
        );
    }

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