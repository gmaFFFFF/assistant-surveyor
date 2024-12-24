using gmafffff.AssistantSurveyor.FilePost;

namespace gmafffff.AssistantSurveyor.Service.ФоновыеСлужбы;

public class ПочтоваяСлужба<Т>(ILogger<ПочтоваяСлужба<Т>> logger, IHostApplicationLifetime lifetime, Т почта)
    : BackgroundService
    where Т : ИФайловаяПочта {
    private readonly Т _почта = почта;

    protected override async Task ExecuteAsync(CancellationToken токенОстановки) {
        try {
            await ЖдатьЗапускПриложения(lifetime, токенОстановки);
        }
        catch (OperationCanceledException) {
            logger.LogInformation("Служба: {Служба} {Событие}", _почта.Название, "Запуск отменен");
            return;
        }

        while (!токенОстановки.IsCancellationRequested) {
            logger.LogInformation("Служба: {Служба} {Событие}", _почта.Название, "Запущена");

            try {
                await _почта.СтартАсинх(токенОстановки);
            }
            catch (OperationCanceledException) {
                logger.LogInformation("Служба: {Служба} {Событие}", _почта.Название, "Остановлена");
                throw;
            }
            catch (Exception e) {
                logger.LogWarning(e, "Служба: {Служба} {Событие}", _почта.Название, "Завершилась с ошибкой");

                // TODO: Обработка ошибки неуспешного запуска
            }

            // Перезапуск
            await Task.Delay(2000, токенОстановки);
        }
    }

    private static async Task ЖдатьЗапускПриложения(IHostApplicationLifetime lifetime,
        CancellationToken токенОстановки) {
        var tcs = new TaskCompletionSource();

        using var регСтарт = lifetime.ApplicationStarted.Register(() => tcs.TrySetResult());
        using var регОстановка = токенОстановки.Register(() => tcs.TrySetCanceled(токенОстановки));

        await tcs.Task;
    }
}