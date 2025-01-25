using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class PlayerDebugUI : MonoBehaviour
{
    public TextMeshProUGUI Label;

    private void Reset()
    {
        Label = GetComponent<TextMeshProUGUI>();
    }

    private void LateUpdate()
    {
        transform.position = MainCamera.Instance.CameraComponent.WorldToScreenPoint(Player.Instance.transform.position);
        Label.text = Player.Instance.BCC.Body.linearVelocityY.ToString("N0");
    }
}
