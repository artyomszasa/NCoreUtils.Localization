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

    string? INamedLocalizationSource.Location => Path.Location;

    string? INamedLocalizationSource.BasePath => Path.BasePath;

    private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

    private IReadOnlyDictionary<CultureInfo, Task<IReadOnlyDictionary<CaseInsensitive, string>>> LazyEntries { get; }

    public LocalizationPath Path { get; }

    public IReadOnlyList<CultureInfo> Cultures { get; }

    public ILogger<JsonLocalizationSource> Logger { get; }

    public Func<CultureInfo, CancellationToken, ValueTask<Stream>> StreamFactory { get; }

    public IEnumerable<CultureInfo> Keys => LazyEntries.Keys;

    public IEnumerable<IReadOnlyDictionary<CaseInsensitive, string>> Values
        => Task.WhenAll(LazyEntries.Values).GetAwaiter().GetResult();

    public int Count => LazyEntries.Count;

    public IReadOnlyDictionary<CaseInsensitive, string> this[CultureInfo key]
        => LazyEntries[key].GetAwaiter().GetResult();

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
        LazyEntries = PrepareEntries(cultures, streamFactory, CancellationTokenSource.Token);
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public bool ContainsKey(CultureInfo key)
        => LazyEntries.ContainsKey(key);

#if NETSTANDARD2_1
    public bool TryGetValue(CultureInfo key, [NotNullWhen(true)] out IReadOnlyDictionary<CaseInsensitive, string> value)
#else
    public bool TryGetValue(CultureInfo key, [MaybeNullWhen(false)] out IReadOnlyDictionary<CaseInsensitive, string> value)
#endif
    {
        if (LazyEntries.TryGetValue(key, out var lazyEntry))
        {

            value = lazyEntry.GetAwaiter().GetResult();
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
        foreach (var kv in LazyEntries)
        {
            yield return new KeyValuePair<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>(
                kv.Key,
                kv.Value.GetAwaiter().GetResult()
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
                if (!Task.WhenAll(LazyEntries.Values).Wait(TimeSpan.FromMilliseconds(100)))
                {
                    Logger.LogWarning("Lazy entries generation did not end within 100 ms after cancellation.");
                }
                CancellationTokenSource.Dispose();
            }
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            CancellationTokenSource.Cancel();
            await Task.WhenAll(LazyEntries.Values);
            CancellationTokenSource.Dispose();
        }
    }

    #endregion
}