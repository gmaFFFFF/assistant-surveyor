using gmafffff.AssistantSurveyor.FilePost.НаблюдательФС;

namespace gmafffff.AssistantSurveyor.FilePost.Акторы.ПочтаФайловая;

public partial class НаблюдательНовыхФайлов {
    #region Публичный интерфейс

    #region Команды

    public record НаблюдайЗа(Ориентировка Ориентировка, Guid Ид = default) : Сообщение(Ид);

    public record ПрекратиНаблюдение(Guid Ид = default) : Сообщение(Ид);

    public record ДайСтатус(Guid Ид) : Сообщение(Ид);

    #endregion

    #region Мгновенные ответы

    public record ПриступилКНаблюдению(Ориентировка Ориентировка, Guid Ид) : Сообщение(Ид) {
        public ПриступилКНаблюдению(НаблюдайЗа команда) : this(команда.Ориентировка, команда.Ид) { }
    }

    public record НаблюдениеНевозможно(Guid Ид, Ориентировка Ориентировка, Exception Ошибка)
        : НаблюдениеПрервано(Ид, Ориентировка, Ошибка) {
        public НаблюдениеНевозможно(НаблюдайЗа команда) : this(команда.Ид, команда.Ориентировка,
            new DirectoryNotFoundException()) { }
    }

    public record НаблюдениеПрекращено(Guid Ид) : Сообщение(Ид) {
        public НаблюдениеПрекращено(ПрекратиНаблюдение команда) : this(команда.Ид) { }
    }

    public record ЗасеченыНовыеФайлы(Ориентировка Ориентировка, Guid Ид, Seq<string> Файлы) : Сообщение(Ид) {
        public ЗасеченыНовыеФайлы(НаблюдайЗа команда, Seq<string> Файлы) : this(команда.Ориентировка, команда.Ид,
            Файлы) { }
    }

    public record ТекущийСтатус(Guid Ид) : Сообщение(Ид);

    #endregion

    #region Уведомления

    public record НаблюдениеПрервано(Guid Ид, Ориентировка Ориентировка, Exception Ошибка)
        : НаблюдениеПрекращено(Ид) {
        public НаблюдениеПрервано(НаблюдайЗа команда, Exception Ошибка) :
            this(команда.Ид, команда.Ориентировка, Ошибка) { }
    }

    #endregion

    #endregion


    #region Внутренние процессы

    public record ВозниклиФайловыеСобытия(IList<FileSystemEventArgs> События);

    public record ВозниклаОшибкаНаблюдения(Exception Ошибка);

    public record ОпустошиОчередьЗасечённыхФайлов(DateTime моментДо);

    #endregion

    #region Завершение

    #endregion

    #region Процесс

    #endregion
}