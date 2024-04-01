using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using NCoreUtils.Localization.Internal;

namespace NCoreUtils.Localization;

public static class ServiceCollectionLocalizationExtensions
{
    public static IServiceCollection AddSourceBasedLocalization(
        this IServiceCollection services,
        Action<LocalizationSourceRepositoryBuilder> init)
    {
        var builder = new LocalizationSourceRepositoryBuilder(services);
        init(builder);
        services.AddSingleton(typeof(ILocalizationSourceRepository), builder.GetRepositoryType());
        services.TryAddSingleton<StringLocalizerFactory>();
        services.TryAddSingleton<StringLocalizerFactory>();
        services.TryAddTransient<IStringLocalizerFactory>(serviceProvider => serviceProvider.GetRequiredService<StringLocalizerFactory>());
        services.TryAddTransient(typeof(IStringLocalizer<>), typeof(Internal.StringLocalizer<>));
        return services;
    }
}