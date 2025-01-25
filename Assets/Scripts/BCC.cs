using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class BCC : MonoBehaviour
{
    private Collider2D[] _hitBuffer = new Collider2D[16];
    
    public float MoveForce = 2f;
    public float JumpSpeed = 3f;
    public float BubbleJumpSpeed = 5f;
    public float SuperJumpSpeed = 7f;
    public float MaxJumpTime = 0.2f;
    public float BubbleJumpWindow = .1f;
    public Rigidbody2D Body;
    public CircleCollider2D GroundedCollider;

    private bool _jumping;
    private bool _superJumped;
    private bool _grounded;
    private bool _jumpButton;
    private bool _jumpButtonDown;
    private float _jumpButtonDownTime;
    private float _jumpedTime;
    private float _bubbledTime;
    private float _normalGravity;

    private void Reset()
    {
        Body = GetComponent<Rigidbody2D>();
        GroundedCollider = GetComponentInChildren<CircleCollider2D>();
    }

    private void Awake()
    {
        Assert.IsTrue(GroundedCollider.offset == Vector2.zero);
        _normalGravity = Body.gravityScale;
    }

    private void Update()
    {
        _jumpButtonDown = Input.GetButtonDown("Jump");
        if (_jumpButtonDown)
        {
            _jumpButtonDownTime = Time.time;
            _jumpButton = true;
        }
        else // not jump down
        {
            _jumpButton = Input.GetButton("Jump");
        }
        
        UpdateGrounded();
        if (_grounded)
        {
            // if(_jumping && Time.time - _jumpButtonDownTime < 0.016f)
            // {
            //     JumpOff();
            // }
            _superJumped = false;
        }
        
        var hInput = Input.GetAxis("Horizontal");
        // todo - change force based on grounded
        Body.AddForce(MoveForce * hInput * Vector2.right);

        if (_jumpButtonDown)
        {
            if (CanSuperJump())
            {
                SuperJumpOn();
            }
            else if (CanJump())
            {
                JumpOn();
            }
        }
        else if (ShouldReleaseJump())
        {
            JumpOff();
        }
    }

    private void UpdateGrounded()
    {
        // todo - actually filter things
        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = false,
            useDepth = false,
            useLayerMask = false,
            useNormalAngle = false,
            useOutsideDepth = false
        };
        var hitCount = Physics2D.OverlapCircle(GroundedCollider.transform.position, GroundedCollider.radius, filter,
            _hitBuffer);
        Assert.IsTrue(hitCount <= _hitBuffer.Length);
        if (hitCount == 0)
        {
            _grounded = false;
        }
        else
        {
            var foundHit = false;
            for (int i = 0; i < hitCount; i++)
            {
                if (Player.Instance.IsPlayer(_hitBuffer[i].gameObject))
                {
                    continue;
                }

                foundHit = true;
                _grounded = true;
                break;
            }

            if (!foundHit)
            {
                _grounded = false;
            }
        }
    }

    private bool CanSuperJump()
    {
        return !_superJumped && !_jumping && Time.time - _bubbledTime <= BubbleJumpWindow;
    }
    
    private bool CanJump()
    {
        return _grounded && !_jumping;
    }

    private bool ShouldReleaseJump()
    {
        return _jumping && (!_jumpButton || (Time.time - _jumpedTime) >= MaxJumpTime);
    }

    private void JumpOn()
    {
        Body.linearVelocityY = JumpSpeed;
        Body.gravityScale = 0f;
        _jumping = true;
        _jumpedTime = Time.time;
        
        Debug.Log($"[{Time.frameCount}] Jumping");
    }

    private void SuperJumpOn()
    {
        _superJumped = true;
        Body.linearVelocityY = SuperJumpSpeed;
        Body.gravityScale = 0f;
        _jumping = true;
        _jumpedTime = Time.time;
        
        Debug.Log($"[{Time.frameCount}] Super Jumping WindowTime:{Time.time - _bubbledTime:F2}/{BubbleJumpWindow:F2}");
    }

    // private void DoubleJumpOn()
    // {
    //     Body.AddForce(Vector2.up * DoubleJumpForce, ForceMode2D.Impulse);
    //     Body.gravityScale = 0f;
    //     _jumping = true;
    //     var time = Time.time;
    //     _doubleJumpedTime = time;
    //     _jumpedTime = time;
    //     _doubleJumpedTime = time;
    //     
    //     Debug.Log($"[{Time.frameCount}] DoubleJumping");
    // }

    private void JumpOff()
    {
        Body.gravityScale = _normalGravity;
        _jumping = false;
        Debug.Log($"[{Time.frameCount}] EndJump");
    }

    public void OnBubblePop()
    {
        //_doubleJumped = false;
        var time = Time.time;
        _bubbledTime = time;
        Body.linearVelocityY = BubbleJumpSpeed;
        _superJumped = false;
        
        MainCamera.Instance.ScreenShake();
        
        Debug.Log($"[{Time.frameCount}] OnBubblePop");
        
        if (_jumpButton && time - _jumpButtonDownTime <= BubbleJumpWindow)
        {
            Debug.Log($"[{Time.frameCount}] EarlyWindowSuperJump time:{time - _jumpButtonDownTime:F2}/{BubbleJumpWindow:F2}");
            
            SuperJumpOn();
        }
        else if (_jumping)
        {
            Debug.Log($"[{Time.frameCount}] EarlySuperJump MISS time:{time - _jumpButtonDownTime:F2}/{BubbleJumpWindow:F2}");
        }
    }
}
