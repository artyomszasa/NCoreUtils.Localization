using System;
using Microsoft.Extensions.Localization;

namespace NCoreUtils.Localization.Internal;

public class StringLocalizerFactory(ILocalizationSourceRepository repository) : IStringLocalizerFactory
{
    public ILocalizationSourceRepository Repository { get; } = repository ?? throw new ArgumentNullException(nameof(repository));

    public IStringLocalizer Create(Type resourceSource)
        => new StringLocalizer(Repository.GetSource(resourceSource));

    public IStringLocalizer Create(string baseName, string location)
        => new StringLocalizer(Repository.GetSource(baseName, location));
}