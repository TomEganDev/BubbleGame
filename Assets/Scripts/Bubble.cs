using UnityEngine;
using UnityEngine.Assertions;

public class Bubble : MonoBehaviour
{
    public enum State : byte
    {
        Floor,
        Wall,
        Roof,
        Floating
    }
    
    [field: SerializeField]
    public State CurrentState { get; private set; }

    public ParticleSystem BubblePopVFX_Prefab;
    public CircleCollider2D BubbleTrigger;
    public Rigidbody2D Body;
    private Rigidbody2D _attachedToBody;
    public float FloatRiseSpeed = 0.01f;

    private int _spawnTick;
    private float _spawnTime;
    public int SpawnTick => _spawnTick;

    private BubbleReceiver _receiver;
    private bool _popped;
    
    private static Bubble[] _bubbleSlots = new Bubble[3];
    
    public void OnSpawn(State state, Rigidbody2D _attachedBody)
    {
        // set state
        CurrentState = state;

        // initialize floating
        if (CurrentState == State.Floating)
        {
            Body.bodyType = RigidbodyType2D.Dynamic;
            Body.gravityScale = 0f;
        }
        else
        {
            _attachedToBody = _attachedBody;
            if (_attachedToBody != null)
            {
                Body.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        
        
        // bubble active queue logic
        _spawnTick = Time.frameCount;
        _spawnTime = Time.time;
        var oldestTick = int.MaxValue;
        var oldestIndex = -1;
        for (int i = 0; i < _bubbleSlots.Length; i++)
        {
            if (_bubbleSlots[i] == null)
            {
                //Debug.Log($"Bubble claim FREE slot {i}");
                _bubbleSlots[i] = this;
                return;
            }
            if(_bubbleSlots[i].SpawnTick <= oldestTick)
            {
                oldestTick = _bubbleSlots[i].SpawnTick;
                oldestIndex = i;
            }
        }
        
        var destroyingBubble = _bubbleSlots[oldestIndex];
        //Debug.Log($"Bubble claim TAKEN slot {oldestIndex}");
        _bubbleSlots[oldestIndex] = this;
        var vfxPos = destroyingBubble.transform.position;
        Instantiate(BubblePopVFX_Prefab, vfxPos, Quaternion.identity);
        Destroy(destroyingBubble.gameObject);
    }

    private void Update()
    {
        if (_attachedToBody != null)
        {
            Body.linearVelocity = _attachedToBody.linearVelocity;
        }
        
        if (CurrentState == State.Floating)
        {
            Body.AddForce(Vector2.up * FloatRiseSpeed);
        }
    }

    public void SetReceiver(BubbleReceiver receiver)
    {
        _receiver = receiver;
        transform.localScale = Vector3.one * (receiver.BubbleRadius / BubbleTrigger.radius);
    }

    public void Pop()
    {
        if (_receiver != null)
        {
            Assert.IsTrue(CurrentState == State.Floating);
            
            _receiver.OnDetach();
        }
        Instantiate(BubblePopVFX_Prefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
        _popped = true;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // are we destroying
        if (_popped)
        {
            return;
        }
        
        if (CurrentState == State.Floating && Time.time - _spawnTime > 0.1f)
        {
            var collidingWithReceiver = _receiver != null && other.gameObject == _receiver.gameObject;
            if (!collidingWithReceiver)
            {
                Pop();
                return;
            }
        }
        
        var isPopper = BubblePopperLookup.IsPopper(other.gameObject);
        if (isPopper)
        {
            Pop();
            return;
        }
        
        var isPlayer = Player.Instance.IsPlayer(other.gameObject);
        if (!isPlayer)
        {
            return;
        }
        
        // TODO - fix hax properly for rocket jump spawning in floor thinking its roof
        if (CurrentState == State.Roof && Time.frameCount == _spawnTick)
        {
            //Debug.LogWarning($"[{Time.frameCount}] HACKY_ROCKET_DETECTED Forcing Floor state");
            CurrentState = State.Floor;
        }
        
        Player.Instance.BCC.OnBubblePop(this);
        MainCamera.Instance.ScreenShake();
        Pop();
    }
}
