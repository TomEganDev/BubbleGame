using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class BubblePopperLookup
{
    private static HashSet<GameObject> _lookup = new HashSet<GameObject>(512);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnPlayModeEnter()
    {
        _lookup.Clear();
    }
    
    public static bool IsPopper(GameObject gameObject)
    {
        return _lookup.Contains(gameObject);
    }

    public static void RegisterBubblePopper(GameObject gameObject)
    {
        Assert.IsTrue(!_lookup.Contains(gameObject));
        _lookup.Add(gameObject);
    }

    public static void UnregisterBubblePopper(GameObject gameObject)
    {
        Assert.IsTrue(_lookup.Contains(gameObject));
        _lookup.Remove(gameObject);
    }
}
