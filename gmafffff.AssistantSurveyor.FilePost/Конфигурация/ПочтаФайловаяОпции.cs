namespace gmafffff.AssistantSurveyor.FilePost.Конфигурация;

public class ПочтаФайловаяОпции {
    public const string Секция = "ПочтаФайловая";

    /// <summary>
    ///     Задержка, помогающая игнорировать промежуточные записи блоков файла на диск
    /// </summary>
    public TimeSpan ОжидаемоеВремяЗаписиНаДиск { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Задержка после создания файла и отправкой его на конвейер обработки
    /// </summary>
    public TimeSpan ЗадержкаПередОтправкойФайла { get; set; } = TimeSpan.FromMinutes(5);
}