namespace gmafffff.AssistantSurveyor.Service.Конфигурация;

public class ОбщиеОпции {
    public const string Секция = "Общие";
    public List<string> КонфигФайлы { get; set; } = [];

    public IEnumerable<string> ДайКонфигФайлы() {
        return КонфигФайлы
            .Select(Environment.ExpandEnvironmentVariables)
            .Where(File.Exists)
            .Where(путь => Path.GetExtension(путь) == ".json")
            .Select(Path.GetFullPath);
    }
}