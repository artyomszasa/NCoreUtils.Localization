using System.Globalization;

namespace NCoreUtils.Localization;

public readonly struct LocalizationSourceEntry(LocalizationPath path, CultureInfo? culture)
{
    public LocalizationPath Path { get; } = path;

    public CultureInfo? Culture { get; } = culture;
}