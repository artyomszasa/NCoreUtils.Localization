namespace NCoreUtils.Localization

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Globalization
open System.Runtime.InteropServices
open NCoreUtils

/// Event arguments for localization source change events.
type LocalizationChangedEventArgs (changedLocales : ImmutableArray<string>) =
  inherit EventArgs ()
  /// Gets changed locales.
  member val ChangedLocales = changedLocales :> IReadOnlyList<string>

type LocalizationChangedEventHandler = delegate of obj * LocalizationChangedEventArgs -> unit

type private NullEmptyStringComparer private () =
  static member val Instance = NullEmptyStringComparer ()
  member __.Equals (a, b) =
    match String.IsNullOrEmpty a with
    | true -> String.IsNullOrEmpty b
    | _    -> String.Equals (a, b)
  member __.GetHashCode a =
    match String.IsNullOrEmpty a with
    | true -> 0
    | _    -> a.GetHashCode ()

[<Struct>]
[<CustomEquality; NoComparison>]
type LocalizationPath =
  val public Location : string
  val public BaseName : string
  static member private Eq (a : LocalizationPath, b : LocalizationPath) =
    NullEmptyStringComparer.Instance.Equals (a.Location, b.Location)
      && NullEmptyStringComparer.Instance.Equals (a.BaseName, b.BaseName)
  member this.IsEmpy = String.IsNullOrEmpty this.Location && String.IsNullOrEmpty this.BaseName
  new (location, baseName) =
    { Location = location
      BaseName = baseName }
  new (baseName) =
    { Location = null
      BaseName = baseName }
  override this.ToString () =
    sprintf "[%s,%s]" this.Location this.BaseName
  member this.Equals that = LocalizationPath.Eq (this, that)
  interface IEquatable<LocalizationPath> with
    member this.Equals that = LocalizationPath.Eq (this, that)
  override this.Equals obj =
    match obj with
    | null                        -> false
    | :? LocalizationPath as that -> LocalizationPath.Eq (this, that)
    | _                           -> false
  override this.GetHashCode () =
    NullEmptyStringComparer.Instance.GetHashCode this.Location * 17
      + NullEmptyStringComparer.Instance.GetHashCode this.BaseName

[<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Struct, Inherited = false, AllowMultiple = false)>]
[<AllowNullLiteral>]
type OverrideLocalizationPathAttribute =
  inherit Attribute
  val private location : string
  val private baseName : string
  member this.Location = this.location
  member this.BaseName = this.baseName
  new ([<Optional; DefaultParameterValue(null:string)>] location : string, [<Optional; DefaultParameterValue(null:string)>] baseName : string) =
    { location = location
      baseName = baseName }

/// <summary>
/// Defines functionality of the localization string sources.
/// </summary>
type ILocalizationSource =
  inherit IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>

/// <summary>
/// Defines functionality of the mutable localization string sources.
/// </summary>
type IMutableLocalizationSource =
  inherit ILocalizationSource
  /// <summary>
  /// Triggered when localization data has been changed.
  /// </summary>
  [<CLIEvent>]
  abstract Changed : IEvent<LocalizationChangedEventHandler, LocalizationChangedEventArgs>

/// <summary>
/// Defines functionality of the named localization string sources.
/// </summary>
type INamedLocalizationSource =
  inherit ILocalizationSource
  /// The complete path to load resources from.
  abstract Path     : LocalizationPath
  /// The location to load resources from.
  abstract Location : string
  /// The base name of the resource to load strings from.
  abstract BaseName : string

type ILocalizationSourceRepository =
  abstract GetSource : ``type``:Type -> ILocalizationSource
  abstract GetSource : baseName:string * location:string -> ILocalizationSource
