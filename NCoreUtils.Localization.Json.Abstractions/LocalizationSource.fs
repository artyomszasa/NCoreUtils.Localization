namespace NCoreUtils.Localization.Json

open System.Collections
open System.Collections.Generic
open System.Collections.Immutable
open System.Diagnostics.CodeAnalysis
open System.Globalization
open System.IO
open System.Runtime.InteropServices
open Microsoft.Extensions.Logging
open NCoreUtils
open NCoreUtils.Collections
open NCoreUtils.Localization

type JsonLocalizationSource (path : LocalizationPath, logger : ILogger<JsonLocalizationSource>, cultures : IReadOnlyList<CultureInfo>, streamFactory : CultureInfo -> Stream) =
  let name (culture : CultureInfo) =
    sprintf "[%s,%s,%s]" path.Location path.BaseName culture.Name

  let read culture =
    use stream = streamFactory culture
    Json.JsonKeyValueReader.readFromStream logger (name culture) stream
    :> IReadOnlyDictionary<_, _>

  let lazyEntries =
    cultures
    |> Seq.map
      (fun culture -> KeyValuePair (culture, lazy read culture))
    |> ImmutableDictionary.CreateRange

  member val Entries =
    MappingReadOnlyDictionary (lazyEntries, fun l -> l.Value) :> IReadOnlyDictionary<_, _>

  member val Path = path

  [<ExcludeFromCodeCoverage>]
  member this.GetEnumerator () = this.Entries.GetEnumerator ()

  interface IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>> with
    member this.Count  with [<ExcludeFromCodeCoverage>] get () = this.Entries.Count
    member this.Keys   with [<ExcludeFromCodeCoverage>] get () = this.Entries.Keys
    member this.Values with [<ExcludeFromCodeCoverage>] get () = this.Entries.Values
    member this.Item   with [<ExcludeFromCodeCoverage>] get key = this.Entries.[key]
    [<ExcludeFromCodeCoverage>]
    member this.GetEnumerator () = this.GetEnumerator () :> IEnumerator
    [<ExcludeFromCodeCoverage>]
    member this.GetEnumerator () = this.GetEnumerator ()
    [<ExcludeFromCodeCoverage>]
    member this.ContainsKey key = this.Entries.ContainsKey key
    member this.TryGetValue (key, [<Out>] value : byref<_>) = this.Entries.TryGetValue (key, &value)
  interface INamedLocalizationSource with
    member this.BaseName with [<ExcludeFromCodeCoverage>] get () = this.Path.BaseName
    member this.Location with [<ExcludeFromCodeCoverage>] get () = this.Path.Location
    member this.Path     with [<ExcludeFromCodeCoverage>] get () = this.Path