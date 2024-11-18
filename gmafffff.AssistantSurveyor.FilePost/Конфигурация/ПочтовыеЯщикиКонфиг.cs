namespace gmafffff.AssistantSurveyor.FilePost.Конфигурация;

public class ПочтовыеЯщикиКонфиг {
    public static readonly string Секция = nameof(ПочтовыеЯщикиКонфиг).Replace("Конфиг", "");
    public List<ПочтовыйЯщикКонфиг> ПочтовыеЯщики { get; set; } = [];
}