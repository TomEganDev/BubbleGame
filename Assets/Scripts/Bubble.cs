using UnityEngine;

public class Bubble : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var isPlayer = Player.Instance.IsPlayer(other.gameObject);
        if (!isPlayer)
        {
            return;
        }
        
        Player.Instance.BCC.OnBubblePop();
        Destroy(gameObject);
    }
}
