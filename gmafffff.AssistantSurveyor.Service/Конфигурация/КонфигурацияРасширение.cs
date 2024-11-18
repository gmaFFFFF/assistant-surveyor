using gmafffff.AssistantSurveyor.FilePost.Конфигурация;
using Microsoft.Extensions.Options;

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
}