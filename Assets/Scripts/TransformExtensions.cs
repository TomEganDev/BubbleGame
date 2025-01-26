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
        var diff = worldPos - transform.position;
        var angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f,0f,angle);
    }
}
