namespace gmafffff.AssistantSurveyor.FilePost.Подписки;

[Flags]
public enum СобытиеФайловойСистемыТип {
    Нет = 0,
    СуществовавшиеФайлы = 1 << 0,
    Создание = 1 << 1,
    Изменение = 1 << 2,
    Удаление = 1 << 3,
    Любое = 0b_1111
}