using UnityEngine;

public static class TransformExtensions
{
    public static void LookAt2D(this Transform transform, Transform target)
    {
        // TODO - calculate rotation before using transform (for perf)
        // var targetPos = target.position;
        // targetPos.z = 0f;
        // transform.LookAt(targetPos);
        // transform.Rotate(Vector3.up, -90f);

        var projectedPos = transform.position;
        projectedPos.z = 0;
        var targetPos = target.transform.position;
        targetPos.z = 0;

        var targetRight = targetPos - projectedPos;
        transform.right = targetRight;
    }

    public static void LookAt2D_Mouse(this Transform transform, Vector2 mousePos, Camera camera)
    {
        var worldPos = camera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;
        var projectedPos = transform.position;
        projectedPos.z = 0;
        // transform.LookAt(worldPos);
        // transform.Rotate(Vector3.up, -90f);
        var targetRight = worldPos - projectedPos;
        transform.right = targetRight;
    }
}
