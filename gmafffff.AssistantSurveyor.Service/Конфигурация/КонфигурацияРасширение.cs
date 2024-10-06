using gmafffff.AssistantSurveyor.FilePost.Конфигурация;

namespace gmafffff.AssistantSurveyor.Service.Конфигурация;

public static class КонфигурацияРасширение {
    public static IServiceCollection ДобавьОпцииКонфигурации(this IServiceCollection службы, IConfiguration конфиг) {
        службы.Configure<ОбщиеОпции>(конфиг.GetSection(ОбщиеОпции.Секция));
        службы.Configure<ПочтаФайловаяОпции>(конфиг.GetSection(ПочтаФайловаяОпции.Секция));

        return службы;
    }

    public static ConfigurationManager ДобавьПользовательскиеФайлыКонфигурации(this ConfigurationManager конфигурация,
        IConfiguration конфиг) {
        var общиеОпции = конфиг.GetSection(ОбщиеОпции.Секция).Get<ОбщиеОпции>() ?? new ОбщиеОпции();
        foreach (var конфигФайл in общиеОпции.ДайКонфигФайлы())
            конфигурация.AddJsonFile(конфигФайл, true, true);
        return конфигурация;
    }
}