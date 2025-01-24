using UnityEngine;

public static class TransformExtensions
{
    public static void LookAt2D(this Transform transform, Transform target)
    {
        // TODO - calculate rotation before using transform (for perf)
        var targetPos = target.position;
        targetPos.y = 0f;
        transform.LookAt(targetPos);
        transform.Rotate(Vector3.up, -90f);
    }

    public static void LookAt2D_Mouse(this Transform transform, Vector2 mousePos, Camera camera)
    {
        var worldPos = camera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;
        transform.LookAt(worldPos);
        transform.Rotate(Vector3.up, -90f);
    }
}
