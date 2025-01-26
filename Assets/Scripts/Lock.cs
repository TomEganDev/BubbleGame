using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour
{
    private static Dictionary<KeyLockColor, Lock> _lookup = new(4);
    
    public KeyLockColor Color;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private void OnPlayModeEnter()
    {
        _lookup.Clear();
    }
    
    private void Awake()
    {
        if (_lookup.ContainsKey(Color))
        {
            Debug.LogError($"Too many {Color} locks!", this);
        }
        
        _lookup.Add(Color, this);
    }

    public static bool TryGetLock(KeyLockColor color, out Lock lockObj)
    {
        if (_lookup.TryGetValue(color, out lockObj))
        {
            return true;
        }
        
        lockObj = null;
        return false;
    }
}
