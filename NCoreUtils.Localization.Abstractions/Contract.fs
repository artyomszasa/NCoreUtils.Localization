namespace NCoreUtils.Localization

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Globalization
open NCoreUtils

/// Event arguments for localization source change events.
type LocalizationChangedEventArgs (changedLocales : ImmutableArray<string>) =
  inherit EventArgs ()
  /// Gets changed locales.
  member val ChangedLocales = changedLocales :> IReadOnlyList<string>

type LocalizationChangedEventHandler = delegate of obj * LocalizationChangedEventArgs -> unit

/// <summary>
/// Defines functionality for implemention localization string sources.
/// </summary>
type ILocalizationSource =
  inherit IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>
  /// <summary>
  /// Triggered when localization data has been changed.
  /// </summary>
  [<CLIEvent>]
  abstract Changed : IEvent<LocalizationChangedEventHandler, LocalizationChangedEventArgs>
