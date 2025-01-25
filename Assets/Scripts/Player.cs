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
    
    private void Reset()
    {
        BCC = GetComponent<BCC>();
        Body = GetComponent<Rigidbody2D>();
    }

    public bool IsPlayer(GameObject queryGameObject)
    {
        if (gameObject == queryGameObject)
        {
            return true;
        }

        return ChildLookups.Contains(queryGameObject);
    }
}
