using System;
using System.Collections.Generic;

// Owns the runtime flag set. Lives outside DialogueManager so gameplay
// code can set/read flags without touching dialogue plumbing.
public static class GameFlagsService
{
    private static readonly HashSet<GameFlags> _flags = new();

    public static event Action<GameFlags> OnFlagAdded;
    public static event Action<GameFlags> OnFlagRemoved;

    public static IReadOnlyCollection<GameFlags> All => _flags;

    public static bool Has(GameFlags flag) => _flags.Contains(flag);

    public static void Add(GameFlags flag)
    {
        if (flag == GameFlags.None) return;
        if (_flags.Add(flag))
        {
            OnFlagAdded?.Invoke(flag);
        }
    }

    public static void Remove(GameFlags flag)
    {
        if (_flags.Remove(flag))
        {
            OnFlagRemoved?.Invoke(flag);
        }
    }

    public static void Clear()
    {
        _flags.Clear();
    }

    public static bool TryAddByName(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return false;
        if (!Enum.TryParse(flagName, out GameFlags parsed)) return false;
        Add(parsed);
        return true;
    }
}
