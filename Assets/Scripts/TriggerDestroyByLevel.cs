using UnityEngine;

public class TriggerDestroyByLevel : MonoBehaviour
{
    public bool TreatDefaultAsLevel = true;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var layer = other.gameObject.layer;
        if (layer == (int)BubbleLayers.Level_Geometry)
        {
            Destroy(gameObject);
        }
        if (TreatDefaultAsLevel && layer == (int)BubbleLayers.Default)
        {
            Destroy(gameObject);
        }
    }
}