namespace NCoreUtils.Localization.Unit

open System.Globalization
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Localization
open NCoreUtils.Localization
open Xunit
open System

type JsonFileSystemTests () =

  let services =
    ServiceCollection()
      .AddLogging()
      .AddSourceBasedLocalization(fun b -> b.AddJsonFolder() |> ignore)
      .BuildServiceProvider(true)

  member private __.TestLocalizer (stringLocalizer : IStringLocalizer) =
    do
      let huLocalizer = stringLocalizer.WithCulture <| CultureInfo.GetCultureInfo "hu-HU"
      let value1 = huLocalizer.["Key1"]
      Assert.False value1.ResourceNotFound
      Assert.Equal ("Key1HU", value1.Value)
      let value2 = huLocalizer.["Key2"]
      Assert.False value2.ResourceNotFound
      Assert.Equal ("Key2HU", value2.Value)
      let value3 = huLocalizer.["NoValue"]
      Assert.True value3.ResourceNotFound
      Assert.Equal ("NoValue", value3.Value)
    do
      let enLocalizer = stringLocalizer.WithCulture <| CultureInfo.GetCultureInfo "en-US"
      let value1 = enLocalizer.["Key1"]
      Assert.False value1.ResourceNotFound
      Assert.Equal ("Key1EN", value1.Value)
      let value2 = enLocalizer.["Key2"]
      Assert.False value2.ResourceNotFound
      Assert.Equal ("Key2EN", value2.Value)
      let value3 = enLocalizer.["NoValue"]
      Assert.True value3.ResourceNotFound
      Assert.Equal ("NoValue", value3.Value)

  [<Fact>]
  member this.TypedStringLocalizerTests () =

    // if not(System.Diagnostics.Debugger.IsAttached) then
    //   printfn "Please attach a debugger, PID: %d" (System.Diagnostics.Process.GetCurrentProcess().Id)
    // while not(System.Diagnostics.Debugger.IsAttached) do
    //   System.Threading.Thread.Sleep(100)
    // System.Diagnostics.Debugger.Break()

    use scope = services.CreateScope ()
    let serviceProvider = scope.ServiceProvider
    serviceProvider.GetRequiredService<IStringLocalizer<SharedFS>>() |> this.TestLocalizer
    serviceProvider.GetRequiredService<IStringLocalizerFactory>().Create(null, "Inner") |> this.TestLocalizer
    serviceProvider.GetRequiredService<IStringLocalizerFactory>().Create("A", "Inner") |> this.TestLocalizer

  interface IDisposable with
    member __.Dispose () = services.Dispose ()

