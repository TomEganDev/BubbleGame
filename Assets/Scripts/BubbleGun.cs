using System;
using UnityEngine;

public class BubbleGun : MonoBehaviour
{
    public PreBubble PreBubblePrefab;
    public Transform FiringPoint;
    
    private void Update()
    {
        transform.LookAt2D_Mouse(Input.mousePosition, MainCamera.Instance.CameraComponent);

        if (Input.GetMouseButtonDown(0)) // fire
        {
            FiringPoint.GetPositionAndRotation(out var position, out var rotation);
            var projectile = Instantiate(PreBubblePrefab, position, rotation);
            
        }
    }
}
