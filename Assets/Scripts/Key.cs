using UnityEngine;

public enum KeyLockColor : byte
{
    Blue,
    Gold,
    Green,
    Orange
}

public class Key : MonoBehaviour
{
    public KeyLockColor Color;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!Player.Instance.IsPlayer(collision.gameObject))
        {
            return;
        }

        Destroy(gameObject);
        
        if (!Lock.TryGetLock(Color, out var lockObj))
        {
            Debug.LogError($"No lock found for color {Color}");
            return;
        }
        
        Destroy(lockObj.gameObject);
    }
}
