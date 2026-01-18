using UnityEngine;
using UnityEngine.UI;

public class UIButtonSFX : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUI();
    }
}
