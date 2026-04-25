using System;
using System.Collections.Generic;
using UnityEngine;

// String-keyed event registry for the dialogue system. Gameplay scripts
// register handlers in their Awake/OnEnable and the dialogue runner fires
// them when an entry plays or a choice is picked.
//
// JSON usage (on a DialogueEntry or Choice):
//   "Actions": [
//     { "Key": "ProgressBar.Advance" },
//     { "Key": "ProgressBar.SetFill", "Arg": "0.5" }
//   ]
public static class DialogueActions
{
    private static readonly Dictionary<string, Action<string>> _handlers
        = new(StringComparer.OrdinalIgnoreCase);

    public static void Register(string key, Action handler)
    {
        if (handler == null) { Unregister(key); return; }
        Register(key, _ => handler());
    }

    public static void Register(string key, Action<string> handler)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (handler == null) { Unregister(key); return; }
        _handlers[key] = handler;
    }

    public static void Unregister(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        _handlers.Remove(key);
    }

    public static void Clear() => _handlers.Clear();

    public static bool IsRegistered(string key) =>
        !string.IsNullOrEmpty(key) && _handlers.ContainsKey(key);

    public static bool Invoke(string key, string arg = null)
    {
        if (string.IsNullOrEmpty(key)) return false;
        if (!_handlers.TryGetValue(key, out var handler))
        {
            Debug.LogWarning($"DialogueActions: no handler registered for key '{key}'.");
            return false;
        }
        try
        {
            handler.Invoke(arg);
        }
        catch (Exception ex)
        {
            Debug.LogError($"DialogueActions: handler '{key}' threw: {ex}");
            return false;
        }
        return true;
    }

    public static void InvokeMany(IList<DialogueAction> actions)
    {
        if (actions == null) return;
        for (int i = 0; i < actions.Count; i++)
        {
            var a = actions[i];
            if (a == null) continue;
            Invoke(a.Key, a.Arg);
        }
    }
}
