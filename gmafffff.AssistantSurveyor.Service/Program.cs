using gmafffff.AssistantSurveyor.FilePost;
using gmafffff.AssistantSurveyor.Service.Конфигурация;
using gmafffff.AssistantSurveyor.Service.ФоновыеСлужбы;

namespace gmafffff.AssistantSurveyor.Service;

public class Program {
    public static void Main(string[] args) {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.ДобавьПользовательскиеФайлыКонфигурации(builder.Configuration);

        builder.Services.ДобавьОпцииКонфигурации(builder.Configuration);
        builder.Services.AddSingleton<ФайловаяПочтаЕгрн>();
        builder.Services.AddHostedService<ПочтоваяСлужба<ФайловаяПочтаЕгрн>>();

        var host = builder.Build();
        host.Run();
    }
}