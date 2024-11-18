using gmafffff.AssistantSurveyor.FilePost;
using gmafffff.AssistantSurveyor.Service.Конфигурация;
using gmafffff.AssistantSurveyor.Service.ФоновыеСлужбы;
using Serilog;

namespace gmafffff.AssistantSurveyor.Service;

public class Program {
    public static void Main(string[] args) {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.ДобавьПользовательскиеФайлыКонфигурации(builder.Configuration);

        builder.Services.ДобавьОпцииКонфигурации();
        builder.Services.AddSingleton<ФайловаяПочтаЕгрн>();
        builder.Services.AddHostedService<ПочтоваяСлужба<ФайловаяПочтаЕгрн>>();
        builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        var host = builder.Build();
        host.Run();
    }
}