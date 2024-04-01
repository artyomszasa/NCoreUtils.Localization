using System;

namespace NCoreUtils.Localization;

public interface ILocalizationSourceRepository
{
    ILocalizationSource GetSource(Type type);

    ILocalizationSource GetSource(string? baseName, string? location);
}