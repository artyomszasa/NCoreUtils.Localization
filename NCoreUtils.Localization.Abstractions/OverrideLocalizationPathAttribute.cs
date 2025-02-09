using System;

namespace NCoreUtils.Localization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class OverrideLocalizationPathAttribute(string? location, string? basePath)
    : Attribute
{
    public string? Location { get; } = location;

    public string? BasePath { get; } = basePath;
}