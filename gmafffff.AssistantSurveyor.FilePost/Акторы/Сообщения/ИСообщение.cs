namespace gmafffff.AssistantSurveyor.FilePost.Акторы.Сообщения;

public interface ИСообщение<Т> where Т : struct {
    Т Ид { get; init; }
}