namespace NCoreUtils.Localization

open System
open Microsoft.Extensions.Localization

type StringLocalizerFactory =
  val private source : ILocalizationSource

  new (source) = { source = source }

  member this.Create (resourceSource : Type) = NCoreUtils.Localization.StringLocalizer (this.source, null) :> IStringLocalizer

  member this.Create (baseName : string, location : string) = NCoreUtils.Localization.StringLocalizer (this.source, null) :> IStringLocalizer

  interface IStringLocalizerFactory with
    member this.Create resourceType = this.Create resourceType
    member this.Create (baseName, location) = this.Create (baseName, location)
