namespace NCoreUtils.Localization.Json

open System.Globalization
open System.IO
open System.Text.RegularExpressions
open NCoreUtils.Localization

type JsonFileSystemLocalizationAccessor (path: string) =
  static let localizationName =
    Regex (
      "((?<basename>[^.]+)\\.)?(?<locale>[a-z]+|[a-z]+-[a-z]+)\\.i18n\\.json$",
      RegexOptions.Compiled ||| RegexOptions.IgnoreCase)

  let getCultureSafe (name : string) =
    try CultureInfo.GetCultureInfo name
    with | :? CultureNotFoundException -> null

  abstract Enumerate : unit -> seq<LocalizationSourceEntry>

  abstract Open : path:LocalizationPath * culture:CultureInfo -> Stream

  default __.Enumerate () : seq<Json.LocalizationSourceEntry> = seq {
    for name in Directory.EnumerateFiles (path, "*.i18n.json", SearchOption.AllDirectories) do
      match Path.GetFileName name |> localizationName.Match with
      | m when m.Success ->
        match getCultureSafe m.Groups.["locale"].Value with
        | null -> ()
        | culture ->
          let location = Path.GetDirectoryName(name).Remove(0, path.Length).Trim '/'
          yield { Path = LocalizationPath (location, m.Groups.["basename"].Value); Culture = culture }
      | _ -> () }

  default __.Open (lpath : LocalizationPath, culture : CultureInfo) =
    let name =
      match lpath.Location, lpath.BaseName with
      | (null | ""), (null | "") -> sprintf "%s.i18n.json" culture.Name
      |  loc,        (null | "") -> Path.Combine (loc, sprintf "%s.i18n.json" culture.Name)
      | (null | ""),  name       -> sprintf "%s.%s.i18n.json" name culture.Name
      |  loc,         name       -> Path.Combine (loc, sprintf "%s.%s.i18n.json" name culture.Name)
    Path.Combine (path, name) |> File.OpenRead :> _

  interface Json.IJsonLocalizationAccessor with
    member this.Enumerate () = this.Enumerate ()
    member this.Open (path, culture) = this.Open (path, culture)
