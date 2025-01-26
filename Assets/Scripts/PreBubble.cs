using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class PreBubble : MonoBehaviour
{
    public Bubble BubblePrefab;
    public float Speed = 10f;
    public float Radius = 0.25f;

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        var filter = new ContactFilter2D
        {
            useTriggers = true
        };
        var hitCount = Physics2D.CircleCast(transform.position, Radius, transform.right , filter, GlobalBuffers.HitBuffer, Speed * deltaTime);
        Assert.IsTrue(hitCount <= GlobalBuffers.HitBuffer.Length);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = GlobalBuffers.HitBuffer[i];

            var hitBubble = hit.collider.GetComponent<Bubble>();
            if (hitBubble != null)
            {
                hitBubble.Pop();
                Destroy(gameObject);
                return;
            }
            
            if (BubblePopperLookup.IsPopper(hit.collider.gameObject))
            {
                Destroy(gameObject);
                return;
            }
            
            if (Player.Instance.IsPlayer(hit.collider.gameObject))
            {
                continue;
            }
            
            Debug.Log($"[{Time.frameCount}] Bubble_Spawn triggered by {hit.collider.gameObject.name}", hit.collider);
            
            // attach
            var hasReceiver = BubbleReceiver.TryGetReceiver(hit.collider.gameObject, out var receiver);
            var bubble = Instantiate(BubblePrefab, hasReceiver ? receiver.transform.position : hit.point, Quaternion.identity);
            var bubbleState = Bubble.State.Floor;
            if (hasReceiver)
            {
                bubbleState = Bubble.State.Floating;
                receiver.OnAttach(bubble);
                bubble.SetReceiver(receiver);
            }
            else if (hit.normal.y < -0.5f)
            {
                bubbleState = Bubble.State.Roof;
            }
            else if (hit.normal.y < 0.5f)
            {
                bubbleState = Bubble.State.Wall;
            }
            bubble.OnSpawn(bubbleState);
            
            Destroy(gameObject);

            return;
        }
        
        transform.Translate(Vector3.right * (Speed * Time.deltaTime),Space.Self);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
