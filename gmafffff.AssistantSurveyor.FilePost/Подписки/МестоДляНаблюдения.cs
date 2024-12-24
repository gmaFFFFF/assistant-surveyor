namespace gmafffff.AssistantSurveyor.FilePost.Подписки;

/// <summary>
///     Наблюдаемая папка
/// </summary>
/// <param name="Путь">полный путь к папке без завершающего символа разделителя каталогов</param>
/// <param name="РекурсивноЛи">включить вложенные папки</param>
public record МестоДляНаблюдения(string Путь, bool РекурсивноЛи = false) {
    public МестоДляНаблюдения(DirectoryInfo каталог, bool рекурсивноЛи)
        : this(Path.TrimEndingDirectorySeparator(каталог.FullName), рекурсивноЛи) { }
}