using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class BCC : MonoBehaviour
{
    public Rigidbody2D Body;
    public Collider2D LocalCollider;
    public ParticleSystem BubblePopVFX;
    
    public float PreGroundedJumpWindow = 0.15f;
    public float CoyoteWindow = 0.15f;
    public float InitialJumpVelocity = 9f;
    public float BubblePopVelocity = 13f;
    public float SuperJumpVelocity = 18f;

    public float RunForce = 75f;
    public float AirStrafeForce = 30f;
    public float MaxRunSpeed = 10f;
    public float MaxFallSpeed = -15f;
    public float MaxAirStrafeSpeed = 10f;

    private bool _grounded;
    private float _lastGroundedTime;
    
    private bool _jumpButton;
    private bool _jumpButtonDown;
    private float _jumpButtonDownTime;
    
    private bool _jumped;
    private bool _jumping;
    private float _jumpedTime;

    private float _bubblePoppedTime;
    private bool _bubblePopped;
    
    private void Reset()
    {
        Body = GetComponent<Rigidbody2D>();
        LocalCollider = GetComponent<Collider2D>();
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
        var hitCount = Physics2D.OverlapBox(transform.TransformPoint(LocalCollider.offset), Vector2.one, 0f, new ContactFilter2D(), GlobalBuffers.ColliderBuffer);
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
    }

    private void StartSuperJump()
    {
        Body.linearVelocityY = SuperJumpVelocity;
        _jumped = true;
        _jumping = false;
    }

    private void StartBubblePopJump()
    {
        Body.linearVelocityY = BubblePopVelocity;
        _jumped = false;
        _jumping = false;
    }

    public void OnBubblePop()
    {
        var time = Time.time;
        
        BubblePopVFX.Play();
        MainCamera.Instance.ScreenShake();
        
        _bubblePoppedTime = time;
        _bubblePopped = true;
            
        if (_jumpButton && time - _jumpButtonDownTime <= PreGroundedJumpWindow)
        {
            StartSuperJump();
        }
        else
        {
            StartBubblePopJump();
        }
    }
}
