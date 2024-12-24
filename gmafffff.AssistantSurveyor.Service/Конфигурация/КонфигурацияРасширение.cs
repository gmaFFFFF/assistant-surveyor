using Akka.Hosting;
using gmafffff.AssistantSurveyor.FilePost.Конфигурация;
using Microsoft.Extensions.Options;
using Serilog;

namespace gmafffff.AssistantSurveyor.Service.Конфигурация;

public static class КонфигурацияРасширение {
    public static IServiceCollection ДобавьОпцииКонфигурации(this IServiceCollection службы) {
        службы
            .AddOptions<ОбщиеКонфиг>()
            .BindConfiguration(ОбщиеКонфиг.Секция);
        службы
            .AddOptions<ПочтаФайловаяКонфиг>()
            .BindConfiguration(ПочтаФайловаяКонфиг.Секция);

        службы.AddTransient<ОбщиеКонфиг>(static службы =>
            службы.GetRequiredService<IOptionsMonitor<ОбщиеКонфиг>>().CurrentValue
        );

        службы.AddTransient<ПочтаФайловаяКонфиг>(static службы =>
            службы.GetRequiredService<IOptionsMonitor<ПочтаФайловаяКонфиг>>().CurrentValue
        );

        return службы;
    }

    public static ConfigurationManager ДобавьПользовательскиеФайлыКонфигурации(this ConfigurationManager конфигурация,
        IConfiguration конфиг) {
        var общиеОпции = конфиг.GetSection(ОбщиеКонфиг.Секция).Get<ОбщиеКонфиг>() ?? new ОбщиеКонфиг();
        foreach (var конфигФайл in общиеОпции.ДайКонфигФайлы())
            конфигурация.AddJsonFile(конфигФайл, true, true);
        return конфигурация;
    }


    public static IServiceCollection ДобавьSerilog(this IServiceCollection службы) {
        службы.AddSerilog((services, loggerConfiguration) => {
            var конфиг = services.GetRequiredService<IConfiguration>();
            loggerConfiguration
                .ReadFrom.Configuration(конфиг)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();
        });
        return службы;
    }

    public static IServiceCollection ДобавьСистемуАкторов(this IServiceCollection службы) {
        службы.AddAkka("AssistantSurveyor", (конфиг, services) => {
            var секцияAkka = services.GetRequiredService<IConfiguration>().GetSection("Akka");
            конфиг.AddHocon(секцияAkka, HoconAddMode.Prepend);
            конфиг.WithActors((system, registry, resolver) => {
                // var props = resolver.Props<HelloActor>();
                // var helloActor = system.ActorOf(props, "hello-actor");
                // registry.Register<HelloActor>(helloActor);
            });
        });
        return службы;
    }
}