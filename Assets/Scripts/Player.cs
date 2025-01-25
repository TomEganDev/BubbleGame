using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BCC))]
public class Player : SingletonComponent<Player>
{
    public BCC BCC;
    public Rigidbody2D Body;
    public GameObject[] ChildLookups;
    public GameObject RendererRoot;
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
        
        BCC.enabled = false;
        IsDead = true;
        BCC.Body.bodyType = RigidbodyType2D.Kinematic;
        BCC.Body.linearVelocity = Vector2.zero;
        RendererRoot.SetActive(false);
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
        RendererRoot.SetActive(true);
    }
}
