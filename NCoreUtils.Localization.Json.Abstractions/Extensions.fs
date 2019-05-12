namespace NCoreUtils.Localization

open System.Collections.Immutable
open System.Globalization
open System.Runtime.CompilerServices
open NCoreUtils.Collections

[<Extension>]
[<Sealed; AbstractClass>]
type JsonLocalizationAccessorExtensions =

  [<Extension>]
  static member GetAll (this : Json.IJsonLocalizationAccessor) =
    let builder = ImmutableDictionary.CreateBuilder<LocalizationPath, CultureInfo[]> ()
    let map = MultiValueDictionary ()
    this.Enumerate ()
      |> Seq.iter (fun entry -> map.Add (entry.Path, entry.Culture))
    for key in map.Keys do
      let mutable cultures = Unchecked.defaultof<_>
      match map.TryGetValues (key, &cultures) with
      | true -> builder.Add (key, cultures)
      | _    -> ()
    builder.ToImmutable ()