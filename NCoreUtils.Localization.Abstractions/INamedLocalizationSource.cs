namespace NCoreUtils.Localization;

public interface INamedLocalizationSource : ILocalizationSource
{
    string? Location { get; }

    string? BasePath { get; }

    LocalizationPath Path => new(Location, BasePath);
}