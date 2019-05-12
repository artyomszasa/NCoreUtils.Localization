namespace NCoreUtils.Localization

open System.Reflection

type JsonResourceLocalizationConfiguration (defaultAssembly : Assembly, prefix : string) =
  do if isNull defaultAssembly then nullArg "defaultAssembly"
  member val DefaultAssembly = defaultAssembly
  member val Prefix          = prefix
