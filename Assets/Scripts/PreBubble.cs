using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PreBubble : MonoBehaviour
{
    public Bubble BubblePrefab;
    public float Speed = 10f;

    private void Update()
    {
        transform.Translate(Vector3.forward * Speed,Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        // todo - can we attach?
        
        // attach
        transform.GetPositionAndRotation(out var position, out var rotation);
        Instantiate(BubblePrefab, position, rotation);
        Destroy(gameObject);
    }
}
