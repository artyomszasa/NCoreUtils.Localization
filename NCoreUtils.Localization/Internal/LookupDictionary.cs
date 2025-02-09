using System;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
using System.Linq;
using System.Threading;
#else
using System.Collections.Concurrent;
#endif

namespace NCoreUtils.Localization.Internal;

public sealed class LookupDictionary<TKey, TValue>(IEqualityComparer<TKey>? keyComparer)
    where TKey : notnull
{
#if NET8_0_OR_GREATER

    private readonly IEqualityComparer<TKey> _eq = keyComparer ?? EqualityComparer<TKey>.Default;

    private FrozenDictionary<TKey, TValue> _data = FrozenDictionary<TKey, TValue>.Empty;

    public TValue GetOrAdd<TArg>(TKey key, TArg arg, Func<TKey, TArg, TValue> factory)
    {
        if (_data.TryGetValue(key, out var value))
        {
            return value;
        }
        bool success;
        value = factory(key, arg);
        do
        {
            var curr = Volatile.Read(ref _data);
            if (curr.TryGetValue(key, out var value1))
            {
                // NOTE: value has been added on another thread...
                return value1;
            }
            var @new = curr.Append(new KeyValuePair<TKey, TValue>(key, value)).ToFrozenDictionary(_eq);
            success = ReferenceEquals(curr, Interlocked.CompareExchange(ref _data, @new, curr));
        }
        while (!success);
        return value;
    }

#else

    private readonly ConcurrentDictionary<TKey, TValue> _data = new(keyComparer ?? EqualityComparer<TKey>.Default);

    public TValue GetOrAdd<TArg>(TKey key, TArg arg, Func<TKey, TArg, TValue> factory)
    {
        if (_data.TryGetValue(key, out var value))
        {
            return value;
        }
        return _data.GetOrAdd(key, factory(key, arg));
    }

#endif

    public LookupDictionary() : this(default) { }
}