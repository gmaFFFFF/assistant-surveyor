using gmafffff.AssistantSurveyor.FilePost.Конфигурация;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gmafffff.AssistantSurveyor.FilePost;

public class ФайловаяПочтаЕгрн : ИФайловаяПочта {
    private readonly ПочтовыеЯщикиКонфиг _почтовыеЯщикиОпции;
    private readonly ILogger<ФайловаяПочтаЕгрн> _logger;

    public ФайловаяПочтаЕгрн(ILogger<ФайловаяПочтаЕгрн> logger, IOptions<ПочтовыеЯщикиКонфиг> ПочтовыеЯщикиОпции) {
        _logger = logger;
        _почтовыеЯщикиОпции = ПочтовыеЯщикиОпции.Value;
    }

    public string Название => "Почтовая служба ЕГРН";

    public async Task СтартАсинх(CancellationToken токенОстановки) {
        foreach (var p in _почтовыеЯщикиОпции.ПочтовыеЯщики)
            Console.WriteLine($"Мониторим {p.ДайНаблюдаемаяПапка()}");
    }
}