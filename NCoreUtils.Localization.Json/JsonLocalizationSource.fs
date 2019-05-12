namespace NCoreUtils.Localization


(*
// type JsonLocalizationSourceFactory

[<AutoOpen>]
module private JsonLocalizationSourceHelpers =

  let localizationName = Regex ("([^/]+).i18n.json$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)

  let readSafe logger (path : string) (locale : string) (builder : ImmutableDictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>.Builder) =
    try
      let culture = CultureInfo locale
      let data = Json.JsonKeyValueReader.readFrom path logger
      builder.[culture] <- data
    with exn ->
      logger.LogError (exn, "Failed to load json localization data from {0}: {1}", box path, box exn.Message)

// FIXME: watch file changes
type JsonLocalizationSource =

  val private localizations : ImmutableDictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>

  val private options : JsonLocalizationSourceOptions
  val private logger : ILogger

  val private changed : Event<LocalizationChangedEventHandler, LocalizationChangedEventArgs>

  member this.Keys with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = this.localizations.Keys

  member this.Values with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = this.localizations.Values

  member this.Count with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = this.localizations.Count

  [<CLIEvent>]
  member this.Changed = this.changed.Publish

  new (options : JsonLocalizationSourceOptions, logger : ILogger<JsonLocalizationSource>) =
    if isNull logger        then ArgumentNullException "logger"  |> raise
    if isNull (box options) then ArgumentNullException "options" |> raise
    if not (Directory.Exists options.Path) then
      invalidOpf "Invalid localization path %s" options.Path
    let builder = ImmutableDictionary.CreateBuilder<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>> ()
    Directory.GetFiles options.Path
    |> Array.iter
      (fun path ->
        match localizationName.Match path with
        | m when m.Success ->
          let locale = m.Groups.[1].Value
          readSafe logger path locale builder
        | _ -> logger.LogWarning("Skipping file {0} in json localization directory.", path)
      )
    { localizations = builder.ToImmutable ()
      options       = options
      logger        = logger
      changed       = Event<_, _>() }

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.ContainsKey key = this.localizations.ContainsKey key

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.TryGetValue (key, [<Out>] value : byref<_>) = this.localizations.TryGetValue (key, &value)

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.GetEnumerator () = this.localizations.GetEnumerator () :> IEnumerator<_>

  interface IEnumerable with
    member this.GetEnumerator () = this.GetEnumerator () :> _
  interface IEnumerable<KeyValuePair<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>> with
    member this.GetEnumerator () = this.GetEnumerator ()
  interface IReadOnlyCollection<KeyValuePair<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>> with
    member this.Count = this.Count
  interface IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>> with
    member this.Keys = this.Keys
    member this.Values = this.Values
    member this.Item with get key = this.localizations.[key]
    member this.ContainsKey key = this.ContainsKey key
    member this.TryGetValue (key, [<Out>] value : byref<_>) = this.TryGetValue(key, &value)
  interface ILocalizationSource with
    [<CLIEvent>]
    member this.Changed = this.Changed
*)