namespace NCoreUtils.Localization.Json

open System.Globalization
open System.IO
open NCoreUtils.Localization

[<Struct>]
[<NoEquality; NoComparison>]
type public LocalizationSourceEntry = {
  Path    : LocalizationPath
  Culture : CultureInfo }
  with
    member this.IsEmpty = this.Path.IsEmpy && isNull this.Culture

[<Interface>]
type public IJsonLocalizationAccessor =
  abstract Enumerate : unit -> seq<LocalizationSourceEntry>
  abstract Open : path:LocalizationPath * culture:CultureInfo -> Stream