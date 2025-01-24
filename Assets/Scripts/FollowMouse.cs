using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    private void Update()
    {
        var mainCam = MainCamera.Instance.CameraComponent;
        var pos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        transform.position = pos;
    }
}
