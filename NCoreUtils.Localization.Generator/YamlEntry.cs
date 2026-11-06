namespace NCoreUtils.Localization.Generator;

/// <summary>
/// Single key of the parsed YAML file: either a string (<paramref name="Children" /> is <c>null</c>) or a nested
/// object. <paramref name="Value" /> is <c>null</c> for nested objects and for explicit YAML nulls.
/// </summary>
internal sealed record YamlEntry(string Key, string? Value, EquatableArray<YamlEntry>? Children, LocationInfo Location);
