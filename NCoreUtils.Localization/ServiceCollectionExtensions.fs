namespace NCoreUtils.Localization

open System
open System.Runtime.CompilerServices
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Localization

[<Extension>]
[<Sealed; AbstractClass>]
type LocalizationServiceCollectionExtensions =

  [<Extension>]
  static member AddSourceBasedLocalization (this : IServiceCollection, init : Action<LocalizationSourceRepositoryBuilder>) =
    let builder = LocalizationSourceRepositoryBuilder this
    init.Invoke builder
    this.AddSingleton (typeof<ILocalizationSourceRepository>, builder.Repository) |> ignore
    this.TryAddSingleton<StringLocalizerFactory> ()
    this.TryAddSingleton<IStringLocalizerFactory> (fun serviceProvider -> serviceProvider.GetRequiredService<StringLocalizerFactory> () :> IStringLocalizerFactory)
    this.TryAddTransient (typedefof<IStringLocalizer<_>>, typedefof<NCoreUtils.Localization.TypedStringLocalizer<_>>)
    this