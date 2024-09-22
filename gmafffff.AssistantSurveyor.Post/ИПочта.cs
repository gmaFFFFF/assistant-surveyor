namespace gmafffff.AssistantSurveyor.Post;

public interface ИПочта {
    string Название { get; }
    Task СтартАсинх(CancellationToken токенОстановки);
}