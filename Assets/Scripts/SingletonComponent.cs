using UnityEngine;
using UnityEngine.Assertions;

public abstract class SingletonComponent<T> : MonoBehaviour where T : SingletonComponent<T>
{
    public static T Instance;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnEnterPlayMode()
    {
        Instance = null;
    }
    
    protected virtual void Awake()
    {
        Assert.IsNull(Instance);
        Instance = (T)this;
    }
}
