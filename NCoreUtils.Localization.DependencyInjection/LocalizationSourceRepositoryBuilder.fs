namespace NCoreUtils.Localization

open System
open Microsoft.Extensions.DependencyInjection

type LocalizationSourceRepositoryBuilder (services : IServiceCollection) =
  let mutable repository = ValueNone

  member internal __.Repository =
    match repository with
    | ValueSome r -> r
    | ValueNone -> invalidOp "No localization source repository has been registered."

  member val Services = services

  member __.AddRepository<'TRepository when 'TRepository :> ILocalizationSourceRepository and 'TRepository : not struct> () =
    match repository with
    | ValueNone ->
      repository <- ValueSome typeof<'TRepository>
    | _ -> NotSupportedException "Composite source repository not yet supported" |> raise
