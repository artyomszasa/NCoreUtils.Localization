using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Localization;

namespace NCoreUtils.Localization.Internal;

public class StringLocalizer(ILocalizationSource source, CultureInfo? culture = default) : IStringLocalizer
{
    private static readonly ConditionalWeakTable<IReadOnlyDictionary<CaseInsensitive, string>, LookupDictionary<CaseInsensitive, LocalizedString>> looseCache = [];

    private CultureInfo? Culture { get; } = culture;

    private LookupDictionary<CaseInsensitive, LocalizedString> Cache { get; } = looseCache.GetOrCreateValue(source);

    internal ILocalizationSource Source { get; } = source;

    public LocalizedString this[string key]
        => Cache.GetOrAdd(new CaseInsensitive(key), this, static (ciKey, self) =>
        {
            var uKey = ciKey.ToUpperString();
            return self.TryGet(ciKey, out var result)
                ? new LocalizedString(uKey, result, false)
                : new LocalizedString(uKey, uKey, true);
        });

    public LocalizedString this[string key, params object[] arguments]
        => TryGet(new CaseInsensitive(key), out var result)
            ? new LocalizedString(key, string.Format(result, arguments), false)
            : new LocalizedString(key, string.Format(key, arguments), true);

    private bool TryGetLocalizations([MaybeNullWhen(false)] out IReadOnlyDictionary<CaseInsensitive, string> localizations)
        => Source.TryGetValue(Culture ?? CultureInfo.CurrentCulture, out localizations);

    private bool TryGet(CaseInsensitive key, [MaybeNullWhen(false)] out string result)
    {
        if (TryGetLocalizations(out var localizations))
        {
            return localizations.TryGetValue(key, out result);
        }
        result = default;
        return false;
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        if (TryGetLocalizations(out var localizations))
        {
            return localizations.Select(kv => new LocalizedString(kv.Key.Value, kv.Value));
        }
        return [];
    }

#if !NET6_0_OR_GREATER

    IStringLocalizer IStringLocalizer.WithCulture(CultureInfo culture)
        => new StringLocalizer(Source, culture);

#endif
}

public class StringLocalizer<T>(StringLocalizerFactory factory, CultureInfo? culture = default)
    : StringLocalizer(factory.Repository.GetSource(typeof(T)), culture), IStringLocalizer<T>
{ }