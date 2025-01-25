using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PreBubble : MonoBehaviour
{
    public Bubble BubblePrefab;
    public float Speed = 10f;

    private void Update()
    {
        transform.Translate(Vector3.right * (Speed * Time.deltaTime),Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // todo - can we attach?
        if (Player.Instance.IsPlayer(other.gameObject))
        {
            return;
        }
        
        // attach
        transform.GetPositionAndRotation(out var position, out var rotation);
        Instantiate(BubblePrefab, position, rotation);
        Destroy(gameObject);
    }
}
