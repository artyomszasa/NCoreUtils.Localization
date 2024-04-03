using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Localization;

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = LocalizationError,
        EventName = nameof(LocalizationError),
        Level = LogLevel.Error,
        Message = "Overwriting localization data for {locale}."
    )]
    public static partial void LogLocalizationError(this ILogger logger, Exception exn, string locale);

    [LoggerMessage(
        EventId = LocalizationWarning,
        EventName = nameof(LocalizationWarning),
        Level = LogLevel.Warning,
        Message = "Failed to read localization data for {locale}." 
    )]
    public static partial void LogLocalizationWarning(this ILogger logger, string locale);

    [LoggerMessage(
        EventId = LocalizationEntriesWarning,
        EventName = nameof(LocalizationEntriesWarning),
        Level = LogLevel.Warning,
        Message = "Lazy entries generation did not end within 100 ms after cancellation."
    )]
    public static partial void LogLocalizationEntriesWarning(this ILogger logger);

    [LoggerMessage(
        EventId = LocalizationDataDebug,
        EventName = nameof(LocalizationDataDebug),
        Level = LogLevel.Debug,
        Message = "Reading localization data for {locale} took {elapsedMilliseconds}ms."
    )]
    public static partial void LogLocalizationDataDebug(this ILogger logger, string locale, long elapsedMilliseconds);

    [LoggerMessage(
        EventId = LocalizationEntriesDebug,
        EventName = nameof(LocalizationEntriesDebug),
        Level = LogLevel.Debug,
        Message = "Reading localization entries took {elapsedMilliseconds}ms."
    )]
    public static partial void LogLocalizationEntriesDebug(this ILogger logger, long elapsedMilliseconds);

}