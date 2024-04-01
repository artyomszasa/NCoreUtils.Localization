using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NCoreUtils.Localization.Serialization;

[ExcludeFromCodeCoverage]
[JsonSerializable(typeof(Map))]
public partial class MapSerializerContext : JsonSerializerContext { }