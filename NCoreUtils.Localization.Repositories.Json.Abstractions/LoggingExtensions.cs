using System;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Localization;

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = OverwritingLocalizationData,
        EventName = nameof(OverwritingLocalizationData),
        Level = LogLevel.Warning,
        Message = "Overwriting localization data for {locale}."
    )]
    public static partial void LogOverwritingLocalizationData(this ILogger logger, string locale);

    [LoggerMessage(
        EventId = FailedToReadLocalizationData,
        EventName = nameof(FailedToReadLocalizationData),
        Level = LogLevel.Warning,
        Message = "Failed to read localization data for {locale}."
    )]
    public static partial void LogFailedToReadLocalizationData(this ILogger logger, Exception exn, string locale);

    [LoggerMessage(
        EventId = GenerationDidNotEndWithin100Ms,
        EventName = nameof(GenerationDidNotEndWithin100Ms),
        Level = LogLevel.Warning,
        Message = "Lazy entries generation did not end within 100 ms after cancellation."
    )]
    public static partial void LogGenerationDidNotEndWithin100Ms(this ILogger logger);

    [LoggerMessage(
        EventId = ReadingLocalizationDataSuccess,
        EventName = nameof(ReadingLocalizationDataSuccess),
        Level = LogLevel.Debug,
        Message = "Reading localization data for {locale} took {elapsedMilliseconds}ms."
    )]
    public static partial void LogReadingLocalizationDataSuccess(this ILogger logger, string locale, long elapsedMilliseconds);

    [LoggerMessage(
        EventId = ReadingLocalizationEntriesSuccess,
        EventName = nameof(ReadingLocalizationEntriesSuccess),
        Level = LogLevel.Debug,
        Message = "Reading localization entries took {elapsedMilliseconds}ms."
    )]
    public static partial void LogReadingLocalizationEntriesSuccess(this ILogger logger, long elapsedMilliseconds);

}