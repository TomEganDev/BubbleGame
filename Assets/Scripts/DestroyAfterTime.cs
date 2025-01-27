using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float DestroyTimer = 10f;

    private void Update()
    {
        DestroyTimer -= Time.deltaTime;
        if (DestroyTimer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
