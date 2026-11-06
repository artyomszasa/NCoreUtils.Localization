using System;

namespace NCoreUtils.Localization.Generator;

/// <param name="Path">Full path of the file.</param>
/// <param name="Stem">File name without the <c>.{locale}.i18n.yaml</c> part.</param>
/// <param name="Locale">Normalized locale, <c>null</c> if the file name has no valid locale.</param>
/// <param name="Entries">Root keys of the file.</param>
/// <param name="Diagnostics">Issues found while parsing the file.</param>
/// <param name="HasErrors">Whether parsing failed, <paramref name="Entries" /> are incomplete in that case.</param>
internal sealed record LocalizationFile(
    string Path,
    string Stem,
    string? Locale,
    EquatableArray<YamlEntry> Entries,
    EquatableArray<DiagnosticInfo> Diagnostics,
    bool HasErrors)
{
    public bool MatchesSuffix(string suffix)
        => Locale is not null
            && (suffix.Length == 0 || Stem.EndsWith("." + suffix, StringComparison.Ordinal));
}
