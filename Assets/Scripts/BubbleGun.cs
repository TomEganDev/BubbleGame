using UnityEngine;
using UnityEngine.Assertions;

public class BubbleGun : MonoBehaviour
{
    public PreBubble PreBubblePrefab;
    public Transform FiringPoint;
    public Animator PlayerAnimator;
    
    private void Update()
    {
        transform.LookAt2D_Mouse(Input.mousePosition, MainCamera.Instance.CameraComponent);

        if (Input.GetMouseButtonDown(0)) // fire
        {
            var filter = new ContactFilter2D(); // todo - actually filter
            FiringPoint.GetPositionAndRotation(out var position, out var rotation);
            var hitCount = Physics2D.Raycast(transform.position, transform.right, filter, GlobalBuffers.HitBuffer,
                Vector3.Distance(position, transform.position));
            Assert.IsTrue(hitCount <= GlobalBuffers.HitBuffer.Length);
            var closestDistance = float.MaxValue;
            var closestIndex = -1;
            for (int i = 0; i < hitCount; i++)
            {
                var hit = GlobalBuffers.HitBuffer[i];
                if (Player.Instance.IsPlayer(hit.collider.gameObject))
                {
                    continue;
                }
                
                // todo - extra filtering

                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestIndex = i;
                }
            }

            if (closestIndex >= 0)
            {
                position = GlobalBuffers.HitBuffer[closestIndex].point;
            }
                
            var projectile = Instantiate(PreBubblePrefab, position, rotation);
            // Debug.Log($"Shot {projectile.name}", projectile);
            // Debug.Break();
        }
        
        // update animator
        var downUp01 = (Vector3.Dot(transform.right, Vector3.up) + 1f) / 2f;
        PlayerAnimator.SetFloat("AimDownUp01", downUp01);
    }
}
