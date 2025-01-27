using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class BCC : MonoBehaviour
{
    public Rigidbody2D Body;
    public BoxCollider2D GroundedTrigger;

    public float GravityScale = 2.5f;
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

    private float _bubblePoppedTime;
    private bool _bubblePopped;
    public bool BubblePopped => _bubblePopped;

    public Vector2 PlatformVelocity => _platformVelocity;
    [SerializeField] private Vector2 _platformVelocity;
    
    public Vector2 LocalVelocity => _localVelocity;
    [SerializeField] private Vector2 _localVelocity;
    
    private void Reset()
    {
        Body = GetComponent<Rigidbody2D>();
        GroundedTrigger = GetComponentInChildren<BoxCollider2D>(includeInactive: true);
    }

    private void Update()
    {
        var time = Time.time;
        var deltaTime = Time.deltaTime;

        _localVelocity = Body.linearVelocity;
        if (_grounded)
        {
            _localVelocity -= _platformVelocity;
        }
        
        //Debug.Log($"[{Time.frameCount}] UpdateBegin bodyVelocity:{Body.linearVelocity:N2} localReintegrate:{_localVelocity:N2}");
        
        _localVelocity += Physics2D.gravity * (GravityScale * deltaTime);
        
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
        var hitCount = Physics2D.BoxCast(boxOrigin, GroundedTrigger.size * GroundedTrigger.transform.lossyScale,
            0f, Vector2.down, new ContactFilter2D(), GlobalBuffers.HitBuffer, 0.05f);
        Assert.IsTrue(hitCount <= GlobalBuffers.HitBuffer.Length);
        
        //Debug.Log($"[{Time.frameCount}] ground hits: {hitCount}");

        _grounded = false;
        Collider2D hitCollider = null;
        for (int i = 0; i < hitCount; i++)
        {
            var hit = GlobalBuffers.HitBuffer[i];
            //Debug.Log($"Hit {i} {hit.collider.name}", hit.collider);
            
            // ignore local colliders
            if (Player.Instance.IsPlayer(hit.collider.gameObject))
            {
                continue;
            }
            if (hit.normal.y < 0.5f)
            {
                continue;
            }

            hitCollider = hit.collider;
            _grounded = true;
            break;
        }
        
        // HORIZONTAL LOGIC
        if (_grounded)
        {
            var platformBody = hitCollider!.GetComponent<Rigidbody2D>();
            if (platformBody != null)
            {
                _platformVelocity = platformBody.linearVelocity;
            }
            else
            {
                _platformVelocity = Vector2.zero;
            }
            
            var velX = _localVelocity.x;

            if (Mathf.Abs(hInput) < 0.05f)
            {
                _localVelocity.x = 0f;
            }
            else if (!Mathf.Approximately(Mathf.Sign(velX), Mathf.Sign(hInput)))
            {
                _localVelocity.x = hInput * RunForce * deltaTime;
            }
            else
            {
                _localVelocity.x += hInput * RunForce * deltaTime;
            }
            
            // ground clamp
            _localVelocity.x = Mathf.Clamp(_localVelocity.x, -MaxRunSpeed, MaxRunSpeed);
        }
        else // airborne
        {
            _localVelocity.x += hInput * AirStrafeForce * deltaTime;
            // air clamp
            _localVelocity.x = Mathf.Clamp(_localVelocity.x, -MaxAirStrafeSpeed, MaxAirStrafeSpeed);
        }

        // JUMP LOGIC
        if (_grounded)
        {
            _jumped = false;
            _jumping = false;
            _bubblePopped = false;
            
            _lastGroundedTime = time;
            var pendingJump = _jumpButton && time - _jumpButtonDownTime <= PreGroundedJumpWindow && time - _bubblePoppedTime >= CoyoteWindow;
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
            
            var isCoyote = time - _lastGroundedTime <= CoyoteWindow && time - _bubblePoppedTime > CoyoteWindow;
            if (_jumpButtonDown && isCoyote && !_jumped)
            {
                StartJump();
            }

            // shortfall SMB style
            if (!_jumpButton && _jumping && Body.linearVelocityY > 0 && !_bubblePopped)
            {
                _localVelocity.y = 0f;
                _jumping = false;
                Debug.Log($"[{Time.frameCount}] Shortfall");
            }
        }
        
        // CLAMP FALL SPEED
        if (!_grounded && _localVelocity.y < MaxFallSpeed)
        {
            _localVelocity.y = MaxFallSpeed;
        }
        
        // FINAL VELOCITY
        Body.linearVelocity = _localVelocity;
        if (_grounded)
        {
            Body.linearVelocity += _platformVelocity;
        }
        
        //Debug.Log($"[{Time.frameCount}] final velocity: {Body.linearVelocity:N2} local: {_localVelocity:N2} platform:{_platformVelocity:N2}");
    }

    private void StartJump()
    {
        _localVelocity.y = InitialJumpVelocity;
        _localVelocity.x += _platformVelocity.x;
        _platformVelocity = Vector2.zero;

        _jumped = true;
        _jumping = true;
        _grounded = false;
        
        Debug.Log($"[{Time.frameCount}] StartJump vel:{_localVelocity.y:N2}");
    }

    private void StartSuperJump()
    {
        _localVelocity.y = SuperJumpVelocity;
        Body.linearVelocityY = _localVelocity.y;
        _localVelocity.x += _platformVelocity.x;
        _platformVelocity = Vector2.zero;
        
        _jumped = true;
        _jumping = false;
        _grounded = false;

        Debug.Log($"[{Time.frameCount}] StartSuperJump vel:{_localVelocity.y:N2}");
    }

    private void StartBubblePopJump(Bubble bubble)
    {
        _localVelocity.y = bubble.CurrentState == Bubble.State.Roof ? -BubblePopVelocity : BubblePopVelocity;
        Body.linearVelocityY = _localVelocity.y;
        _localVelocity.x += _platformVelocity.x;
        _platformVelocity = Vector2.zero;

        if (bubble.CurrentState == Bubble.State.Wall)
        {
            HandleWallBubble(bubble);
        }
        
        _jumped = false;
        _jumping = false;
        _grounded = false;
        
        Debug.Log($"[{Time.frameCount}] {bubble.GetInstanceID()} StartBubblePopJump Bubble_State:{bubble.CurrentState} vel:{_localVelocity.y:N2}");
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
            
            Debug.Log($"[{Time.frameCount}] PreGroundedSuperJump vel:{_localVelocity:N2}");
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
        _localVelocity.x = onRight ? -BubblePopWallPushVelocity : BubblePopWallPushVelocity;
        Body.linearVelocityX = _localVelocity.x;
    }

    public void StopDead()
    {
        _localVelocity = Vector2.zero;
        _platformVelocity = Vector2.zero;
        Body.linearVelocity = Vector2.zero;
    }
}
