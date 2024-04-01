using System.Collections.Generic;
using System.Globalization;

namespace NCoreUtils.Localization;

public interface ILocalizationSource : IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<CaseInsensitive, string>>
{ }