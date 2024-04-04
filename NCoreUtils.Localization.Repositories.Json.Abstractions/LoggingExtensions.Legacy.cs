using System;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Localization;

internal static partial class LoggingExtensions
{
    public static void LogOverwritingLocalizationData(this ILogger logger, Exception exn, string locale)
        => logger.Log(
            logLevel: LogLevel.Warning,
            eventId: new EventId(OverwritingLocalizationData, nameof(OverwritingLocalizationData)),
            message: "Overwriting localization data for {locale}.",
            exception: exn
        );

    public static void LogLocalizationWarning(this ILogger logger, string locale)
        => logger.Log(
            logLevel: LogLevel.Warning,
            eventId: new EventId(FailedToReadLocalizationData, nameof(FailedToReadLocalizationData)),
            message: "Failed to read localization data for {locale}."
        );

    public static void LogLocalizationEntriesWarning(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Warning,
            eventId: new EventId(GenerationDidNotEndWithin100Ms, nameof(GenerationDidNotEndWithin100Ms)),
            message: "Lazy entries generation did not end within 100 ms after cancellation."
        );

    public static void LogLocalizationDataDebug(this ILogger logger, string locale, long elapsedMilliseconds)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(ReadingLocalizationDataSuccess, nameof(ReadingLocalizationDataSuccess)),
            message: "Reading localization data for {locale} took {elapsedMilliseconds}ms."
        );

    public static void LogLocalizationEntriesDebug(this ILogger logger, long elapsedMilliseconds)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(ReadingLocalizationEntriesSuccess, nameof(ReadingLocalizationEntriesSuccess)),
            message: "Reading localization entries took {elapsedMilliseconds}ms."
        );
}