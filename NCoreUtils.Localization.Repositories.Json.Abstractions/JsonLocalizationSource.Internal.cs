using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Localization.Serialization;

namespace NCoreUtils.Localization;

public partial class JsonLocalizationSource
{
    private static JsonTypeInfo<Map> MapTypeInfo { get; } = MapSerializerContext.Default.Map;

    private static async Task<IReadOnlyDictionary<CaseInsensitive, string>> ReadAsync(
        CultureInfo culture,
        Func<CultureInfo, CancellationToken, ValueTask<Stream>> streamFactory,
        CancellationToken cancellationToken)
    {
        await using var stream = await streamFactory(culture, cancellationToken);
        return (await JsonSerializer.DeserializeAsync(stream, MapTypeInfo, cancellationToken))!;
    }

    private static IReadOnlyDictionary<CultureInfo, Task<IReadOnlyDictionary<CaseInsensitive, string>>> PrepareEntries(
        IReadOnlyList<CultureInfo> cultures,
        Func<CultureInfo, CancellationToken, ValueTask<Stream>> streamFactory,
        CancellationToken cancellationToken)
        => cultures.ToDictionary(culture => culture, culture => ReadAsync(culture, streamFactory, cancellationToken));
}