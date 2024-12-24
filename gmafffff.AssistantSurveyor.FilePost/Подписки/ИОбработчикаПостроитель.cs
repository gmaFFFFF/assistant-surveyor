namespace gmafffff.AssistantSurveyor.FilePost.Подписки;

public interface ИОбработчикаПостроитель {
    /// <summary>
    ///     Предварительная (быстрая) проверка файла/папки на то поддерживается ли он обработчиком
    /// </summary>
    /// <param name="fsi">файл/папка, проверяемый на соответствие запросу</param>
    /// <returns>true — файл/папка соответствует</returns>
    static abstract bool МожноПопробоватьОбработатьЛи(FileSystemInfo fsi);

    /// <summary>
    ///     Возвращает
    /// </summary>
    /// <returns></returns>
    static abstract ИОбработчик ДайОбработчик();
}