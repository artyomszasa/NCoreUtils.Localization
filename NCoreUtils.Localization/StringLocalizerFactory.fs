namespace NCoreUtils.Localization

open System
open System.Collections.Concurrent
open Microsoft.Extensions.Localization

[<AbstractClass>]
type private InternalStringLocalizerFactory () =
  static let cache = ConcurrentDictionary<Type, InternalStringLocalizerFactory> ()

  static let createFactory =
    Func<Type, _> (fun ty -> Activator.CreateInstance (typedefof<InternalStringLocalizerFactory<_>>.MakeGenericType ty, true) :?> InternalStringLocalizerFactory)

  static member Create (source, resourceSource : Type) =
    cache.GetOrAdd(resourceSource, createFactory).Create source

  abstract Create : source:ILocalizationSource -> IStringLocalizer

and private InternalStringLocalizerFactory<'T> () =
  inherit InternalStringLocalizerFactory()
  override __.Create source = NCoreUtils.Localization.StringLocalizer<'T> (source, null) :> _

type StringLocalizerFactory =
  val private source : ILocalizationSource

  new (source) = { source = source }

  member this.Create (resourceSource : Type) = InternalStringLocalizerFactory.Create (this.source, resourceSource)

  member this.Create (baseName : string, location : string) = NCoreUtils.Localization.StringLocalizer (this.source, null) :> IStringLocalizer

  interface IStringLocalizerFactory with
    member this.Create resourceType = this.Create resourceType
    member this.Create (baseName, location) = this.Create (baseName, location)
