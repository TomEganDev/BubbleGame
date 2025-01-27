using UnityEngine;
using UnityEngine.Events;

public class KillPlayerTrigger : MonoBehaviour
{
    public UnityEvent OnPlayerKilled;

    private void OnEnable()
    {
        BubblePopperLookup.RegisterBubblePopper(gameObject);
    }

    private void OnDisable()
    {
        BubblePopperLookup.UnregisterBubblePopper(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Player.Instance.IsPlayer(other.gameObject))
        {
            return;
        }

        if (Player.Instance.IsDead)
        {
            //Debug.LogWarning($"[{Time.frameCount}] {name}.KillPlayerTrigger.OnTriggerEnter2D called while dead");
            return;
        }
        
        Player.Instance.Die();
        OnPlayerKilled.Invoke();
    }
}
