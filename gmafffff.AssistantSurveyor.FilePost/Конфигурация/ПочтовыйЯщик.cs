namespace gmafffff.AssistantSurveyor.FilePost.Конфигурация;

public class ПочтовыйЯщик {
    public string НаблюдаемаяПапка { get; set; } = string.Empty;
    public List<string> ПапкиРезультата { get; set; } = [];

    public IEnumerable<string> ДайПапкиРезультата() {
        return ПапкиРезультата
            .Select(Environment.ExpandEnvironmentVariables)
            .Where(Directory.Exists)
            .Select(Path.GetFullPath);
    }

    public string? ДайНаблюдаемаяПапка() {
        var раскрытПуть = Environment.ExpandEnvironmentVariables(НаблюдаемаяПапка);
        return Directory.Exists(раскрытПуть)
            ? Path.GetFullPath(раскрытПуть)
            : null;
    }
}