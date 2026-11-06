using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace NCoreUtils.Localization.Generator
{
    /// <summary>
    /// Immutable array with value equality, required to keep the incremental pipeline cacheable.
    /// </summary>
    internal readonly struct EquatableArray<T>(T[] items) : IEquatable<EquatableArray<T>>, IEnumerable<T>
        where T : IEquatable<T>
    {
        private readonly T[]? items = items;

        public static EquatableArray<T> Empty => new([]);

        public int Length => items?.Length ?? 0;

        public bool Equals(EquatableArray<T> other)
            => ((ReadOnlySpan<T>)items).SequenceEqual(other.items);

        public override bool Equals(object? obj)
            => obj is EquatableArray<T> other && Equals(other);

        public override int GetHashCode()
        {
            var hash = 17;
            foreach (var item in this)
            {
                hash = hash * 31 + (item?.GetHashCode() ?? 0);
            }
            return hash;
        }

        public IEnumerator<T> GetEnumerator()
            => ((IEnumerable<T>)(items ?? [])).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

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

    internal sealed record DiagnosticInfo(DiagnosticDescriptor Descriptor, LocationInfo? Location, EquatableArray<string> Arguments)
    {
        public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, LocationInfo? location, params string[] arguments)
            => new(descriptor, location, new(arguments));

        public Diagnostic ToDiagnostic()
        {
            var arguments = new List<object>();
            foreach (var argument in Arguments)
            {
                arguments.Add(argument);
            }
            return Diagnostic.Create(Descriptor, Location?.ToLocation(), [.. arguments]);
        }
    }

    /// <summary>
    /// Single key of the parsed YAML file: either a string (<paramref name="Children" /> is <c>null</c>) or a nested
    /// object. <paramref name="Value" /> is <c>null</c> for nested objects and for explicit YAML nulls.
    /// </summary>
    internal sealed record YamlEntry(string Key, string? Value, EquatableArray<YamlEntry>? Children, LocationInfo Location);

    /// <param name="Path">Full path of the file.</param>
    /// <param name="Stem">File name without the <c>.{locale}.i18n.yaml</c> part.</param>
    /// <param name="Locale">Normalized locale, <c>null</c> if the file name has no valid locale.</param>
    /// <param name="Entries">Root keys of the file.</param>
    /// <param name="Diagnostics">Issues found while parsing the file.</param>
    /// <param name="HasErrors">Whether parsing failed, <paramref name="Entries" /> are incomplete in that case.</param>
    internal sealed record LocalizationFile(
        string Path,
        string Stem,
        string? Locale,
        EquatableArray<YamlEntry> Entries,
        EquatableArray<DiagnosticInfo> Diagnostics,
        bool HasErrors)
    {
        public bool MatchesSuffix(string suffix)
            => Locale is not null
                && (suffix.Length == 0 || Stem.EndsWith("." + suffix, StringComparison.Ordinal));
    }

    /// <summary>
    /// Interface annotated with the GeneratedLocalizerAttribute.
    /// </summary>
    /// <param name="Namespace">Containing namespace, <c>null</c> for the global namespace.</param>
    /// <param name="Name">Name of the interface.</param>
    /// <param name="Accessibility">Accessibility keyword of the interface.</param>
    /// <param name="Suffix">Value of the suffix attribute argument.</param>
    /// <param name="Location">Location of the interface name.</param>
    /// <param name="Error">Set when no code can be generated for the interface.</param>
    internal sealed record LocalizerTarget(
        string? Namespace,
        string Name,
        string Accessibility,
        string Suffix,
        LocationInfo? Location,
        DiagnosticInfo? Error);
}
