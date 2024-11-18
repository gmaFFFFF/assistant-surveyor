namespace gmafffff.AssistantSurveyor.Service.Конфигурация;

public class ОбщиеКонфиг {
    public static readonly string Секция = nameof(ОбщиеКонфиг).Replace("Конфиг", "");
    public List<string> КонфигФайлы { get; set; } = [];

    public IEnumerable<string> ДайКонфигФайлы() {
        return КонфигФайлы
            .Select(Environment.ExpandEnvironmentVariables)
            .Where(File.Exists)
            .Where(путь => Path.GetExtension(путь) == ".json")
            .Select(Path.GetFullPath);
    }
}