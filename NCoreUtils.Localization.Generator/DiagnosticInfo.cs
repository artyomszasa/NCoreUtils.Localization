using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Localization.Generator;

internal sealed record DiagnosticInfo(DiagnosticDescriptor Descriptor, LocationInfo? Location, EquatableArray<string> Arguments)
{
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, LocationInfo? location, params string[] arguments)
        => new(descriptor, location, new(arguments));

    public Diagnostic ToDiagnostic()
    {
        var arguments = new List<object>();
        foreach (var argument in Arguments)
        {
            arguments.Add(argument);
        }
        return Diagnostic.Create(Descriptor, Location?.ToLocation(), [.. arguments]);
    }
}
