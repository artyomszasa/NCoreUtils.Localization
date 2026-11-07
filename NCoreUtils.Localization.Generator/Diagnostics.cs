using Microsoft.CodeAnalysis;

namespace NCoreUtils.Localization.Generator;

internal static class Diagnostics
{
    private const string Category = "NCoreUtils.Localization";

    public static readonly DiagnosticDescriptor InvalidTarget = Error(
        "LOC001",
        "Invalid localizer interface",
        "Interface '{0}' cannot be used with GeneratedLocalizerAttribute: it must be a partial, non-generic, top-level interface");

    public static readonly DiagnosticDescriptor InvalidFileName = new(
        "LOC002",
        "Localization file ignored",
        "File '{0}' is ignored as its name does not match '*.{{locale}}.i18n.yaml' with a valid locale",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidYaml = Error(
        "LOC003",
        "Invalid YAML",
        "File '{0}' is not a valid YAML file: {1}");

    public static readonly DiagnosticDescriptor UnsupportedStructure = Error(
        "LOC004",
        "Unsupported localization file structure",
        "File '{0}' cannot be used for localization: {1}");

    public static readonly DiagnosticDescriptor MergeConflict = Error(
        "LOC005",
        "Localization files cannot be merged",
        "Localization files of '{0}' cannot be merged: key '{1}' is {2} in '{3}' (line {4}) but {5} in '{6}' (line {7})");

    public static readonly DiagnosticDescriptor InvalidKey = Error(
        "LOC006",
        "Invalid localization key",
        "Key '{0}' in '{1}' cannot be used by '{2}': {3}");

    public static readonly DiagnosticDescriptor TypeNameCollision = Error(
        "LOC007",
        "Generated type name collision",
        "Keys '{0}' and '{1}' of '{2}' both require the generated interface '{3}'");

    public static readonly DiagnosticDescriptor InvalidPlaceholder = Error(
        "LOC009",
        "Invalid placeholder",
        "Value of the key '{0}' in '{1}' cannot be used by '{2}': {3}");

    public static readonly DiagnosticDescriptor NoMatchingFiles = new(
        "LOC008",
        "No localization files found",
        "None of the files passed as AdditionalFiles matches '{0}' for interface '{1}', all properties will return their names",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static DiagnosticDescriptor Error(string id, string title, string messageFormat)
        => new(id, title, messageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
}