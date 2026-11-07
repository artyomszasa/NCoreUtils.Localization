namespace NCoreUtils.Localization.Generator;

/// <summary>
/// Interface annotated with the GeneratedLocalizerAttribute.
/// </summary>
/// <param name="Namespace">Containing namespace, <c>null</c> for the global namespace.</param>
/// <param name="Name">Name of the interface.</param>
/// <param name="Accessibility">Accessibility keyword of the interface.</param>
/// <param name="Suffix">Value of the suffix attribute argument.</param>
/// <param name="ForceProperties">Properties to generate even if no localization file defines them.</param>
/// <param name="Location">Location of the interface name.</param>
/// <param name="Error">Set when no code can be generated for the interface.</param>
internal sealed record LocalizerTarget(
    string? Namespace,
    string Name,
    string Accessibility,
    string Suffix,
    EquatableArray<string> ForceProperties,
    LocationInfo? Location,
    DiagnosticInfo? Error
);
