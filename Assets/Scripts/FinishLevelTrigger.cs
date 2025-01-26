using UnityEngine;

public class FinishLevelTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Player.Instance.IsPlayer(other.gameObject))
        {
            return;
        }
        
        GameManager.Instance.OnFinishLevel();
    }
}
