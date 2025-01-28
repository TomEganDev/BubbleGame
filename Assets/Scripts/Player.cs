using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BCC))]
public class Player : SingletonComponent<Player>
{
    public BCC BCC;
    public BubbleGun BubbleGun;
    public Rigidbody2D Body;
    public GameObject[] ChildLookups;
    public GameObject RendererRoot;
    public Animator PlayerAnimator;
    public ParticleSystem DeathVFX;

    private bool _reversed;
    
    [field: SerializeField]
    public bool IsDead { get; private set; }
    
    private void Reset()
    {
        IsDead = false;
        BCC = GetComponent<BCC>();
        Body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        IsDead = false;
    }

    public bool IsPlayer(GameObject queryGameObject)
    {
        if (gameObject == queryGameObject)
        {
            return true;
        }

        return ChildLookups.Contains(queryGameObject);
    }

    public void Die()
    {
        if (IsDead)
        {
            Debug.LogWarning($"[{Time.frameCount}] Die() called while dead");
            return;
        }
        
        BCC.StopDead();
        BCC.enabled = false;
        IsDead = true;
        BCC.Body.bodyType = RigidbodyType2D.Static;
        BubbleGun.enabled = false;
        
        RendererRoot.SetActive(false);
        if (DeathVFX != null)
        {
            DeathVFX.Play();
        }
    }

    public void OnRespawn()
    {
        if (!IsDead)
        {
            Debug.LogWarning($"[{Time.frameCount}] OnRespawn() called while alive");
            return;
        }
        
        BCC.enabled = true;
        IsDead = false;
        BCC.Body.bodyType = RigidbodyType2D.Dynamic;
        BCC.Body.linearVelocity = Vector2.zero;
        BubbleGun.enabled = true;
        RendererRoot.SetActive(true);
    }

    private void LateUpdate()
    {
        var mousePos = MainCamera.Instance.CameraComponent.ScreenToWorldPoint(Input.mousePosition);
        var thisPos = transform.position;
        if (_reversed && mousePos.x > thisPos.x)
        {
            RendererRoot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            _reversed = false;
        }
        else if(!_reversed && mousePos.x < thisPos.x)
        {
            RendererRoot.transform.rotation = Quaternion.identity;
            _reversed = true;
        }
        
        // animator params
        PlayerAnimator.SetFloat("Speed_X", BCC.LocalVelocity.x * (_reversed ? -1f : 1f));
        PlayerAnimator.SetFloat("Speed_Y", BCC.LocalVelocity.y);
        PlayerAnimator.SetBool("Grounded", BCC.Grounded);
    }
}
