using System;
using System.Collections;
using System.Collections.Generic;

namespace NCoreUtils.Localization.Generator;

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
