using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Localization;

public partial class JsonLocalizationSource : IAsyncDisposable, IDisposable, INamedLocalizationSource
{
    private int _isDisposed;

    private IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>? _entries;

    string? INamedLocalizationSource.Location => Path.Location;

    string? INamedLocalizationSource.BasePath => Path.BasePath;

    private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

    private Task<IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>> LazyEntries { get; }

    private IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>> Entries
        => _entries ??= LazyEntries.GetAwaiter().GetResult();

    public LocalizationPath Path { get; }

    public IReadOnlyList<CultureInfo> Cultures { get; }

    public ILogger<JsonLocalizationSource> Logger { get; }

    public Func<CultureInfo, CancellationToken, ValueTask<Stream>> StreamFactory { get; }

    public IEnumerable<CultureInfo> Keys => Entries.Keys;

    public IEnumerable<IReadOnlyDictionary<CaseInsensitive, string>> Values
        => Entries.Values;

    public int Count => Entries.Count;

    public IReadOnlyDictionary<CaseInsensitive, string> this[CultureInfo key]
        => Entries[key];

    public JsonLocalizationSource(
        LocalizationPath path,
        IReadOnlyList<CultureInfo> cultures,
        ILogger<JsonLocalizationSource> logger,
        Func<CultureInfo, CancellationToken, ValueTask<Stream>> streamFactory)
    {
        Path = path;
        Cultures = cultures ?? throw new ArgumentNullException(nameof(cultures));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        StreamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        LazyEntries = Task.Run(() => PrepareEntriesAsync(logger, cultures, streamFactory, CancellationTokenSource.Token));
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public bool ContainsKey(CultureInfo key)
        => Entries.ContainsKey(key);

#if NETSTANDARD2_1
    public bool TryGetValue(CultureInfo key, [NotNullWhen(true)] out IReadOnlyDictionary<CaseInsensitive, string> value)
#else
    public bool TryGetValue(CultureInfo key, [MaybeNullWhen(false)] out IReadOnlyDictionary<CaseInsensitive, string> value)
#endif
    {
        if (Entries.TryGetValue(key, out var data))
        {

            value = data;
            return true;
        }
#if NETSTANDARD2_1
        value = default!;
#else
        value = default;
#endif
        return false;
    }

    public IEnumerator<KeyValuePair<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>> GetEnumerator()
    {
        foreach (var kv in Entries)
        {
            yield return new KeyValuePair<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>(
                kv.Key,
                kv.Value
            );
        }
    }

    #region disposable

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            if (disposing)
            {
                CancellationTokenSource.Cancel();
                try
                {
                    if (!LazyEntries.Wait(TimeSpan.FromMilliseconds(100)))
                    {
                        Logger.LogGenerationDidNotEndWithin100Ms();
                    }
                }
                catch { /* noop */ }
                CancellationTokenSource.Dispose();
            }
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            CancellationTokenSource.Cancel();
            try { await LazyEntries; } catch { /* noop */ }
            CancellationTokenSource.Dispose();
        }
    }

    #endregion
}