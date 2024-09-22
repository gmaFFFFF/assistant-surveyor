namespace gmafffff.AssistantSurveyor.Post;

public class ПочтаЕгрн : ИПочта {
    public string Название => "Почтовая служба ЕГРН";

    public async Task СтартАсинх(CancellationToken токенОстановки) {
        Console.WriteLine("Мониторим...");
    }
}