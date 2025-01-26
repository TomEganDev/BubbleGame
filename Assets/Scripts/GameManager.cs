using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : SingletonComponent<GameManager>
{
    public enum State : byte
    {
        Playing,
        Dead,
        LevelFinished
    }

    public State CurrentState => _currentState;
    [SerializeField] private State _currentState = State.Playing;

    private float _timeOfDeath;
    public float RespawnTime = 0.5f;

    [SerializeField] private Vector3 _respawnPosition;

    private float _finishLevelTime;

    private void Start()
    {
        _currentState = State.Playing;
        SetRespawnPosition(Player.Instance.transform.position);
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Playing:
                Update_Playing();
                break;
            case State.Dead:
                Update_Dead();
                break;
            case State.LevelFinished:
                Update_LevelFinished();
                break;
        }
    }

    private void Update_Playing()
    {
        if (Player.Instance.IsDead)
        {
            _currentState = State.Dead;
            _timeOfDeath = Time.time;
        }
    }

    private void Update_Dead()
    {
        if (Time.time - _timeOfDeath >= RespawnTime)
        {
            Player.Instance.transform.position = _respawnPosition;
            Player.Instance.OnRespawn();
            _currentState = State.Playing;
        }
    }

    private void Update_LevelFinished()
    {
        
    }

    public void SetRespawnPosition(Vector3 position)
    {
        _respawnPosition = position;
    }

    public void OnFinishLevel()
    {
        Assert.IsTrue(CurrentState == State.Playing);
        
        _currentState = State.LevelFinished;
        _finishLevelTime = Time.time;

        Player.Instance.BCC.enabled = false;
        Player.Instance.BCC.Body.simulated = false;
        Player.Instance.BCC.Body.linearVelocity = Vector3.zero;
        
        Debug.Log($"[{Time.frameCount}] FINISHED_LEVEL");
    }
}
