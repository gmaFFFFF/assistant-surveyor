using gmafffff.AssistantSurveyor.FilePost.Акторы.Сообщения;
using gmafffff.AssistantSurveyor.FilePost.Подписки;

namespace gmafffff.AssistantSurveyor.FilePost.Акторы.ПочтаФайловая;

public sealed partial class УправляющийПодписокАктор {
    public record ДобавьПодписку(Подписка Подписка, МестоДляНаблюдения Место, Guid Ид = default) : Команда(Ид);

    public record ПодпискаДобавлена(Подписка Подписка, МестоДляНаблюдения Место, Guid Ид) : Событие(Ид) {
        public ПодпискаДобавлена(ДобавьПодписку команда) : this(команда.Подписка, команда.Место, команда.Ид) { }
    }


    public record УдалиПодписку(Подписка Подписка, Guid Ид = default) : Команда(Ид);

    public record ПодпискаУдалена(Подписка Подписка, Guid Ид) : Событие(Ид) {
        public ПодпискаУдалена(УдалиПодписку команда) : this(команда.Подписка, команда.Ид) { }
    }


    public record ДайПодписки(Guid Ид = default) : Команда(Ид);

    public record ПодпискиПросмотр(ILookup<Подписка, МестоДляНаблюдения> Подписки, Guid Ид) : Сообщение(Ид);
}