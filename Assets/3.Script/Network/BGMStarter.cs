using UnityEngine;

public class BGMStarter : MonoBehaviour
{
    [Header("BGM ¼³Á¤")]
    [SerializeField] private string bgmName = "Stage"; // Title, Lobby, Stage, Stage2, Stage3
    [SerializeField] private bool playOnStart = true;

    private void Start()
    {
        if (playOnStart && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(bgmName);
        }
    }
}