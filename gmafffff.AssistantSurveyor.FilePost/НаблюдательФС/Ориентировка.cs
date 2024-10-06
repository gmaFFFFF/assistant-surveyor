namespace gmafffff.AssistantSurveyor.FilePost.НаблюдательФС;

public record Ориентировка(
    DirectoryInfo Каталог,
    DateTime? СозданПосле = default,
    string Фильтр = "",
    bool ОтслеживатьПодкаталоги = false);