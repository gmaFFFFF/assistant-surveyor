using gmafffff.AssistantSurveyor.Post;

namespace gmafffff.AssistantSurveyor.Service.ФоновыеСлужбы;

public class ПочтоваяСлужба<Т> : BackgroundService
    where Т : ИПочта {
    private readonly Т _почта;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<ПочтоваяСлужба<Т>> _logger;

    public ПочтоваяСлужба(ILogger<ПочтоваяСлужба<Т>> logger, IHostApplicationLifetime lifetime, Т почта) {
        _logger = logger;
        _lifetime = lifetime;
        _почта = почта;
    }

    protected override async Task ExecuteAsync(CancellationToken токенОстановки) {
        try {
            await ЖдатьЗапускПриложения(_lifetime, токенОстановки);
        }
        catch (OperationCanceledException e) {
            _logger.LogInformation("Служба: {Служба} {Событие}", _почта.Название, "Запуск отменен");
            return;
        }

        while (!токенОстановки.IsCancellationRequested) {
            _logger.LogInformation("Служба: {Служба} {Событие}", _почта.Название, "Запущена");

            try {
                await _почта.СтартАсинх(токенОстановки);
            }
            catch (OperationCanceledException e) {
                _logger.LogInformation("Служба: {Служба} {Событие}", _почта.Название, "Остановлена");
                throw;
            }
            catch (Exception e) {
                _logger.LogWarning(e, "Служба: {Служба} {Событие}", _почта.Название, "Завершилась с ошибкой");

                // TODO: Обработка ошибки неуспешного запуска
            }

            // Перезапуск
            await Task.Delay(1000, токенОстановки);
        }
    }

    private static async Task
        ЖдатьЗапускПриложения(IHostApplicationLifetime lifetime, CancellationToken токенОстановки) {
        var tcs = new TaskCompletionSource();
        var cts = new CancellationTokenSource();

        using var регСтарт = lifetime.ApplicationStarted.Register(() => tcs.TrySetResult());
        using var регОстановка = токенОстановки.Register(() => tcs.TrySetCanceled(токенОстановки));

        await tcs.Task;
    }
}
