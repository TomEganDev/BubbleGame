using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class BCCDebugUI : MonoBehaviour
{
    public TextMeshProUGUI Label;

    private void Reset()
    {
        Label = GetComponent<TextMeshProUGUI>();
    }

    private void LateUpdate()
    {
        if (Player.Instance == null)
        {
            return;
        }

        var bcc = Player.Instance.BCC;
        Label.text = $"Grounded: {bcc.Grounded}\nJumped: {bcc.Jumped}\nJumping: {bcc.Jumping}\nBubbled: {bcc.BubblePopped}\nVelocity: {bcc.Body.linearVelocity:N1}";
    }
}
