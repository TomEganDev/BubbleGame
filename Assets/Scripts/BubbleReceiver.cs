using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class BubbleReceiver : MonoBehaviour
{
    private static Dictionary<GameObject, BubbleReceiver> _lookup = new(256);

    [SerializeField] [CanBeNull] private ParentConstraint _parentConstraint;
    [SerializeField] private UnityEvent<Bubble> _onBubbleReceived;
    [SerializeField] private UnityEvent _onBubbleDestroyed;

    public float BubbleRadius = 0.6f;

    private Bubble _bubble;
    private bool _received;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private void OnPlayModeEnter()
    {
        _lookup.Clear();
    }
    
    private void OnEnable()
    {
        _lookup.Add(gameObject, this);
    }

    private void OnDisable()
    {
        _lookup.Remove(gameObject);
    }

    public void OnDetach()
    {
        // todo - fix this
        if (_received == false)
        {
            Debug.LogWarning($"[{Time.frameCount}] OnDetach() called without attachment. This shouldn't happen but it can.");
            return;
        }
        
        _received = false;
        if (_parentConstraint != null)
        {
            _parentConstraint.RemoveSource(0);
            _parentConstraint.constraintActive = false;
        }
        _onBubbleDestroyed.Invoke();
    }
    
    public void OnAttach(Bubble bubble)
    {
        _received = true;
        _bubble = bubble;
        _onBubbleReceived.Invoke(bubble);
        if (_parentConstraint != null)
        {
            _parentConstraint.AddSource(new ConstraintSource
            {
                sourceTransform = bubble.transform,
                weight = 1f
            });
            _parentConstraint.constraintActive = true;
        }
    }

    public static bool TryGetReceiver(GameObject gameObject, out BubbleReceiver receiver)
    {
        if (!_lookup.TryGetValue(gameObject, out BubbleReceiver bubbleReceiver))
        {
            receiver = null;
            return false;
        }

        receiver = bubbleReceiver;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, BubbleRadius);
    }
}
