namespace NCoreUtils.Localization.Json

open System.Collections.Immutable
open System.IO
open System.Runtime.CompilerServices
open System.Text
open Microsoft.Extensions.Logging
open Newtonsoft.Json
open NCoreUtils

[<AutoOpen>]
module private JsonExt =

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let jfailwith msg = JsonException msg |> raise

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let jfailwithf fmt = Printf.kprintf jfailwith fmt

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let readOrFail (reader : JsonReader) =
    if not (reader.Read()) then
      jfailwith "End of stream reached while reading json."

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let expectedOrFail expectedToken (reader : JsonReader) =
    if reader.TokenType <> expectedToken then
      jfailwithf "Expected %A, got %A at %s." expectedToken reader.TokenType reader.Path

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let readExpectedOrFail expectedToken (reader : JsonReader) =
    readOrFail reader
    expectedOrFail expectedToken reader


module private Readers =

  [<Struct>]
  [<NoEquality; NoComparison>]
  type KvRes =
    | Kv of Key:string * Value:string
    | No

  [<Struct>]
  [<NoEquality; NoComparison>]
  type Both =
    | Neither
    | Left  of KeyOnly:string
    | Right of ValueOnly:string
    | Both  of Key:string * Value:string

  [<RequireQualifiedAccess>]
  module Both =

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    let withKey key both =
      match both with
      | Neither -> Left key
      | Right v -> Both (key, v)
      | Both (k, _)
      | Left  k -> jfailwithf "Key already specified (orignal = %s,  new = %s)" k key

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    let withValue value both =
      match both with
      | Neither -> Right value
      | Left k -> Both (k, value)
      | Both (_, v)
      | Right v -> jfailwithf "Value already specified (orignal = %s,  new = %s)" v value

  type IKeyValueReader =
    abstract ResRead : reader:JsonReader * logger:ILogger -> KvRes

  let objectReader =
    { new IKeyValueReader with
        member __.ResRead (reader, _) =
          readOrFail reader
          match reader.TokenType with
          | JsonToken.EndObject -> No
          | _ ->
            expectedOrFail JsonToken.PropertyName reader
            let key = reader.Value :?> string
            readExpectedOrFail JsonToken.String reader
            Kv (key, reader.Value :?> string)
    }

  let arrayReader =
    let rec read acc (logger : ILogger) (reader : JsonReader) =
      match reader.TokenType with
      | JsonToken.EndObject ->
        match acc with
        | Neither -> jfailwithf "Neither key nor value found at path %s" reader.Path
        | Left  v -> jfailwithf "No value found for key %s at path %s" v reader.Path
        | Right v -> jfailwithf "No key found for value %s at path %s" v reader.Path
        | Both (k, v) -> Kv (k, v)
      | JsonToken.PropertyName ->
        let propertyName = reader.Value :?> string
        match propertyName with
        | EQI "key" ->
          readExpectedOrFail JsonToken.String reader
          let key = reader.Value :?> string
          readOrFail reader
          read (Both.withKey key acc) logger reader
        | EQI "value" ->
          readExpectedOrFail JsonToken.String reader
          let value = reader.Value :?> string
          readOrFail reader
          read (Both.withValue value acc) logger reader
        | _ ->
          logger.LogWarning ("Unexpected property {0} in key/value object.", propertyName)
          let depth = reader.Depth
          readOrFail reader
          while (reader.Depth > depth) || (reader.Depth = depth && reader.TokenType <> JsonToken.PropertyName && reader.TokenType <> JsonToken.EndObject) do
            readOrFail reader
          readOrFail reader
          read acc logger reader
      | _ -> jfailwithf "Expected %A ot %A, got %A at %s." JsonToken.EndObject JsonToken.PropertyName reader.TokenType reader.Path
    { new IKeyValueReader with
        member __.ResRead (reader, logger) =
          readOrFail reader
          match reader.TokenType with
          | JsonToken.EndArray -> No
          | _ ->
            expectedOrFail JsonToken.StartObject reader
            readOrFail reader
            read Neither logger reader
    }

module public JsonKeyValueReader =

  [<CompiledName("Read")>]
  let read logger source (reader : JsonReader) =
    try
      if JsonToken.None = reader.TokenType then
        readOrFail reader
      let kvReader =
        match reader.TokenType with
        | JsonToken.StartArray -> Readers.arrayReader
        | JsonToken.StartObject -> Readers.objectReader
        | _ -> jfailwithf "Unexception token %A at path %A" reader.TokenType reader.Path
      let builder = ImmutableDictionary.CreateBuilder<CaseInsensitive, string> ()
      let rec readAll (_ : int) =
        match kvReader.ResRead (reader, logger) with
        | Readers.No -> builder.ToImmutable ()
        | Readers.Kv (key, value) ->
          let k = CaseInsensitive key
          if builder.ContainsKey k then
            logger.LogWarning ("Found duplicate key {0} while parsing {1}.", key, source)
          builder.Add (k, value)
          readAll Unchecked.defaultof<_>
      readAll Unchecked.defaultof<_>
    with e -> JsonException (sprintf "Failed to read key/values from %s" source, e) |> raise

  [<CompiledName("Read")>]
  let readFromTextReader logger source (reader : TextReader) =
    use jreader = new JsonTextReader (reader)
    read logger source jreader

  [<CompiledName("Read")>]
  let readFromStreamWithEncoding logger source encoding (stream : Stream) =
    use reader = new StreamReader (stream, encoding, false)
    readFromTextReader logger source reader

  [<CompiledName("Read")>]
  let readFromStream logger source (stream : Stream) =
    use reader = new StreamReader (stream, true)
    readFromTextReader logger source reader

  [<CompiledName("Read")>]
  let readFromPath logger (path : string) =
    use reader = new StreamReader (path, Encoding.UTF8, true)
    use jreader = new JsonTextReader (reader)
    read logger path jreader