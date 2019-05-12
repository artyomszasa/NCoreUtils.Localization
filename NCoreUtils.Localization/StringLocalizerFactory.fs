namespace NCoreUtils.Localization

open System
open System.Globalization
open System.Runtime.InteropServices
open Microsoft.Extensions.Localization

type StringLocalizerFactory =
  val internal repository : ILocalizationSourceRepository

  new (repository) = { repository = repository }

  member this.Create (resourceSource : Type) =
    let source = this.repository.GetSource resourceSource
    NCoreUtils.Localization.StringLocalizer (source, null) :> IStringLocalizer

  member this.Create (baseName : string, location : string) =
    let source = this.repository.GetSource (baseName, location)
    NCoreUtils.Localization.StringLocalizer (source, null) :> IStringLocalizer

  interface IStringLocalizerFactory with
    member this.Create resourceType = this.Create resourceType
    member this.Create (baseName, location) = this.Create (baseName, location)

type TypedStringLocalizer<'T> =
  inherit NCoreUtils.Localization.StringLocalizer

  new (factory : StringLocalizerFactory, [<Optional; DefaultParameterValue(null:CultureInfo)>] culture) =
    { inherit NCoreUtils.Localization.StringLocalizer (factory.repository.GetSource typeof<'T>, culture) }

  interface IStringLocalizer<'T>