using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class JumpTester : MonoBehaviour
{
    public Rigidbody2D Body;
    public Collider2D LocalCollider;
    public ParticleSystem BubblePopVFX;
    
    public float PreGroundedJumpWindow = 0.15f;
    public float CoyoteWindow = 0.1f;
    public float InitialJumpVelocity = 6f;
    public float BubblePopVelocity = 9f;
    public float SuperJumpVelocity = 13f;
    

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
        
        // INPUT UPDATE
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

        var doBubblePop = Input.GetKeyDown(KeyCode.B);

        // GROUNDED UPDATE
        var hitCount = Physics2D.OverlapBox(transform.TransformPoint(LocalCollider.offset), Vector2.one, 0f, new ContactFilter2D(), GlobalBuffers.ColliderBuffer);
        Assert.IsTrue(hitCount <= GlobalBuffers.ColliderBuffer.Length);

        _grounded = false;
        for (int i = 0; i < hitCount; i++)
        {
            var hitCollider = GlobalBuffers.ColliderBuffer[i];
            // ignore local collider
            if (LocalCollider == hitCollider)
            {
                continue;
            }

            _grounded = true;
            break;
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
        
        // APPLY BUBBLE POP
        if (doBubblePop)
        {
            BubblePopVFX.Play();
            _bubblePoppedTime = time;
            _bubblePopped = true;
            
            if (_jumpButton && time - _jumpButtonDownTime <= PreGroundedJumpWindow)
            {
                StartSuperJump();
            }
            else
            {
                StartBubblePop();
            }
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
        _jumped = true;
    }

    private void StartBubblePop()
    {
        Body.linearVelocityY = BubblePopVelocity;
        _jumped = false;
        _jumping = false;
    }
}
