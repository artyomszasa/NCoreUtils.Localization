namespace NCoreUtils.Localization

open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Microsoft.Extensions.DependencyInjection

[<Extension>]
[<Sealed; AbstractClass>]
type LocalizationBuilderJsonFileSystemExtensions =

  [<Extension>]
  static member AddEmbeddedJson (this : LocalizationSourceRepositoryBuilder, defaultAssembly, [<Optional; DefaultParameterValue(null:string)>] prefix : string) =
    JsonResourceLocalizationConfiguration (defaultAssembly, prefix) |> this.Services.AddSingleton |> ignore
    this.AddRepository<Json.JsonResourceLocalizationSourceRepository> ()
    this