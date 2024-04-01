using System.Reflection;

namespace NCoreUtils.Localization;

public interface IJsonResourceLocalizationConfiguration
{
    Assembly DefaultAssembly { get; }

    string? Prefix { get; }
}