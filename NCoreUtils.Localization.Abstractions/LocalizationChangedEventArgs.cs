using System;
using System.Collections.Generic;

namespace NCoreUtils.Localization;

public class LocalizationChangedEventArgs : EventArgs
{
    public IReadOnlyList<string> ChangedLocales { get; }

    public LocalizationChangedEventArgs(IReadOnlyList<string> changedLocales)
        => ChangedLocales = changedLocales ?? throw new ArgumentNullException(nameof(changedLocales));
}