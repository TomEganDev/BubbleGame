using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class PreBubble : MonoBehaviour
{
    public Bubble BubblePrefab;
    public float Speed = 10f;
    public float Radius = 0.25f;

    private float _attachedBubbleRadius;
    
    private void Awake()
    {
        _attachedBubbleRadius = BubblePrefab.transform.localScale.x * BubblePrefab.BubbleTrigger.radius;
    }

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
            var bubbleState = Bubble.State.Floor;
            if (!hasReceiver && hit.normal.y < -0.5f)
            {
                bubbleState = Bubble.State.Roof;

                
            }
            else if (!hasReceiver && hit.normal.y < 0.5f)
            {
                bubbleState = Bubble.State.Wall;
            }

            // bubble corner check
            // todo - refactor this, I'm sorry god
            if (!hasReceiver)
            {
                if (bubbleState == Bubble.State.Wall)
                {
                    // bubble corner snap adjust
                    var isLeft = hit.normal.x < 0;
                    var penOffset = isLeft ? Vector2.right * 0.025f : Vector2.left * 0.025f;
                    var topPoint = hit.point + Vector2.up * _attachedBubbleRadius + penOffset;
                    var bottomPoint = hit.point + Vector2.down * _attachedBubbleRadius + penOffset;
                    if (!Physics2D.OverlapPoint(topPoint))
                    {
                        var fixHit = Physics2D.Raycast(topPoint, Vector2.down, _attachedBubbleRadius);
                        if (fixHit.collider == null)
                        {
                            Debug.LogError($"[{Time.frameCount}] BUBBLE_EDGE_DETECTION_FAIL {bubbleState} MISSING_TOP");
                            var debugHit = new GameObject($"{Time.frameCount} DEBUG_HIT");
                            debugHit.transform.position = hit.point;
                            var debugTop = new GameObject($"{Time.frameCount} DEBUG_UP");
                            debugTop.transform.position = topPoint;
                            Destroy(gameObject);
                            return;
                        }

                        var hitPoint = hit.point;
                        hitPoint.y -= fixHit.distance;
                        hit.point = hitPoint;
                    }
                    else if(!Physics2D.OverlapPoint(bottomPoint))
                    {
                        var fixHit = Physics2D.Raycast(bottomPoint, Vector2.up, _attachedBubbleRadius);
                        if (fixHit.collider == null)
                        {
                            Debug.LogError($"[{Time.frameCount}] BUBBLE_EDGE_DETECTION_FAIL {bubbleState} MISSING_BOTTOM");
                            var debugHit = new GameObject($"{Time.frameCount} DEBUG_HIT");
                            debugHit.transform.position = hit.point;
                            var debugDown = new GameObject($"{Time.frameCount} DEBUG_DOWN");
                            debugDown.transform.position = bottomPoint;
                            Destroy(gameObject);
                            return;
                        }

                        var hitPoint = hit.point;
                        hitPoint.y += fixHit.distance;
                        hit.point = hitPoint;
                    }
                }
                else // roof or ceiling
                {
                    // bubble corner snap adjust
                    var penOffset = bubbleState == Bubble.State.Floor ? Vector2.down * 0.025f : Vector2.up * 0.025f;
                    var rightPoint = hit.point + Vector2.right * _attachedBubbleRadius + penOffset;
                    var leftPoint = hit.point + Vector2.left * _attachedBubbleRadius + penOffset;
                    if (!Physics2D.OverlapPoint(rightPoint))
                    {
                        var fixHit = Physics2D.Raycast(rightPoint, Vector2.left, _attachedBubbleRadius);
                        if (fixHit.collider == null)
                        {
                            Debug.LogError($"[{Time.frameCount}] BUBBLE_EDGE_DETECTION_FAIL {bubbleState} MISSING_RIGHT");
                            var debugHit = new GameObject($"{Time.frameCount} DEBUG_HIT");
                            debugHit.transform.position = hit.point;
                            var debugRight = new GameObject($"{Time.frameCount} DEBUG_RIGHT");
                            debugRight.transform.position = rightPoint;
                            Destroy(gameObject);
                            return;
                        }

                        var hitPoint = hit.point;
                        hitPoint.x -= fixHit.distance;
                        hit.point = hitPoint;
                    }
                    else if(!Physics2D.OverlapPoint(leftPoint))
                    {
                        var fixHit = Physics2D.Raycast(leftPoint, Vector2.right, _attachedBubbleRadius);
                        if (fixHit.collider == null)
                        {
                            Debug.LogError($"[{Time.frameCount}] BUBBLE_EDGE_DETECTION_FAIL {bubbleState} MISSING_LEFT");
                            var debugHit = new GameObject($"{Time.frameCount} DEBUG_HIT");
                            debugHit.transform.position = hit.point;
                            var debugLeft = new GameObject($"{Time.frameCount} DEBUG_LEFT");
                            debugLeft.transform.position = leftPoint;
                            Destroy(gameObject);
                            return;
                        }

                        var hitPoint = hit.point;
                        hitPoint.x += fixHit.distance;
                        hit.point = hitPoint;
                    }
                }
            }
            
            var bubble = Instantiate(BubblePrefab, hasReceiver ? receiver.transform.position : hit.point, Quaternion.identity);
            
            if (hasReceiver)
            {
                bubbleState = Bubble.State.Floating;
                receiver.OnAttach(bubble);
                bubble.SetReceiver(receiver);
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
