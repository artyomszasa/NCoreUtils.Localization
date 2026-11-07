using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils;

// SETUP

var factory = new DefaultLocalizedStringsFactory();
using var services = new ServiceCollection()
    .AddTransient(_ => factory.Create())
    .BuildServiceProvider();

// MAIN



foreach (var locale in new string[] { "hu-HU", "en-GB", "de-DE" })
{
    CultureInfo.CurrentCulture = new CultureInfo(locale);
    ILocalizedStrings l = services.GetRequiredService<ILocalizedStrings>();
    Console.WriteLine("===============");

    // USER CODE *******************************************************************************************************

    Console.WriteLine((l.FormatProvider as CultureInfo)?.Name);
    Console.WriteLine(l.SimpleValue);
    Console.WriteLine(l.Nested.Value);
    Console.WriteLine(l.Interpolate(2));

    // *****************************************************************************************************************
}