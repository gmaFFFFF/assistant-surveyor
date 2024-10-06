using gmafffff.AssistantSurveyor.FilePost.НаблюдательФС;

namespace gmafffff.AssistantSurveyor.FilePost.Акторы.ПочтаФайловая;

public partial class НаблюдательНовыхФайлов {
    #region

    public record НаблюдаемыйСкрылся(Ориентировка Ориентировка, Guid Ид, DateTime ПоследнееСобытие) : Сообщение(Ид) {
        public НаблюдаемыйСкрылся(НаблюдайЗа команда, DateTime ПоследнееСобытие) : this(команда.Ориентировка,
            команда.Ид, ПоследнееСобытие) { }
    }

    #endregion

    #region Публичный интерфейс

    #region Команды

    public record НаблюдайЗа(Ориентировка Ориентировка, Guid Ид = default) : Сообщение(Ид);

    public record ПрекратиНаблюдение(Guid Ид = default) : Сообщение(Ид);

    public record ДайСтатус(Guid Ид) : Сообщение(Ид);

    #endregion

    #region Ответы

    public record ПриступилКНаблюдению(Ориентировка Ориентировка, Guid Ид) : Сообщение(Ид) {
        public ПриступилКНаблюдению(НаблюдайЗа команда) : this(команда.Ориентировка, команда.Ид) { }
    }

    public record НаблюдениеПрекращено(Guid Ид, Ориентировка Ориентировка) : Сообщение(Ид) {
        public НаблюдениеПрекращено(НаблюдайЗа команда) : this(команда.Ид, команда.Ориентировка) { }
    }

    public record НаблюдениеПрервано(Guid Ид, Ориентировка Ориентировка, Exception Ошибка)
        : НаблюдениеПрекращено(Ид, Ориентировка) {
        public НаблюдениеПрервано(НаблюдайЗа команда, Exception Ошибка) :
            this(команда.Ид, команда.Ориентировка, Ошибка) { }
    }

    public record НаблюдениеНевозможно(Guid Ид, Ориентировка Ориентировка, Exception Ошибка)
        : НаблюдениеПрервано(Ид, Ориентировка, Ошибка) {
        public НаблюдениеНевозможно(НаблюдайЗа команда) : this(команда.Ид, команда.Ориентировка,
            new DirectoryNotFoundException()) { }
    }

    public record ЗасеченыНовыеФайлы(Ориентировка Ориентировка, Guid Ид, IList<FileInfo> Файлы) : Сообщение(Ид) {
        public ЗасеченыНовыеФайлы(НаблюдайЗа команда, IList<FileInfo> Файлы) : this(команда.Ориентировка, команда.Ид,
            Файлы) { }
    }

    public record ТекущийСтатус(Guid Ид) : Сообщение(Ид);

    #endregion

    #region Внутренние процессы

    public record ВозниклиФайловыеСобытия(IList<FileSystemEventArgs> События);

    public record ОшибкаНаблюдения(Exception Ошибка);

    #endregion

    #endregion

    #region Завершение

    #endregion

    #region Процесс

    #endregion
}