using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace NCoreUtils.Localization.Generator;

internal enum PlaceholderType
{
    String,
    Int,
    Long,
    Decimal,
    Any,
    Date,
    Time,
    Timestamp
}

internal sealed record TemplateParameter(string Name, PlaceholderType Type)
{
    /// <summary>Name as a C# identifier.</summary>
    public string Identifier
        => SyntaxFacts.GetKeywordKind(Name) == SyntaxKind.None ? Name : "@" + Name;

    public string TypeName => Type switch
    {
        PlaceholderType.Int => "int",
        PlaceholderType.Long => "long",
        PlaceholderType.Decimal => "decimal",
        PlaceholderType.Any => "global::System.ISpanFormattable",
        PlaceholderType.Date => "global::System.DateOnly",
        PlaceholderType.Time => "global::System.TimeOnly",
        PlaceholderType.Timestamp => "global::System.DateTimeOffset",
        _ => "string"
    };

    public override string ToString()
        => $"{{{Name}:{Type.ToString().ToLowerInvariant()}}}";
}

/// <summary>
/// Value containing <c>{name}</c> or <c>{name:format}</c> placeholders. Literal braces are written as <c>{{</c> and
/// <c>}}</c>. Values without placeholders are plain strings and are returned verbatim.
/// </summary>
/// <param name="Segments">Literal text (<c>Parameter</c> is <c>null</c>) or a reference to a parameter.</param>
/// <param name="Parameters">Distinct parameters in order of their first appearance.</param>
internal sealed class ValueTemplate(
    IReadOnlyList<(string Text, TemplateParameter? Parameter)> segments,
    IReadOnlyList<TemplateParameter> parameters)
{
    public const string TimeZoneParameter = "tz";

    public const string FormatProviderParameter = "formatProvider";

    private const string Formats = "s/string, i/int, l/long, x/decimal, a/any, d/date, t/time, ts/timestamp";

    public IReadOnlyList<(string Text, TemplateParameter? Parameter)> Segments { get; } = segments;

    public IReadOnlyList<TemplateParameter> Parameters { get; } = parameters;

    public bool HasTimestamp => Parameters.Any(p => p.Type == PlaceholderType.Timestamp);

    /// <summary>
    /// Whether both templates require the same parameters (the order of the placeholders may differ).
    /// </summary>
    public static bool AreCompatible(ValueTemplate? a, ValueTemplate? b)
        => a is null || b is null
            ? a is null && b is null
            : a.Parameters.Count == b.Parameters.Count && a.Parameters.All(b.Parameters.Contains);

    public static string Describe(ValueTemplate? template)
        => template is null
            ? "a string without placeholders"
            : "a string with placeholders " + string.Join(", ", template.Parameters);

    /// <summary>
    /// Parses the value, returns <c>null</c> if the value contains no placeholders.
    /// </summary>
    /// <param name="value">Value to parse.</param>
    /// <param name="error">Set if the value contains placeholders but is not a valid template.</param>
    public static ValueTemplate? Parse(string value, out string? error)
    {
        error = null;
        if (value.IndexOf('{') < 0)
        {
            return null;
        }
        var segments = new List<(string, TemplateParameter?)>();
        var parameters = new List<TemplateParameter>();
        var literal = new StringBuilder();
        var hasPlaceholder = false;
        var i = 0;
        while (i < value.Length)
        {
            var ch = value[i];
            var next = i + 1 < value.Length ? value[i + 1] : '\0';
            if ((ch == '{' || ch == '}') && next == ch)
            {
                literal.Append(ch);
                i += 2;
                continue;
            }
            if (ch == '}')
            {
                error ??= "'}' has no matching '{' (use '}}' for a literal brace)";
                literal.Append(ch);
                ++i;
                continue;
            }
            if (ch != '{')
            {
                literal.Append(ch);
                ++i;
                continue;
            }
            var end = value.IndexOfAny(['{', '}'], i + 1);
            if (end < 0 || value[end] == '{')
            {
                error ??= "'{' is not closed (use '{{' for a literal brace)";
                literal.Append(ch);
                ++i;
                continue;
            }
            var content = value.Substring(i + 1, end - i - 1);
            var colon = content.IndexOf(':');
            var name = colon < 0 ? content : content.Substring(0, colon);
            if (!SyntaxFacts.IsValidIdentifier(name))
            {
                error ??= $"'{{{content}}}' is not a valid placeholder (use '{{{{' and '}}}}' for literal braces)";
                literal.Append(value, i, end - i + 1);
                i = end + 1;
                continue;
            }
            hasPlaceholder = true;
            i = end + 1;
            var format = colon < 0 ? null : content.Substring(colon + 1);
            if (!TryGetType(format, out var type))
            {
                error ??= $"'{format}' in '{{{content}}}' is not a valid format, expected one of {Formats}";
                continue;
            }
            if (name is TimeZoneParameter or FormatProviderParameter)
            {
                error ??= $"'{name}' is reserved for the generated method parameter";
                continue;
            }
            var parameter = parameters.Find(p => p.Name == name);
            if (parameter is null)
            {
                parameter = new(name, type);
                parameters.Add(parameter);
            }
            else if (parameter.Type != type)
            {
                error ??= $"placeholder '{name}' is used both as {parameter.Type.ToString().ToLowerInvariant()} and {type.ToString().ToLowerInvariant()}";
                continue;
            }
            if (literal.Length != 0)
            {
                segments.Add((literal.ToString(), null));
                literal.Clear();
            }
            segments.Add((name, parameter));
        }
        if (!hasPlaceholder)
        {
            error = null;
            return null;
        }
        if (literal.Length != 0)
        {
            segments.Add((literal.ToString(), null));
        }
        return error is null ? new(segments, parameters) : null;
    }

    private static bool TryGetType(string? format, out PlaceholderType type)
    {
        switch (format)
        {
            case null or "s" or "string":
                type = PlaceholderType.String;
                return true;
            case "i" or "int":
                type = PlaceholderType.Int;
                return true;
            case "l" or "long":
                type = PlaceholderType.Long;
                return true;
            case "x" or "decimal":
                type = PlaceholderType.Decimal;
                return true;
            case "a" or "any":
                type = PlaceholderType.Any;
                return true;
            case "d" or "date":
                type = PlaceholderType.Date;
                return true;
            case "t" or "time":
                type = PlaceholderType.Time;
                return true;
            case "ts" or "timestamp":
                type = PlaceholderType.Timestamp;
                return true;
            default:
                type = default;
                return false;
        }
    }
}
