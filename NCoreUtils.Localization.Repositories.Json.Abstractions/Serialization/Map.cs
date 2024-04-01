using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NCoreUtils.Localization.Serialization;

[JsonConverter(typeof(MapConverter))]
public class Map : Dictionary<CaseInsensitive, string> { }