namespace gmafffff.AssistantSurveyor.FilePost.Подписки;

public interface ИОбработчик {
    /// <summary>
    ///     Желательно ли обработать документ прямо в исходной папке
    /// </summary>
    bool УдаленнаяОбработкаЖелательнаЛи { get; }

    /// <summary>
    ///     Помимо сигнального файла есть и другие файлы, составляющие комплексный документ
    /// </summary>
    bool ДокументСостоитИзНесколькихФайловЛи { get; }

    /// <summary>
    ///     Информация о текущем статусе обработки документа
    /// </summary>
    IObservable<ХодОбработки> ХодОбработки { get; }

    /// <summary>
    ///     Выбирает файлы, составляющие документ
    /// </summary>
    /// <param name="документ"></param>
    /// <param name="оглавление"></param>
    /// <returns></returns>
    Task<List<FileSystemInfo>> ПодбериВспомогательныеФайлы(FileSystemInfo документ,
        IEnumerable<FileSystemInfo> оглавление);

    /// <summary>
    ///     Запускает обработку документа
    /// </summary>
    /// <returns></returns>
    Task Обработай(List<FileSystemInfo> документ, CancellationToken токенОтмены = default);
}