using UnityEngine;

public static class GlobalBuffers
{
    public static RaycastHit2D[] HitBuffer = new RaycastHit2D[32];
    public static Collider2D[] ColliderBuffer = new Collider2D[32];
}
