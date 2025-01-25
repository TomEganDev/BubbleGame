using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class BCC : MonoBehaviour
{
    public Rigidbody2D Body;
    public BoxCollider2D GroundedTrigger;
    
    public float PreGroundedJumpWindow = 0.15f;
    public float CoyoteWindow = 0.15f;
    public float InitialJumpVelocity = 9f;
    public float BubblePopVelocity = 13f;
    public float BubblePopWallPushVelocity = 7f;
    public float SuperJumpVelocity = 18f;

    public float RunForce = 75f;
    public float AirStrafeForce = 30f;
    public float MaxRunSpeed = 10f;
    public float MaxFallSpeed = -15f;
    public float MaxAirStrafeSpeed = 10f;

    private bool _grounded;
    public bool Grounded => _grounded;
    private float _lastGroundedTime;
    
    private bool _jumpButton;
    private bool _jumpButtonDown;
    private float _jumpButtonDownTime;
    
    private bool _jumped;
    public bool Jumped => _jumped;
    private bool _jumping;
    public bool Jumping => _jumping;
    private float _jumpedTime;

    private float _bubblePoppedTime;
    private bool _bubblePopped;
    public bool BubblePopped => _bubblePopped;
    
    private void Reset()
    {
        Body = GetComponent<Rigidbody2D>();
        GroundedTrigger = GetComponentInChildren<BoxCollider2D>(includeInactive: true);
    }

    private void Update()
    {
        var time = Time.time;
        var deltaTime = Time.deltaTime;
        
        // INPUT UPDATE
        var hInput = Input.GetAxisRaw("Horizontal");
        _jumpButtonDown = Input.GetButtonDown("Jump");
        if (_jumpButtonDown)
        {
            _jumpButtonDownTime = time;
            _jumpButton = true;
        }
        else
        {
            _jumpButton = Input.GetButton("Jump");
        }

        // GROUNDED UPDATE
        var boxOrigin = GroundedTrigger.transform.TransformPoint(GroundedTrigger.offset);
        var hitCount = Physics2D.OverlapBox(
            boxOrigin,
            GroundedTrigger.size * GroundedTrigger.transform.lossyScale,
            0f,
            new ContactFilter2D(),
            GlobalBuffers.ColliderBuffer);
        Assert.IsTrue(hitCount <= GlobalBuffers.ColliderBuffer.Length);

        _grounded = false;
        for (int i = 0; i < hitCount; i++)
        {
            var hitCollider = GlobalBuffers.ColliderBuffer[i];
            // ignore local colliders
            if (Player.Instance.IsPlayer(hitCollider.gameObject))
            {
                continue;
            }

            _grounded = true;
            break;
        }
        
        // HORIZONTAL LOGIC
        if (_grounded)
        {
            var velX = Body.linearVelocityX;

            if (Mathf.Abs(hInput) < 0.05f)
            {
                Body.linearVelocityX = 0f;
            }
            else if (!Mathf.Approximately(Mathf.Sign(velX), Mathf.Sign(hInput)))
            {
                Body.linearVelocityX = hInput * RunForce * deltaTime;
            }
            else
            {
                Body.linearVelocityX += hInput * RunForce * deltaTime;
            }
            
            // ground clamp
            Body.linearVelocityX = Mathf.Clamp(Body.linearVelocityX, -MaxRunSpeed, MaxRunSpeed);
        }
        else // airborne
        {
            Body.linearVelocityX += hInput * AirStrafeForce * deltaTime;
            // air clamp
            Body.linearVelocityX = Mathf.Clamp(Body.linearVelocityX, -MaxAirStrafeSpeed, MaxAirStrafeSpeed);
        }

        // JUMP LOGIC
        if (_grounded)
        {
            _jumped = false;
            _jumping = false;
            _bubblePopped = false;
            
            _lastGroundedTime = time;
            var pendingJump = _jumpButton && time - _jumpButtonDownTime <= PreGroundedJumpWindow;
            if (pendingJump)
            {
                StartJump();
            }
        }
        else // not grounded
        {
            var isSuperJump = _bubblePopped && !_jumped && time - _bubblePoppedTime <= CoyoteWindow;
            if (_jumpButtonDown && isSuperJump)
            {
                StartSuperJump();
            }
            
            var isCoyote = time - _lastGroundedTime <= CoyoteWindow;
            if (_jumpButtonDown && isCoyote && !_jumped)
            {
                StartJump();
            }

            // shortfall SMB style
            if (!_jumpButton && _jumping && Body.linearVelocityY > 0)
            {
                Body.linearVelocityY = 0f;
                _jumping = false;
            }
        }
        
        // CLAMP FALL SPEED
        if (Body.linearVelocityY < MaxFallSpeed)
        {
            Body.linearVelocityY = MaxFallSpeed;
        }
    }

    private void StartJump()
    {
        _jumped = true;
        _jumping = true;
        
        Body.linearVelocityY = InitialJumpVelocity;
        
        Debug.Log($"[{Time.frameCount}] StartJump vel:{Body.linearVelocity:N2}");
    }

    private void StartSuperJump()
    {
        Body.linearVelocityY = SuperJumpVelocity;
        _jumped = true;
        _jumping = false;
        
        Debug.Log($"[{Time.frameCount}] StartSuperJump vel:{Body.linearVelocity:N2}");
    }

    private void StartBubblePopJump(Bubble bubble)
    {
        Body.linearVelocityY = bubble.CurrentState == Bubble.State.Roof ? -BubblePopVelocity : BubblePopVelocity;

        if (bubble.CurrentState == Bubble.State.Wall)
        {
            HandleWallBubble(bubble);
        }
        
        _jumped = false;
        _jumping = false;
        
        Debug.Log($"[{Time.frameCount}] StartBubblePopJump Bubble_State:{bubble.CurrentState} vel:{Body.linearVelocity:N2}");
    }

    public void OnBubblePop(Bubble bubble)
    {
        var time = Time.time;
        
        _bubblePoppedTime = time;
        _bubblePopped = true;
            
        if (_jumpButton && time - _jumpButtonDownTime <= PreGroundedJumpWindow && bubble.CurrentState != Bubble.State.Roof)
        {
            StartSuperJump();
            if (bubble.CurrentState == Bubble.State.Wall)
            {
                HandleWallBubble(bubble);
            }
            
            Debug.Log($"[{Time.frameCount}] PreGroundedSuperJump vel:{Body.linearVelocity:N2}");
        }
        else
        {
            StartBubblePopJump(bubble);
        }
    }

    private void HandleWallBubble(Bubble bubble)
    {
        Assert.IsTrue(bubble.CurrentState == Bubble.State.Wall);
        
        var onRight = bubble.transform.position.x > transform.position.x;
        Body.linearVelocityX = onRight ? -BubblePopWallPushVelocity : BubblePopWallPushVelocity;
    }
}
