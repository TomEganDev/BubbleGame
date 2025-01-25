using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class PreBubble : MonoBehaviour
{
    private static RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    
    public Bubble BubblePrefab;
    public float Speed = 10f;
    public float Radius = 0.25f;

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        var filter = new ContactFilter2D(); // todo - actually filter
        var hitCount = Physics2D.CircleCast(transform.position, Radius, transform.right , filter, _hitBuffer, Speed * deltaTime);
        Assert.IsTrue(hitCount <= _hitBuffer.Length);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = _hitBuffer[i];
            if (Player.Instance.IsPlayer(hit.collider.gameObject))
            {
                continue;
            }
            
            // attach
            Instantiate(BubblePrefab, hit.point, Quaternion.identity);
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
