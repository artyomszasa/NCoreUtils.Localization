using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace NCoreUtils.Localization.Generator;

/// <summary>
/// Key of the merged structure.
/// </summary>
internal sealed class MergedMember(string key, bool isObject, string file, YamlEntry origin)
{
    public string Key { get; } = key;

    /// <summary>Key as a C# identifier.</summary>
    public string Identifier { get; } = SyntaxFacts.GetKeywordKind(key) == SyntaxKind.None ? key : "@" + key;

    public bool IsObject { get; } = isObject;

    /// <summary>File where the key has been seen first.</summary>
    public string File { get; } = file;

    public YamlEntry Origin { get; } = origin;

    /// <summary>Nested keys, empty for strings.</summary>
    public List<MergedMember> Children { get; } = [];

    /// <summary>Values by locale (string keys only).</summary>
    public Dictionary<string, (string? Value, ValueTemplate? Template, string File, YamlEntry Entry)> Values { get; } = [];

    /// <summary>
    /// Placeholders of the first non-null value (string keys only), all other values must have the same ones.
    /// A method is generated instead of a property when it is not <c>null</c>.
    /// </summary>
    public ValueTemplate? Template { get; set; }

    /// <summary>First non-null value, defines <see cref="Template" />.</summary>
    public (string File, YamlEntry Entry)? TemplateOrigin { get; set; }

    /// <summary>Locales that define this key (object keys only).</summary>
    public HashSet<string> Locales { get; } = [];
}

internal sealed class LocalizationMerger(LocalizerTarget target)
{
    private const string RawValues = "RawValues";

    private const string FormatProvider = "FormatProvider";

    private const string ForceProperties = "ForceProperties";

    private readonly List<DiagnosticInfo> diagnostics = [];

    /// <summary>
    /// Merges the files into a single structure. The merge fails if the returned diagnostics contain errors.
    /// </summary>
    public static (List<MergedMember> Root, List<DiagnosticInfo> Diagnostics) Merge(
        LocalizerTarget target,
        IEnumerable<LocalizationFile> files)
    {
        var merger = new LocalizationMerger(target);
        var root = new List<MergedMember>();
        foreach (var file in files)
        {
            merger.MergeEntries(root, file.Entries, file, string.Empty);
        }
        merger.MergeForceProperties(root);
        merger.CheckTypeNames(root, string.Empty, string.Empty, []);
        return (root, merger.diagnostics);
    }

    /// <summary>
    /// Merges the forced properties as a file without values, so they follow the rules of the localization files.
    /// Its locale is never emitted.
    /// </summary>
    private void MergeForceProperties(List<MergedMember> root)
    {
        var location = target.Location ?? new(string.Empty, default, default);
        var entries = target.ForceProperties.Select(property => ToEntry(property, location)).ToArray();
        var file = new LocalizationFile(
            ForceProperties,
            ForceProperties,
            string.Empty,
            new(entries),
            EquatableArray<DiagnosticInfo>.Empty,
            HasErrors: false);
        MergeEntries(root, file.Entries, file, string.Empty);
    }

    private static YamlEntry ToEntry(string property, LocationInfo location)
    {
        var keys = property.Split(':');
        var entry = new YamlEntry(keys[keys.Length - 1], null, null, location);
        for (var i = keys.Length - 2; i >= 0; --i)
        {
            entry = new(keys[i], null, new([entry]), location);
        }
        return entry;
    }

    private void MergeEntries(List<MergedMember> members, EquatableArray<YamlEntry> entries, LocalizationFile file, string path)
    {
        foreach (var entry in entries)
        {
            var keyPath = path.Length == 0 ? entry.Key : path + "." + entry.Key;
            var member = members.Find(m => m.Key == entry.Key);
            if (member is null)
            {
                if (!CheckKey(entry, file, keyPath, isRoot: path.Length == 0))
                {
                    continue;
                }
                member = new(entry.Key, entry.Children is not null, FileName(file.Path), entry);
                members.Add(member);
            }
            if (member.IsObject != entry.Children is not null)
            {
                ReportConflict(keyPath, Kind(member.IsObject), member.File, member.Origin, Kind(!member.IsObject), file, entry);
            }
            else if (entry.Children is { } children)
            {
                member.Locales.Add(file.Locale!);
                MergeEntries(member.Children, children, file, keyPath);
            }
            else
            {
                MergeValue(member, entry, file, keyPath);
            }
        }
    }

