namespace gmafffff.AssistantSurveyor.FilePost.Акторы.Сообщения;

public abstract record Сообщение(Guid Ид) : ИСообщение<Guid> {
    public Guid Ид { get; init; } = Ид == default ? Guid.NewGuid() : Ид;
}