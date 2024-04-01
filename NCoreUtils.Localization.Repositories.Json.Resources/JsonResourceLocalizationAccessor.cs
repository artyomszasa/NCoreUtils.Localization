using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Localization;

public class JsonResourceLocalizationAccessor : IJsonLocalizationAccessor
{
    private static Regex RegexI18nJson { get; } = new(
        "(((?<location>.*)\\.)?(?<basename>[^.]+)\\.)?(?<locale>[a-z]+|[a-z]+-[a-z]+)\\.i18n\\.json$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    );

    private static CultureInfo? GetCulture(string name)
    {
        try
        {
            return CultureInfo.GetCultureInfo(name);
        }
        catch (CultureNotFoundException)
        {
            return default;
        }
    }

    private Assembly Assembly { get; }

    public JsonResourceLocalizationAccessor(Assembly assembly)
        => Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));

    IAsyncEnumerable<LocalizationSourceEntry> IJsonLocalizationAccessor.EnumerateAsync(CancellationToken cancellationToken)
        => Enumerate().ToAsyncEnumerable();

    ValueTask<Stream> IJsonLocalizationAccessor.OpenAsync(LocalizationPath path, CultureInfo culture, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ValueTask<Stream>(Open(path, culture));
    }

    public IEnumerable<LocalizationSourceEntry> Enumerate()
    {
        foreach (var resourceName in Assembly.GetManifestResourceNames())
        {
            var m = RegexI18nJson.Match(resourceName);
            if (m.Success)
            {
                var culture = GetCulture(m.Groups["locale"].Value);
                if (culture is not null)
                {
                    yield return new(
                        new LocalizationPath(
                            m.Groups["location"].Value,
                            m.Groups["basename"].Value
                        ),
                        culture
                    );
                }
            }
        }
    }

    public Stream Open(LocalizationPath path, CultureInfo culture)
    {
        string name;
        if (string.IsNullOrEmpty(path.Location))
        {
            if (string.IsNullOrEmpty(path.BasePath))
            {
                name = $"{culture.Name}.i18n.json";
            }
            else
            {
                name = $"{path.BasePath}.{culture.Name}.i18n.json";
            }
        }
        else
        {
            if (string.IsNullOrEmpty(path.BasePath))
            {
                name = $"{path.Location}.{culture.Name}.i18n.json";
            }
            else
            {
                name = $"{path.Location}.{path.BasePath}.{culture.Name}.i18n.json";
            }
        }
        var stream = Assembly.GetManifestResourceStream(name);
        if (stream is null)
        {
            throw new InvalidOperationException($"No resource stream can be created for \"{name}\".");
        }
        return stream;
    }
}