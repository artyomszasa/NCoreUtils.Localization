using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.Collections;

namespace NCoreUtils.Localization;

public class JsonResourceLocalizationSourceRepository(
    IJsonResourceLocalizationConfiguration configuration,
    ILoggerFactory loggerFactory)
    : ILocalizationSourceRepository
    , IDisposable
    , IAsyncDisposable
{
    private static IReadOnlyDictionary<LocalizationPath, IReadOnlyList<CultureInfo>> GetAll(JsonResourceLocalizationAccessor accessor)
    {
        var map = new MultiValueDictionary<LocalizationPath, CultureInfo>();
        foreach (var sourceEntry in accessor.Enumerate())
        {
            if (sourceEntry.Culture is not null)
            {
                map.Add(sourceEntry.Path, sourceEntry.Culture);
            }
        }
        var res = new Dictionary<LocalizationPath, IReadOnlyList<CultureInfo>>();
        foreach (var key in map.Keys)
        {
            var l = new List<CultureInfo>();
            if (map.TryGetValues(key, l))
            {
                res.Add(key, l);
            }
        }
        return res;
    }

    private int _isDisposed;

    private ConcurrentDictionary<Assembly, JsonResourceLocalizationAccessor> AccessorCache { get; } = new();

    private ConcurrentDictionary<JsonResourceLocalizationAccessor, IReadOnlyDictionary<LocalizationPath, IReadOnlyList<CultureInfo>>> EntryCache { get; } = new();

    private ConcurrentDictionary<(Assembly, LocalizationPath), JsonLocalizationSource> SourceCache { get; } = new();

    private Func<Assembly, JsonResourceLocalizationAccessor> AccessorFactory { get; }
        = assembly => new JsonResourceLocalizationAccessor(assembly);

    private Func<JsonResourceLocalizationAccessor, IReadOnlyDictionary<LocalizationPath, IReadOnlyList<CultureInfo>>> EntryFactory { get; }
        = GetAll;

    public IJsonResourceLocalizationConfiguration Configuration { get; } = configuration;

    public ILoggerFactory LoggerFactory { get; } = loggerFactory;

    private JsonResourceLocalizationAccessor GetAccessor(Assembly assembly)
        => AccessorCache.GetOrAdd(assembly, AccessorFactory);

    private IReadOnlyDictionary<LocalizationPath, IReadOnlyList<CultureInfo>> GetEntries(JsonResourceLocalizationAccessor accessor)
        => EntryCache.GetOrAdd(accessor, EntryFactory);

    private ILocalizationSource GetSource(Assembly assembly, LocalizationPath path)
    {
        if (SourceCache.TryGetValue((assembly, path), out var source))
        {
            return source;
        }
        var accessor = GetAccessor(assembly);
        var entries = GetEntries(accessor);
        var cultures = entries.GetValueOrDefault(path, []);
        source = new JsonLocalizationSource(
            path,
            cultures,
            LoggerFactory.CreateLogger<JsonLocalizationSource>(),
            (culture, _) => new ValueTask<Stream>(accessor.Open(path, culture))
        );
        SourceCache.TryAdd((assembly, path), source);
        return source;
    }

    public ILocalizationSource GetSource(Type type)
    {
        var (location, baseName) = type.GetCustomAttribute<OverrideLocalizationPathAttribute>() switch
        {
            null => (type.Namespace, type.Name),
            var attr => (attr.Location ?? type.Namespace, attr.BasePath ?? type.Name)
        };
        var path = new LocalizationPath(
            string.IsNullOrEmpty(Configuration.Prefix)
                ? location
                : $"{location}.{Configuration.Prefix}",
            baseName
        );
        return GetSource(type.Assembly, path);
    }

    public ILocalizationSource GetSource(string? baseName, string? location)
        => GetSource(Configuration.DefaultAssembly, new LocalizationPath(location, baseName));

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
                foreach (var source in SourceCache.Values)
                {
                    source.Dispose();
                }
            }
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            foreach (var source in SourceCache.Values)
            {
                await source.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    #endregion
}