using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace NCoreUtils.Localization.Generator;

/// <summary>
/// Serializable counterpart of <see cref="Location" /> (locations must not be held by pipeline values).
/// </summary>
internal sealed record LocationInfo(string Path, TextSpan Span, LinePositionSpan LineSpan)
{
    public static LocationInfo? From(Location location)
        => location.SourceTree is null
            ? null
            : new(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);

    public Location ToLocation()
        => Location.Create(Path, Span, LineSpan);
}
