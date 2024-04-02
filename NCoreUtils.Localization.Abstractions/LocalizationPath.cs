using System;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Localization.Internal;

namespace NCoreUtils.Localization;

public readonly struct LocalizationPath(string? location, string? basePath) : IEquatable<LocalizationPath>
{
    public static bool operator ==(LocalizationPath left, LocalizationPath right)
        => left.Equals(right);

    public static bool operator !=(LocalizationPath left, LocalizationPath right)
        => !(left == right);

    public string? Location { get; } = location;

    public string? BasePath { get; } = basePath;

    public bool IsEmpty => string.IsNullOrEmpty(Location) && string.IsNullOrEmpty(BasePath);

    public bool Equals(LocalizationPath other)
        => NullEmptyStringComparer.Instance.Equals(Location, other.Location)
            && NullEmptyStringComparer.Instance.Equals(BasePath, other.BasePath);

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is LocalizationPath other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(
            NullEmptyStringComparer.Instance.GetHashCode(Location),
            NullEmptyStringComparer.Instance.GetHashCode(BasePath)
        );
}