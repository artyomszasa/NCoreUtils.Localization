using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.Localization.Serialization;

namespace NCoreUtils.Localization;

public partial class JsonLocalizationSource
{
    private static JsonTypeInfo<Map> MapTypeInfo { get; } = MapSerializerContext.Default.Map;

    private static async ValueTask<IReadOnlyDictionary<CaseInsensitive, string>> ReadAsync(
        CultureInfo culture,
        Func<CultureInfo, CancellationToken, ValueTask<Stream>> streamFactory,
        CancellationToken cancellationToken)
    {
        await using var stream = await streamFactory(culture, cancellationToken);
        return (await JsonSerializer.DeserializeAsync(stream, MapTypeInfo, cancellationToken))!;
    }

    private static async Task<IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>> PrepareEntriesAsync(
        ILogger logger,
        IReadOnlyList<CultureInfo> cultures,
        Func<CultureInfo, CancellationToken, ValueTask<Stream>> streamFactory,
        CancellationToken cancellationToken)
    {
        var stopwatch0 = new Stopwatch();
        stopwatch0.Start();
        var entries = new Dictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>();
        foreach (var culture in cultures)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            IReadOnlyDictionary<CaseInsensitive, string> data;
            try
            {
                data = await ReadAsync(culture, streamFactory, cancellationToken);
                if (entries.ContainsKey(culture))
                {
                    logger.LogFailedToReadLocalizationData(culture.Name);
                }
                entries[culture] = data;
                stopwatch.Stop();
                logger.LogReadingLocalizationDataSuccess(culture.Name, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception exn)
            {
                logger.LogOverwritingLocalizationData(exn, culture.Name);
            }
        }
        stopwatch0.Stop();
        logger.LogReadingLocalizationEntriesSuccess(stopwatch0.ElapsedMilliseconds);
        return entries;
    }
        // => cultures.ToDictionary(culture => culture, culture => ReadAsync(culture, streamFactory, cancellationToken));
}