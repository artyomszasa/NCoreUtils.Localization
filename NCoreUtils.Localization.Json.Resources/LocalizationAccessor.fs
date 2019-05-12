namespace NCoreUtils.Localization.Json

open System.Globalization
open System.IO
open System.Reflection
open System.Text.RegularExpressions
open NCoreUtils.Localization

type JsonResourceLocalizationAccessor (assembly : Assembly) =
  static let localizationName =
    Regex (
      "(((?<location>.*)\\.)?(?<basename>[^.]+)\\.)?(?<locale>[a-z]+|[a-z]+-[a-z]+)\\.i18n\\.json$",
      RegexOptions.Compiled ||| RegexOptions.IgnoreCase)

  let getCultureSafe (name : string) =
    try CultureInfo.GetCultureInfo name
    with | :? CultureNotFoundException -> null

  abstract Enumerate : unit -> seq<LocalizationSourceEntry>

  abstract Open : path:LocalizationPath * culture:CultureInfo -> Stream

  default __.Enumerate () : seq<Json.LocalizationSourceEntry> = seq {
    let names = assembly.GetManifestResourceNames ()

    for name in names do
      match localizationName.Match name with
      | m when m.Success ->
        match getCultureSafe m.Groups.["locale"].Value with
        | null -> ()
        | culture ->
          yield { Path = LocalizationPath (m.Groups.["location"].Value, m.Groups.["basename"].Value); Culture = culture }
      | _ -> () }

  default __.Open (path : LocalizationPath, culture : CultureInfo) =
    let name =
      match path.Location, path.BaseName with
      | (null | ""), (null | "") -> sprintf "%s.i18n.json" culture.Name
      |  name,       (null | "")
      | (null | ""),  name       -> sprintf "%s.%s.i18n.json" name culture.Name
      |  loc,         name -> sprintf "%s.%s.%s.i18n.json" loc name culture.Name
    assembly.GetManifestResourceStream name

  interface Json.IJsonLocalizationAccessor with
    member this.Enumerate () = this.Enumerate ()
    member this.Open (path, culture) = this.Open (path, culture)
