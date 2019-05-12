namespace NCoreUtils.Localization

open System
open System.Globalization
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Microsoft.Extensions.Localization
open NCoreUtils

type StringLocalizer =
  val private source : ILocalizationSource
  val private culture : CultureInfo

  member internal this.Source = this.source

  member this.TargetCulture =
    match this.culture with
    | null -> CultureInfo.CurrentCulture
    | _    -> this.culture

  new (source, [<Optional; DefaultParameterValue(null:CultureInfo)>] culture) =
    { source  = source
      culture = culture }

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member private this.TryGet (key, [<Out>] result : byref<_>) =
    let mutable localizations = Unchecked.defaultof<_>
    this.source.TryGetValue (this.TargetCulture, &localizations) && localizations.TryGetValue (key, &result)

  member private this.GetInternal key =
    let mutable result = Unchecked.defaultof<_>
    match this.TryGet (CaseInsensitive key, &result) with
    | true -> result
    | _    -> key

  member this.Item
    with get (name : string) =
      let mutable localized = Unchecked.defaultof<_>
      match this.TryGet (CaseInsensitive name, &localized) with
      | true -> LocalizedString (name, localized, false)
      | _    -> LocalizedString (name, name,      true)

  member this.Item
    with get (name : string, [<ParamArray>] arguments : obj[]) =
      let args =
        arguments
        |> Array.map
          (function
            | :? string as stringArg -> this.GetInternal stringArg |> box
            | arg                    -> arg
          )
      let mutable nameLocalization = Unchecked.defaultof<_>
      match this.TryGet (CaseInsensitive name, &nameLocalization) with
      | true -> LocalizedString (name, System.String.Format(nameLocalization, args), false)
      | _    -> LocalizedString (name, System.String.Format(name,             args), true)

  member this.GetAllStrings (includeParentCultures : bool) =
    let mutable values = Unchecked.defaultof<_>
    match this.source.TryGetValue (this.TargetCulture, &values) with
    | true -> values |> Seq.map (fun kv -> LocalizedString (kv.Key.Value, kv.Value))
    | _    -> Seq.empty

  abstract WithCulture : culture:CultureInfo -> IStringLocalizer

  default this.WithCulture culture = StringLocalizer (this.source, culture) :> IStringLocalizer

  interface IStringLocalizer with
    member this.Item with get name = this.Item name
    member this.Item with get (name, arguments) = this.Item (name, arguments)
    member this.GetAllStrings includeParentCultures = this.GetAllStrings includeParentCultures
    member this.WithCulture culture = this.WithCulture culture