module NCoreUtils.Localization.Unit.Json

open System
open System.IO
open Microsoft.Extensions.Logging
open Newtonsoft.Json
open NCoreUtils
open NCoreUtils.Localization.Json
open Xunit

let private jsonArray1 = """[{ "key": "key1", "value": "value1" }, { "key": "key2", "value": "value2" }]"""

let private jsonArray2 = """[{ "value": "value1", "key": "key1" }, { "value": "value2", "key": "key2" }]"""

let private jsonArray3 = """[{ "value": "value1", "key": "key1", "meta": { "verified": true } }, { "value": "value2", "key": "key2" }]"""

let private jsonObject = """{ "key1": "value1", "key2": "value2" }"""

let private jsonInvalid1 = "23"

let private jsonNonStringKey = """[{ "key": { "xxx": key1" }, "value": "value1" }, { "key": "key2", "value": "value2" }]"""

let private jsonValueMissing = """[{ "key": "key1" }, { "key": "key2", "value": "value2" }]"""

let private jsonKeyMissing = """[{ "value": "value1" }, { "key": "key2", "value": "value2" }]"""

let private jsonMultipleKey = """[{ "key": "key1", "key": "key1", "value": "value1" }, { "key": "key2", "value": "value2" }]"""

type private DummyLogger () =
  interface ILogger with
    member __.Log<'TState>(logLevel, eventId, state : 'TState, exn, formatter) = ()
    member __.IsEnabled _ = false
    member __.BeginScope<'TState> (_ : 'TState) = { new IDisposable with member __.Dispose () = () }

let private logger = DummyLogger ()

let private ci v = CaseInsensitive v

[<Fact>]
let ``JSON array`` () =
  do
    let data =
      use reader = new StringReader (jsonArray1)
      use jreader = new JsonTextReader (reader)
      JsonKeyValueReader.read jreader logger "inline"
    Assert.True (data.ContainsKey (ci "key1"))
    Assert.True (data.ContainsKey (ci "key2"))
    Assert.Equal ("value1", data.[ci "key1"])
    Assert.Equal ("value2", data.[ci "key2"])
    Assert.Equal (2, data.Count)
  do
    let data =
      use reader = new StringReader (jsonArray2)
      use jreader = new JsonTextReader (reader)
      JsonKeyValueReader.read jreader logger "inline"
    Assert.True (data.ContainsKey (ci "key1"))
    Assert.True (data.ContainsKey (ci "key2"))
    Assert.Equal ("value1", data.[ci "key1"])
    Assert.Equal ("value2", data.[ci "key2"])
    Assert.Equal (2, data.Count)
  do
    let data =
      use reader = new StringReader (jsonArray3)
      use jreader = new JsonTextReader (reader)
      JsonKeyValueReader.read jreader logger "inline"
    Assert.True (data.ContainsKey (ci "key1"))
    Assert.True (data.ContainsKey (ci "key2"))
    Assert.Equal ("value1", data.[ci "key1"])
    Assert.Equal ("value2", data.[ci "key2"])
    Assert.Equal (2, data.Count)

[<Fact>]
let ``JSON object`` () =
  let data =
    use reader = new StringReader (jsonObject)
    use jreader = new JsonTextReader (reader)
    JsonKeyValueReader.read jreader logger "inline"
  Assert.True (data.ContainsKey (ci "key1"))
  Assert.True (data.ContainsKey (ci "key2"))
  Assert.Equal ("value1", data.[ci "key1"])
  Assert.Equal ("value2", data.[ci "key2"])
  Assert.Equal (2, data.Count)

[<Fact>]
let ``Non-string key`` () =
  use reader = new StringReader (jsonNonStringKey)
  use jreader = new JsonTextReader (reader)
  Assert.Throws<JsonException> (fun () -> JsonKeyValueReader.read jreader logger "inline" |> ignore)

[<Fact>]
let ``Key missing`` () =
  use reader = new StringReader (jsonKeyMissing)
  use jreader = new JsonTextReader (reader)
  Assert.Throws<JsonException> (fun () -> JsonKeyValueReader.read jreader logger "inline" |> ignore)

[<Fact>]
let ``Value missing`` () =
  use reader = new StringReader (jsonValueMissing)
  use jreader = new JsonTextReader (reader)
  Assert.Throws<JsonException> (fun () -> JsonKeyValueReader.read jreader logger "inline" |> ignore)

[<Fact>]
let ``JSON primitive`` () =
  use reader = new StringReader (jsonInvalid1)
  use jreader = new JsonTextReader (reader)
  Assert.Throws<JsonException> (fun () -> JsonKeyValueReader.read jreader logger "inline" |> ignore)

[<Fact>]
let ``Multiple key property`` () =
  use reader = new StringReader (jsonMultipleKey)
  use jreader = new JsonTextReader (reader)
  Assert.Throws<JsonException> (fun () -> JsonKeyValueReader.read jreader logger "inline" |> ignore)

