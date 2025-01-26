using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class WaypointMovement : MonoBehaviour
{
    public enum LoopBehaviour : byte
    {
        Stop,
        PingPong,
        Repeat,
        Loop // Last point goes toward first
    }
    
    [SerializeField] private LoopBehaviour _loopBehaviour = LoopBehaviour.PingPong;
    [SerializeField] private Rigidbody2D _body;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private bool _moving = true;
    public Transform[] Waypoints;
    [SerializeField] private Vector3[] _runtimeWaypoints;
    

    private bool _reversing;
    private int _currentWaypointIndex;
    
    private void Reset()
    {
        _body = GetComponent<Rigidbody2D>();
        _body.bodyType = RigidbodyType2D.Kinematic;
        _body.gravityScale = 0f;
    }

    private void Awake()
    {
        Assert.IsTrue(Waypoints.Length >= 1);
        
        _runtimeWaypoints = new Vector3[Waypoints.Length + 1];
        _runtimeWaypoints[0] = transform.position;
        for (int i = 0; i < Waypoints.Length; i++)
        {
            _runtimeWaypoints[i + 1] = Waypoints[i].position;
        }
    }

    private void Update()
    {
        if (!_moving)
        {
            return;
        }
        
        var deltaTime = Time.deltaTime;
        var startPosition = transform.position;
        var currentPosition = startPosition;
        var remainingMovement = _speed * deltaTime;
        var targetPosition = _runtimeWaypoints[_currentWaypointIndex];
        var waypointDistance = Vector3.Distance(targetPosition, currentPosition);
        var teleporting = false;
        var safetyCount = 0;
        while (waypointDistance < remainingMovement)
        {
            safetyCount++;
            if (safetyCount == 10000)
            {
                Debug.LogError($"Didn't exaust movement in 10000 tries, remaining:{remainingMovement}");
                break;
            }
            
            remainingMovement -= waypointDistance;
            var isEnd = _currentWaypointIndex == _runtimeWaypoints.Length - 1;
            if (isEnd && _loopBehaviour == LoopBehaviour.Stop)
            {
                _body.MovePosition(targetPosition);
                StopMoving();
                return;
            }
            
            // get next waypoint
            _currentWaypointIndex = GetNextWaypointIndex();
            currentPosition = targetPosition;
            targetPosition = _runtimeWaypoints[_currentWaypointIndex];
            waypointDistance = Vector3.Distance(targetPosition, currentPosition);

            if (isEnd && _loopBehaviour == LoopBehaviour.Repeat)
            {
                currentPosition = targetPosition;
                _currentWaypointIndex = GetNextWaypointIndex();
                targetPosition = _runtimeWaypoints[_currentWaypointIndex];
                teleporting = true;
            }
        }

        var translation = (targetPosition - currentPosition).normalized * remainingMovement;
        var finalPosition = currentPosition + translation;
        
        if (teleporting)
        {
            _body.position = finalPosition;
        }
        else
        {
            _body.MovePosition(finalPosition);
        }
    }

    private int GetNextWaypointIndex()
    {
        switch (_loopBehaviour)
        {
            case LoopBehaviour.Stop:
            {
                return _currentWaypointIndex + 1;
            }
            case LoopBehaviour.PingPong:
            {
                if (_reversing)
                {
                    if (_currentWaypointIndex == 0)
                    {
                        _reversing = false;
                        return 1;
                    }
                    else
                    {
                        return _currentWaypointIndex - 1;
                    }
                }
                else // forward travel
                {
                    if (_currentWaypointIndex == _runtimeWaypoints.Length - 1)
                    {
                        _reversing = true;
                        return _currentWaypointIndex - 1;
                    }
                    else
                    {
                        return _currentWaypointIndex + 1;
                    }
                }
            }
            case LoopBehaviour.Repeat:
            case LoopBehaviour.Loop:
            {
                if (_currentWaypointIndex == _runtimeWaypoints.Length - 1)
                {
                    return 0;
                }
                else
                {
                    return _currentWaypointIndex + 1;
                }
            }
            default:
            {
                throw new NotImplementedException($"LoopBehaviour {_loopBehaviour} is not implemented.");
            }
        }
    }
    
    public void StartMoving()
    {
        if (_moving)
        {
            return;
        }

        _moving = true;
    }

    public void StopMoving()
    {
        if (!_moving)
        {
            return;
        }
        
        _body.linearVelocity = Vector3.zero;
        _moving = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (Waypoints == null || Waypoints.Length == 0)
        {
            return;
        }
        
        Gizmos.color = Color.yellow;
        if (Application.isPlaying)
        {
            for (int i = 0; i < _runtimeWaypoints.Length - 1; i++)
            {
                Gizmos.DrawLine(_runtimeWaypoints[i], _runtimeWaypoints[i + 1]);
            }
        }
        else
        {
            for (int i = -1; i < Waypoints.Length - 1; i++)
            {
                if ((i > -1 && Waypoints[i] == null) || Waypoints[i + 1] == null)
                {
                    continue;
                }

                var p1 = i == -1 ? transform.position : Waypoints[i].position;
                var p2 = Waypoints[i + 1].position;
                Gizmos.DrawLine(p1, p2);
            }
        }

        if (_loopBehaviour == LoopBehaviour.Loop && Waypoints.Length >= 1)
        {
            if (Waypoints[^1] != null)
            {
                Gizmos.DrawLine(Application.isPlaying ? _runtimeWaypoints[0] : transform.position, GetEndWaypointPosition());
            }
        }
    }

    private Vector3 GetEndWaypointPosition()
    {
        return Application.isPlaying ? _runtimeWaypoints[^1] : Waypoints[^1].position;
    }
}
