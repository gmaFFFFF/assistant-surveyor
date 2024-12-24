using Akka.Actor;
using Akka.Event;
using gmafffff.AssistantSurveyor.FilePost.Подписки;
using Microsoft.Extensions.DependencyInjection;

namespace gmafffff.AssistantSurveyor.FilePost.Акторы.ПочтаФайловая;

public sealed partial class УправляющийПодписокАктор : ReceiveActor {
    #region Состояние

    private readonly Dictionary<Подписка, List<МестоДляНаблюдения>> _подписки = [];

    #endregion

    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IServiceScope _scope;

    #region Akka

    public УправляющийПодписокАктор(IServiceProvider sp) {
        _scope = sp.CreateScope();

        Receive<ДобавьПодписку>(ОбработайДобавьПодписку);
        Receive<УдалиПодписку>(ОбработайУдалиПодписку);
        Receive<ДайПодписки>(ОбработайДайПодписки);
    }

    protected override void PostStop() {
        _scope.Dispose();
    }

    #endregion

    #region Обработчики команд

    private void ОбработайДобавьПодписку(ДобавьПодписку команда) {
        var новаяПапкаНаблюдения = new DirectoryInfo(команда.Место.Путь);
        if (!новаяПапкаНаблюдения.Exists)
            _log.Warning("Наблюдаемая папка: {папка}. Событие: {событие}", команда.Место.Путь,
                "Каталог не существует или недоступен");

        var естьПодписка = _подписки.TryGetValue(команда.Подписка, out var наблюдаемыеПапки);
        if (!естьПодписка) {
            наблюдаемыеПапки = [];
        }
        else if (наблюдаемыеПапки!.Contains(команда.Место)) {
            _log.Debug("Наблюдаемая папка: {папка}. Событие: {событие}", команда.Место.Путь,
                "Подписка существовала ранее");

            Sender.Tell(new ПодпискаДобавлена(команда));
            return;
        }

        _подписки[команда.Подписка] = наблюдаемыеПапки;

        //TODO: Передать в менеджер наблюдателей


        _log.Debug("Наблюдаемая папка: {папка}. Событие: {событие}", команда.Место.Путь,
            "Подписан на события файловой системы");

        Sender.Tell(new ПодпискаДобавлена(команда));
    }

    private void ОбработайУдалиПодписку(УдалиПодписку команда) {
        var естьПодписка = _подписки.TryGetValue(команда.Подписка, out _);
        if (!естьПодписка) {
            Sender.Tell(new ПодпискаУдалена(команда));
            _log.Warning("Подписка: {@подписка}. Событие: {событие}", команда.Подписка,
                "Подписка не существовала ранее");
            return;
        }

        _подписки.Remove(команда.Подписка);
        //TODO: Передать в менеджер наблюдателей


        _log.Debug("Подписка: {@подписка}. Событие: {событие}", команда.Подписка,
            "Подписка удалена");

        Sender.Tell(new ПодпискаУдалена(команда));
    }

    private void ОбработайДайПодписки(ДайПодписки команда) {
        var просмотр = (from папки in _подписки
                from папка in папки.Value
                select new { папки.Key, Value = папка })
            .ToLookup(kv => kv.Key, kv => kv.Value);

        var ответ = new ПодпискиПросмотр(просмотр, команда.Ид);
        Sender.Tell(ответ);
    }

    #endregion
}