using UnityEngine;

public class Bubble : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
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
