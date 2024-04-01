using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NCoreUtils.Localization;

public static class LocalizationSourceRepositoryBuilderJsonResourceExtensions
{
    public static LocalizationSourceRepositoryBuilder AddEmbeddedJson(
        this LocalizationSourceRepositoryBuilder builder,
        Assembly defaultAssembly,
        string? prefix = default)
    {
        builder.Services
            .AddOptions<JsonResourceLocalizationConfiguration>()
                .Configure(o =>
                {
                    o.DefaultAssembly = defaultAssembly;
                    o.Prefix = prefix;
                })
                .Services
            .AddTransient<IJsonResourceLocalizationConfiguration>( serviceProvider => serviceProvider
                .GetRequiredService<IOptionsMonitor<JsonResourceLocalizationConfiguration>>()
                .CurrentValue
            );
        return builder.AddRepository<JsonResourceLocalizationSourceRepository>();
    }
}