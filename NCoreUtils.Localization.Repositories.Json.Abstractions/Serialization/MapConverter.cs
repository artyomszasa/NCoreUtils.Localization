using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.Localization.Serialization
{
    public sealed class MapConverter : JsonConverter<Map>
    {
        private static Map ReadObject(ref Utf8JsonReader reader)
        {
            if (!reader.Read()) { throw new JsonException("Unexpected end of stream."); }
            var data = new Map();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException($"Expected {JsonTokenType.PropertyName} got {reader.TokenType}."); }
                var key = CaseInsensitive.Create(reader.GetString() ?? string.Empty);
                if (!reader.Read()) { throw new JsonException("Unexpected end of stream."); }
                var value = reader.GetString() ?? string.Empty;
                data[key] = value;
                if (!reader.Read()) { throw new JsonException("Unexpected end of stream."); }
            }
            return data;
        }

        private static Map ReadArray(ref Utf8JsonReader reader)
        {
            if (!reader.Read()) { throw new JsonException("Unexpected end of stream."); }
            var data = new Map();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException($"Expected {JsonTokenType.StartObject} got {reader.TokenType}."); }
                if (!reader.Read()) { throw new JsonException("Unexpected end of stream."); }
                CaseInsensitive? key = default;
                string? value = default;
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException($"Expected {JsonTokenType.PropertyName} got {reader.TokenType}."); }
                    if (reader.ValueTextEquals("key"))
                    {
                        if (!reader.Read()) { throw new JsonException("Unexpected end of stream."); }
                        key = new CaseInsensitive(reader.GetString() ?? string.Empty);
                    }
                    else if (reader.ValueTextEquals("value"))
                    {
                        if (!reader.Read()) { throw new JsonException("Unexpected end of stream."); }
                        value = reader.GetString() ?? string.Empty;
                    }
                    else
                    {
                        if (!reader.Read()) { throw new JsonException("Unexpected end of stream."); }
                        reader.TrySkip();
                    }
                    if (!reader.Read()) { throw new JsonException("Unexpected end of stream."); }
                }
                if (!key.HasValue)
                {
                    throw new InvalidOperationException($"\"key\" not specified at {reader.Position}.");
                }
                data[key.Value] = value ?? string.Empty;
                if (!reader.Read()) { throw new JsonException("Unexpected end of stream."); }
            }
            return data;
        }

        public override Map? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType switch
            {
                JsonTokenType.StartArray => ReadArray(ref reader),
                JsonTokenType.StartObject => ReadObject(ref reader),
                var tokenType => throw new JsonException($"Expected either array or object got {tokenType}.")
            };

        [ExcludeFromCodeCoverage]
        public override void Write(Utf8JsonWriter writer, Map value, JsonSerializerOptions options)
            => throw new InvalidOperationException("Writer not supposed to be used.");
    }
}