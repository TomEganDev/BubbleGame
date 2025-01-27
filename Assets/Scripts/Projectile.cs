using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public Rigidbody2D Body;
    [SerializeField]
    private Vector2 _velocity; 

    private void Reset()
    {
        Body = GetComponent<Rigidbody2D>();
        Body.gravityScale = 0f;
        Body.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Update()
    {
        Body.linearVelocity = _velocity;
    }

    public void SetVelocity(Vector2 velocity)
    {
        _velocity = velocity;
    }

    public void SetVelocityDirection(Vector2 direction)
    {
        Assert.IsTrue(Mathf.Approximately(direction.sqrMagnitude, 1f), "Velocity direction not normalized");
        
        _velocity = direction * _velocity.magnitude;
    }
}
