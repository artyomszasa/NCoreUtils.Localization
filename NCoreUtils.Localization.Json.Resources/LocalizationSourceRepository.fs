namespace NCoreUtils.Localization.Json

open System
open System.Collections.Concurrent
open System.Reflection
open Microsoft.Extensions.Logging
open NCoreUtils
open NCoreUtils.Localization

type JsonResourceLocalizationSourceRepository (configuration : JsonResourceLocalizationConfiguration, loggerFactory : ILoggerFactory) =
  let accessorCache = ConcurrentDictionary ()
  let entryCache    = ConcurrentDictionary ()
  let sourceCache   = ConcurrentDictionary ()

  member private __.GetAccessor (assembly : Assembly) =
    let mutable accessor = Unchecked.defaultof<_>
    if not (accessorCache.TryGetValue (assembly, &accessor)) then
      accessor <- JsonResourceLocalizationAccessor assembly
      accessorCache.TryAdd (assembly, accessor) |> ignore
    accessor

  member private __.GetEntries (accessor : Json.IJsonLocalizationAccessor) =
    let mutable entries = Unchecked.defaultof<_>
    if not (entryCache.TryGetValue (accessor, &entries)) then
      entries <- accessor.GetAll ()
      entryCache.TryAdd (accessor, entries) |> ignore
    entries

  abstract GetSource : assembly:Assembly * path:LocalizationPath -> JsonLocalizationSource

  default this.GetSource (assembly, path) =
    let accessor = this.GetAccessor assembly
    let entries = this.GetEntries accessor
    let cultures = entries.GetOrDefault (path, [||])
    JsonLocalizationSource (
      path,
      loggerFactory.CreateLogger<JsonLocalizationSource> (),
      cultures,
      fun culture -> accessor.Open (path, culture))

  member private this.GetCachedSource (assembly : Assembly, path) =
    let mutable source = Unchecked.defaultof<_>
    let key = struct (assembly, path)
    if not (sourceCache.TryGetValue (key, &source)) then
      source <- this.GetSource (assembly, path)
      sourceCache.TryAdd (key, source) |> ignore
    source

  member this.GetSource (``type`` : Type) =
    let lpath =
      let (location, baseName) =
        match ``type``.GetCustomAttribute<OverrideLocalizationPathAttribute> () with
        | null -> (``type``.Namespace, ``type``.Name)
        | attr -> (attr.Location |?? ``type``.Namespace, attr.BaseName |?? ``type``.Name)
      match configuration.Prefix with
      | null | "" -> LocalizationPath (location, baseName)
      | prefix    -> LocalizationPath (sprintf "%s.%s" location prefix, baseName)
    this.GetCachedSource (``type``.Assembly, lpath)

  member this.GetSource (baseName: string, location) =
    this.GetCachedSource (configuration.DefaultAssembly, LocalizationPath (location, baseName))

  interface ILocalizationSourceRepository with
    member this.GetSource ``type``             = this.GetSource ``type`` :> _
    member this.GetSource (baseName, location) = this.GetSource (baseName, location) :> _
