using UnityEngine;

public class RepeatingSpawner : MonoBehaviour
{
    [SerializeField] private bool _spawning = true;
    [SerializeField] private float _spawnInterval = 1f;

    private float _lastSpawnTime;
    
    public void StartSpawning()
    {
        if (_spawning)
        {
            return;
        }

        _spawning = true;
    }

    public void StopSpawning()
    {
        if (!_spawning)
        {
            return;
        }
        
        _spawning = false;
    }
}
