using System;
using System.Collections.Generic;

namespace NCoreUtils.Localization;

public class LocalizationChangedEventArgs(IReadOnlyList<string> changedLocales) : EventArgs
{
    public IReadOnlyList<string> ChangedLocales { get; } = changedLocales ?? throw new ArgumentNullException(nameof(changedLocales));
}