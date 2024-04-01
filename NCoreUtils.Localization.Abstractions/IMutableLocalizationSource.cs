namespace NCoreUtils.Localization;

public interface IMutableLocalizationSource : ILocalizationSource
{
    event LocalizationChangedEventHandler Changed;
}