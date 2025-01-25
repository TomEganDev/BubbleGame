using UnityEngine;

public class PlayerFacingBehaviour : MonoBehaviour
{
    private bool _reversed;
    
    private void LateUpdate()
    {
        var mousePos = MainCamera.Instance.CameraComponent.ScreenToWorldPoint(Input.mousePosition);
        var thisPos = transform.position;
        if (_reversed && mousePos.x > thisPos.x)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            _reversed = false;
        }
        else if(!_reversed && mousePos.x < thisPos.x)
        {
            transform.rotation = Quaternion.identity;
            _reversed = true;
        }
    }
}
