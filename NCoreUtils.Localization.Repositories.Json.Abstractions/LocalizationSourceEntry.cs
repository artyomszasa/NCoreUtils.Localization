using System.Globalization;

namespace NCoreUtils.Localization;

public readonly struct LocalizationSourceEntry
{
    public LocalizationPath Path { get; }

    public CultureInfo? Culture { get; }

    public LocalizationSourceEntry(LocalizationPath path, CultureInfo? culture)
    {
        Path = path;
        Culture = culture;
    }
}