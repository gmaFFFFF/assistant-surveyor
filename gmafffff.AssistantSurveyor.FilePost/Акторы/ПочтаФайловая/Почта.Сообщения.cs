using gmafffff.AssistantSurveyor.FilePost.НаблюдательФС;

namespace gmafffff.AssistantSurveyor.FilePost.Акторы.ПочтаФайловая;

public partial class Почта {
    #region Внешние сообщения

    public record ДобавьНаблюдателя(Ориентировка Ориентировка, Guid Ид = default) : Сообщение(Ид);

    public record НаблюдательДобавлен(Ориентировка Ориентировка, Guid Ид) : Сообщение(Ид) {
        public НаблюдательДобавлен(ДобавьНаблюдателя запрос) : this(запрос.Ориентировка, запрос.Ид) { }
    }

    public record УдалиНаблюдателя(Ориентировка Ориентировка, Guid Ид = default) : Сообщение(Ид);

    public record НаблюдательУдален(Ориентировка Ориентировка, Guid Ид) : Сообщение(Ид) {
        public НаблюдательУдален(УдалиНаблюдателя запрос) : this(запрос.Ориентировка, запрос.Ид) { }
    }

    public record Подписаться(Ориентировка Ориентировка, Абонент Абонент, Guid Ид = default) : Сообщение(Ид);

    public record АбонентПодписан(Ориентировка Ориентировка, Абонент Абонент, Guid Ид) : Сообщение(Ид) {
        public АбонентПодписан(Подписаться запрос) : this(запрос.Ориентировка, запрос.Абонент, запрос.Ид) { }
    }

    public record Отписаться(Ориентировка Ориентировка, Абонент Абонент, Guid Ид = default) : Сообщение(Ид);

    public record АбонентОтписан(Ориентировка Ориентировка, Абонент Абонент, Guid Ид) : Сообщение(Ид) {
        public АбонентОтписан(Отписаться запрос) : this(запрос.Ориентировка, запрос.Абонент, запрос.Ид) { }
    }

    public record ДайПодписки(Guid Ид = default) : Сообщение(Ид);

    public record АктуальныеПодписки(Lookup<DirectoryInfo, Абонент> Подписки, Guid Ид) : Сообщение(Ид);

    public record ДайСостояниеКаталога(DirectoryInfo Каталог, Guid Ид = default) : Сообщение(Ид);

    public record СостояниеКаталогаТек(DirectoryInfo Каталог, СостояниеКаталога Состояние, Guid Ид) : Сообщение(Ид);

    public record ДайДайджестПоследних(DirectoryInfo Каталог, DateTime Отсечка, Guid Ид = default) : Сообщение(Ид);

    #endregion

    #region Внутренние сообщения

    public record ПопробуйВосстановитьПочтовыйЯщик(DirectoryInfo Каталог, Guid Ид = default) : Сообщение(Ид);

    public record ПочтовыйЯщикНедоступен(DirectoryInfo Каталог, Guid Ид) : Сообщение(Ид);

    #endregion
}