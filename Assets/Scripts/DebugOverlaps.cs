using UnityEngine;

public class DebugOverlaps : MonoBehaviour
{
    public bool IsOverlapping = false;

    private void Update()
    {
        IsOverlapping = Physics2D.OverlapPoint(transform.position);
    }
}