    private void MergeValue(MergedMember member, YamlEntry entry, LocalizationFile file, string keyPath)
    {
        var locale = file.Locale!;
        var fileName = FileName(file.Path);
        ValueTemplate? template = null;
        if (entry.Value is not null)
        {
            template = ValueTemplate.Parse(entry.Value, out var error);
            if (error is not null)
            {
                diagnostics.Add(DiagnosticInfo.Create(Diagnostics.InvalidPlaceholder, entry.Location, keyPath, fileName, target.Name, error));
                return;
            }
            if (member.TemplateOrigin is not { } origin)
            {
                member.Template = template;
                member.TemplateOrigin = (fileName, entry);
            }
            else if (!ValueTemplate.AreCompatible(member.Template, template))
            {
                ReportConflict(
                    keyPath,
                    ValueTemplate.Describe(member.Template),
                    origin.File,
                    origin.Entry,
                    ValueTemplate.Describe(template),
                    file,
                    entry);
                return;
            }
        }
        if (!member.Values.TryGetValue(locale, out var existing) || existing.Value is null)
        {
            member.Values[locale] = (entry.Value, template, fileName, entry);
        }
        else if (entry.Value is not null && entry.Value != existing.Value)
        {
            ReportConflict(
                keyPath,
                $"\"{existing.Value}\"",
                existing.File,
                existing.Entry,
                $"\"{entry.Value}\"",
                file,
                entry);
        }
    }

    private bool CheckKey(YamlEntry entry, LocalizationFile file, string keyPath, bool isRoot)
    {
        string? problem = null;
        if (!SyntaxFacts.IsValidIdentifier(entry.Key))
        {
            problem = "it is not a valid C# identifier";
        }
        else if (entry.Key == RawValues)
        {
            problem = $"'{RawValues}' is reserved by the generated interface";
        }
        else if (entry.Key == FormatProvider)
        {
            problem = $"'{FormatProvider}' is reserved by the generated interface";
        }
        else if (isRoot && entry.Key == target.Name)
        {
            problem = "it conflicts with a member of the generated interface";
        }
        if (problem is null)
        {
            return true;
        }
        diagnostics.Add(DiagnosticInfo.Create(
            Diagnostics.InvalidKey,
            entry.Location,
            keyPath,
            FileName(file.Path),
            target.Name,
            problem));
        return false;
    }

    private void CheckTypeNames(List<MergedMember> members, string typeSuffix, string path, Dictionary<string, string> seen)
    {
        foreach (var member in members)
        {
            if (!member.IsObject)
            {
                continue;
            }
            var memberTypeSuffix = typeSuffix + "_" + member.Key;
            var keyPath = path.Length == 0 ? member.Key : path + "." + member.Key;
            if (seen.TryGetValue(memberTypeSuffix, out var other))
            {
                diagnostics.Add(DiagnosticInfo.Create(
                    Diagnostics.TypeNameCollision,
                    member.Origin.Location,
                    other,
                    keyPath,
                    target.Name,
                    target.Name + memberTypeSuffix));
            }
            else
            {
                seen.Add(memberTypeSuffix, keyPath);
            }
            CheckTypeNames(member.Children, memberTypeSuffix, keyPath, seen);
        }
    }

    private void ReportConflict(
        string keyPath,
        string firstKind,
        string firstFile,
        YamlEntry firstEntry,
        string secondKind,
        LocalizationFile secondFile,
        YamlEntry secondEntry)
        => diagnostics.Add(DiagnosticInfo.Create(
            Diagnostics.MergeConflict,
            secondEntry.Location,
            target.Name,
            keyPath,
            firstKind,
            firstFile,
            Line(firstEntry),
            secondKind,
            FileName(secondFile.Path),
            Line(secondEntry)));

    private static string Kind(bool isObject)
        => isObject ? "a nested object" : "a string";

    private static string Line(YamlEntry entry)
        => (entry.Location.LineSpan.Start.Line + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string FileName(string path)
        => System.IO.Path.GetFileName(path);
}