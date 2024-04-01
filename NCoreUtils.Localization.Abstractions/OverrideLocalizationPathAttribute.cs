using System;

namespace NCoreUtils.Localization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class OverrideLocalizationPathAttribute : Attribute
{
    public string? Location { get; }

    public string? BasePath { get; }

    public OverrideLocalizationPathAttribute(string? location, string? basePath)
    {
        Location = location;
        BasePath = basePath;
    }
}