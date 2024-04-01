using System.Collections.Generic;

namespace NCoreUtils.Localization.Internal;

public sealed class NullEmptyStringComparer : IEqualityComparer<string>
{
    public static NullEmptyStringComparer Instance { get; } = new NullEmptyStringComparer();

    public bool Equals(string? x, string? y)
    {
        if (string.IsNullOrEmpty(x))
        {
            return string.IsNullOrEmpty(y);
        }
        if (string.IsNullOrEmpty(y))
        {
            return false;
        }
        return x == y;
    }

    public int GetHashCode(string? obj)
        => string.IsNullOrEmpty(obj) ? default : obj.GetHashCode();
}