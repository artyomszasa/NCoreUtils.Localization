using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Collections;

namespace NCoreUtils.Localization;

public interface IJsonLocalizationAccessor
{
    IAsyncEnumerable<LocalizationSourceEntry> EnumerateAsync(CancellationToken cancellationToken = default);

    ValueTask<Stream> OpenAsync(
        LocalizationPath path,
        CultureInfo culture,
        CancellationToken cancellationToken = default
    );

    /*
    async ValueTask<IReadOnlyDictionary<LocalizationPath, IReadOnlyList<CultureInfo>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var map = new MultiValueDictionary<LocalizationPath, CultureInfo>();
        await foreach (var sourceEntry in EnumerateAsync(cancellationToken))
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
    */
}