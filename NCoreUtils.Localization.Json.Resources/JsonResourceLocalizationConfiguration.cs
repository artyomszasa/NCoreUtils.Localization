using System.Reflection;

namespace NCoreUtils.Localization;

public class JsonResourceLocalizationConfiguration : IJsonResourceLocalizationConfiguration
{
    public Assembly DefaultAssembly { get; set; } = default!;

    public string? Prefix { get; set; }
}