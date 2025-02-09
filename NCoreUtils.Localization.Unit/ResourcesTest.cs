using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace NCoreUtils.Localization.Unit;

public sealed class ResourcesTest : IDisposable
{
    private ServiceProvider Services { get; } = new ServiceCollection()
        .AddLogging()
        .AddSourceBasedLocalization(b => b.AddEmbeddedJson(typeof(Shared).Assembly, "i18n"))
        .BuildServiceProvider(true);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Services.Dispose();
    }

    private static void TestLocalizer(IStringLocalizer stringLocalizer)
    {
        {
            CultureInfo.CurrentCulture = new CultureInfo("hu-HU");
            var value1 = stringLocalizer["Key1"];
            Assert.False(value1.ResourceNotFound);
            Assert.Equal("Key1HU", value1.Value);
            var value2 = stringLocalizer["Key2"];
            Assert.False(value2.ResourceNotFound);
            Assert.Equal("Key2HU", value2.Value);
            var value3 = stringLocalizer["NoValue"];
            Assert.True(value3.ResourceNotFound);
            Assert.True(StringComparer.InvariantCultureIgnoreCase.Equals("NoValue", value3.Value));
        }
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var value1 = stringLocalizer["Key1"];
            Assert.False(value1.ResourceNotFound);
            Assert.Equal("Key1EN", value1.Value);
            var value2 = stringLocalizer["Key2"];
            Assert.False(value2.ResourceNotFound);
            Assert.Equal("Key2EN", value2.Value);
            var value3 = stringLocalizer["NoValue"];
            Assert.True(value3.ResourceNotFound);
            Assert.True(StringComparer.InvariantCultureIgnoreCase.Equals("NoValue", value3.Value));
        }
    }

    [Fact]
    public void TypedStringLocalizerTests()
    {
        using var scope = Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        TestLocalizer(serviceProvider.GetRequiredService<IStringLocalizer<Shared>>());
        TestLocalizer(serviceProvider.GetRequiredService<IStringLocalizerFactory>().Create("A", "NCoreUtils.Localization.Unit.i18n.Inner"));
    }
}