namespace NCoreUtils.Localization.Json

open System
open System.Collections.Concurrent
open System.IO
open System.Reflection
open Microsoft.Extensions.Logging
open NCoreUtils
open NCoreUtils.Localization

type JsonFileSystemLocalizationSourceRepository (configuration : JsonLocalizationConfiguration, loggerFactory : ILoggerFactory) =
  let accessor =
    let path =
      match configuration.Path with
      | null | "" -> Path.Combine (Directory.GetCurrentDirectory (), "i18n")
      | path      -> Path.GetFullPath path
    JsonFileSystemLocalizationAccessor path
  let entries     = lazy accessor.GetAll ()
  let sourceCache = ConcurrentDictionary ()

  abstract GetSource : path:LocalizationPath -> JsonLocalizationSource

  default __.GetSource (path) =
    let cultures = entries.Value.GetOrDefault (path, [||])
    JsonLocalizationSource (
      path,
      loggerFactory.CreateLogger<JsonLocalizationSource> (),
      cultures,
      fun culture -> accessor.Open (path, culture))

  member private this.GetCachedSource (path : LocalizationPath) =
    let mutable source = Unchecked.defaultof<_>
    if not (sourceCache.TryGetValue (path, &source)) then
      source <- this.GetSource path
      sourceCache.TryAdd (path, source) |> ignore
    source

  member this.GetSource (``type`` : Type) =
    let lpath =
      match ``type``.GetCustomAttribute<OverrideLocalizationPathAttribute> () with
      | null -> LocalizationPath (``type``.Namespace, ``type``.Name)
      | attr -> LocalizationPath (attr.Location |?? ``type``.Namespace, attr.BaseName |?? ``type``.Name)
    this.GetCachedSource lpath

  member this.GetSource (baseName: string, location) =
    LocalizationPath (location, baseName)
    |> this.GetCachedSource

  interface ILocalizationSourceRepository with
    member this.GetSource ``type``             = this.GetSource ``type`` :> _
    member this.GetSource (baseName, location) = this.GetSource (baseName, location) :> _
