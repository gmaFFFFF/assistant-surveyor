namespace gmafffff.AssistantSurveyor.FilePost.Подписки;

public record ПриметыСобытийФС(
    string ФильтрПоНазванию = "",
    СобытиеФайловойСистемыТип Событие =
        СобытиеФайловойСистемыТип.СуществовавшиеФайлы | СобытиеФайловойСистемыТип.Создание,
    DateTime? СозданПосле = default,
    bool ТолькоФайлы = true);