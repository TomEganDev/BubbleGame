using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Transform CursorRenderer;
    
    private void OnApplicationFocus(bool hasFocus)
    {
        Debug.Log($"Application focus: {hasFocus}");
        Cursor.visible = !hasFocus;
        Cursor.lockState = hasFocus ? CursorLockMode.Confined : CursorLockMode.None;
        CursorRenderer.gameObject.SetActive(hasFocus);
    }

    private void Update()
    {
        CursorRenderer.transform.position = Input.mousePosition;
    }
}
