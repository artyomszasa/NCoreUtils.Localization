using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace NCoreUtils.Localization.Internal;

public class StringLocalizer : IStringLocalizer
{
    private CultureInfo? Culture { get; }

    internal ILocalizationSource Source { get; }

    public LocalizedString this[string key]
        => TryGet(new CaseInsensitive(key), out var result)
            ? new LocalizedString(key, result, false)
            : new LocalizedString(key, key, true);

    public LocalizedString this[string key, params object[] arguments]
        => TryGet(new CaseInsensitive(key), out var result)
            ? new LocalizedString(key, string.Format(result, arguments), false)
            : new LocalizedString(key, string.Format(key, arguments), true);

    public StringLocalizer(ILocalizationSource source, CultureInfo? culture = default)
    {
        Culture = culture;
        Source = source;
    }

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
        return Enumerable.Empty<LocalizedString>();
    }

#if !NET6_0

    IStringLocalizer IStringLocalizer.WithCulture(CultureInfo culture)
        => new StringLocalizer(Source, culture);

#endif
}

public class StringLocalizer<T> : StringLocalizer, IStringLocalizer<T>
{
    public StringLocalizer(StringLocalizerFactory factory, CultureInfo? culture = default)
        : base(factory.Repository.GetSource(typeof(T)), culture)
    { }
}