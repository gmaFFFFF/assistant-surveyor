namespace gmafffff.AssistantSurveyor.FilePost;

public interface ИФайловаяПочта {
    string Название { get; }
    Task СтартАсинх(CancellationToken токенОстановки);
}