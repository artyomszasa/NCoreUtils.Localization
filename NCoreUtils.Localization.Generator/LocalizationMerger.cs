using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NCoreUtils.Localization.Generator
{
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
        public Dictionary<string, (string? Value, string File, YamlEntry Entry)> Values { get; } = [];

        /// <summary>Locales that define this key (object keys only).</summary>
        public HashSet<string> Locales { get; } = [];
    }

    internal sealed class LocalizationMerger(LocalizerTarget target)
    {
        private const string RawValues = "RawValues";

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
            merger.CheckTypeNames(root, string.Empty, string.Empty, []);
            return (root, merger.diagnostics);
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
            if (!member.Values.TryGetValue(locale, out var existing) || existing.Value is null)
            {
                member.Values[locale] = (entry.Value, fileName, entry);
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
            else if (isRoot && (entry.Key == target.Name || entry.Key == LocalizerNames.Factory(target.Name)))
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
}
