using gmafffff.AssistantSurveyor.Post;
using gmafffff.AssistantSurveyor.Service.ФоновыеСлужбы;

namespace gmafffff.AssistantSurveyor.Service;

public class Program {
    public static void Main(string[] args) {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSingleton<ПочтаЕгрн>();
        builder.Services.AddHostedService<ПочтоваяСлужба<ПочтаЕгрн>>();

        var host = builder.Build();
        host.Run();
    }
}