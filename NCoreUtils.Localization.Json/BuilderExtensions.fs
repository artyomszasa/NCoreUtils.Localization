namespace NCoreUtils.Localization

open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Microsoft.Extensions.DependencyInjection

[<Extension>]
[<Sealed; AbstractClass>]
type LocalizationBuilderJsonFileSystemExtensions =

  [<Extension>]
  static member AddJsonFolder (this : LocalizationSourceRepositoryBuilder, [<Optional; DefaultParameterValue(null:string)>] path : string) =
    this.Services.AddSingleton { Path = path } |> ignore
    this.AddRepository<Json.JsonFileSystemLocalizationSourceRepository> ()
    this