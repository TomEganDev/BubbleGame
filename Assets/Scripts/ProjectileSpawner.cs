using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public Projectile Prefab;
    public bool Looping = true;
    public float LoopTime = 1f;
    public bool SetVelocityToUp = true;
    public bool AlignRotation = true;
    
    private float _spawnTime;

    private void Update()
    {
        if (!Looping)
        {
            return;
        }

        if (LoopTime <= 0f)
        {
            StopLooping();
            return;
        }
        
        if (Time.time - _spawnTime >= LoopTime)
        {
            Spawn();
        }
    }

    public void StartLooping()
    {
        if (Looping)
        {
            return;
        }

        Looping = true;
    }

    public void StopLooping()
    {
        if (!Looping)
        {
            return;
        }

        Looping = false;
    }
    
    public void Spawn()
    {
        var rotation = AlignRotation ? transform.rotation : Quaternion.identity;
        var projectile = Instantiate(Prefab, transform.position, rotation);
        if (SetVelocityToUp)
        {
            projectile.SetVelocityDirection(transform.up);
        }
        _spawnTime = Time.time;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.up * 100f);
    }
}
