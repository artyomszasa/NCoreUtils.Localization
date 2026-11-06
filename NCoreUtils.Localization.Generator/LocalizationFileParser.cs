using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace NCoreUtils.Localization.Generator;

internal static class LocalizationFileParser
{
    public const string Extension = ".i18n.yaml";

    /// <summary>Guards against recursive YAML aliases.</summary>
    private const int MaxDepth = 64;

    private static readonly Regex LocaleRegex = new(
        "^[a-z]{2,3}(-[a-z]{4})?(-([a-z]{2}|[0-9]{3}))?(-([a-z0-9]{5,8}|[0-9][a-z0-9]{3}))*$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private sealed class InvalidStructureException(string message, Mark start, Mark end) : Exception(message)
    {
        public Mark Start { get; } = start;

        public Mark End { get; } = end;
    }

    public static LocalizationFile Parse(AdditionalText file, CancellationToken cancellationToken)
    {
        var path = file.Path;
        var name = Path.GetFileName(path);
        if (!TryParseFileName(name, out var stem, out var locale))
        {
            var warning = DiagnosticInfo.Create(
                Diagnostics.InvalidFileName,
                new LocationInfo(path, default, default),
                name);
            return new(path, name, null, EquatableArray<YamlEntry>.Empty, new([warning]), HasErrors: false);
        }
        try
        {
            var text = file.GetText(cancellationToken)?.ToString() ?? string.Empty;
            var stream = new YamlStream();
            stream.Load(new StringReader(text));
            var entries = new List<YamlEntry>();
            foreach (var document in stream.Documents)
            {
                switch (document.RootNode)
                {
                    case YamlMappingNode mapping:
                        entries.AddRange(ReadMapping(mapping, path, 0));
                        break;
                    case YamlScalarNode scalar when IsNull(scalar):
                        break;
                    default:
                        throw new InvalidStructureException(
                            "the document root must be an object",
                            document.RootNode.Start,
                            document.RootNode.End);
                }
            }
            return new(path, stem, locale, new([.. entries]), EquatableArray<DiagnosticInfo>.Empty, HasErrors: false);
        }
        catch (InvalidStructureException exn)
        {
            return Failed(Diagnostics.UnsupportedStructure, exn.Message, exn.Start, exn.End);
        }
        catch (YamlException exn)
        {
            return Failed(Diagnostics.InvalidYaml, exn.Message, exn.Start, exn.End);
        }

        LocalizationFile Failed(DiagnosticDescriptor descriptor, string message, Mark start, Mark end)
        {
            var error = DiagnosticInfo.Create(descriptor, ToLocation(path, start, end), name, message);
            return new(path, stem, locale, EquatableArray<YamlEntry>.Empty, new([error]), HasErrors: true);
        }
    }

    private static IEnumerable<YamlEntry> ReadMapping(YamlMappingNode mapping, string path, int depth)
    {
        if (depth > MaxDepth)
        {
            throw new InvalidStructureException(
                $"nesting is deeper than {MaxDepth} levels (recursive alias?)",
                mapping.Start,
                mapping.End);
        }
        foreach (var pair in mapping.Children)
        {
            if (pair.Key is not YamlScalarNode { Value: not null } key)
            {
                throw new InvalidStructureException("keys must be strings", pair.Key.Start, pair.Key.End);
            }
            var location = ToLocation(path, key.Start, key.End);
            yield return pair.Value switch
            {
                YamlScalarNode scalar => new(key.Value, IsNull(scalar) ? null : scalar.Value, null, location),
                YamlMappingNode nested => new(key.Value, null, new([.. ReadMapping(nested, path, depth + 1)]), location),
                _ => throw new InvalidStructureException(
                    $"value of the key '{key.Value}' must be either a string or an object",
                    pair.Value.Start,
                    pair.Value.End)
            };
        }
    }

    private static bool IsNull(YamlScalarNode scalar)
        => scalar.Style is ScalarStyle.Plain or ScalarStyle.Any
            && scalar.Value is null or "" or "~" or "null" or "Null" or "NULL";

    private static LocationInfo ToLocation(string path, Mark start, Mark end)
        => new(
            path,
            TextSpan.FromBounds((int)start.Index, Math.Max((int)start.Index, (int)end.Index)),
            new(ToLinePosition(start), ToLinePosition(end)));

    private static LinePosition ToLinePosition(Mark mark)
        => new(Math.Max((int)mark.Line - 1, 0), Math.Max((int)mark.Column - 1, 0));

    private static bool TryParseFileName(string fileName, out string stem, out string locale)
    {
        stem = fileName;
        locale = string.Empty;
        var nameWithLocale = fileName.Substring(0, fileName.Length - Extension.Length);
        var localeStart = nameWithLocale.LastIndexOf('.');
        if (localeStart < 0 || !LocaleRegex.IsMatch(nameWithLocale.Substring(localeStart + 1)))
        {
            return false;
        }
        stem = nameWithLocale.Substring(0, localeStart);
        locale = NormalizeLocale(nameWithLocale.Substring(localeStart + 1));
        return true;
    }

    /// <summary>
    /// Converts the locale to the casing used by <c>CultureInfo.Name</c>, e.g. <c>en-gb</c> to <c>en-GB</c>.
    /// </summary>
    private static string NormalizeLocale(string locale)
    {
        var parts = locale.Split('-');
        for (var i = 0; i < parts.Length; ++i)
        {
            var part = parts[i];
            parts[i] = (i, part.Length) switch
            {
                (> 0, 2) => part.ToUpperInvariant(),
                (> 0, 4) when part.All(char.IsLetter) => char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant(),
                _ => part.ToLowerInvariant()
            };
        }
        return string.Join("-", parts);
    }
}