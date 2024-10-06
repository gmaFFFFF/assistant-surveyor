using gmafffff.AssistantSurveyor.FilePost.Сообщения;

namespace gmafffff.AssistantSurveyor.FilePost.Акторы;

public abstract record Сообщение(Guid Ид) : ИСообщение {
    public Guid Ид { get; init; } = Ид == default ? Guid.NewGuid() : Ид;
}