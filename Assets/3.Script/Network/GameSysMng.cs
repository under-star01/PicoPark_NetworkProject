using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameSystemManager : NetworkBehaviour
{
    public static GameSystemManager Instance;

    [Header("White Out UI")]
    [SerializeField] private Image whiteOutImage;
    [SerializeField] private float fadeDuration = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartWhiteOut()
    {
        StartCoroutine(WhiteOutRoutine());
    }

    public IEnumerator WhiteOutRoutine()
    {
        // UI È°¼ºÈ­
        whiteOutImage.gameObject.SetActive(true);

        Color c = whiteOutImage.color;
        c.a = 0f;
        whiteOutImage.color = c;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Clamp01(time / fadeDuration);
            whiteOutImage.color = c;
            yield return null;
        }

        c.a = 1f;
        whiteOutImage.color = c;
    }

    [ClientRpc]
    public void RpcNotifySystemMessage(string message)
    {
        // UIManager.Instance.ShowAlert(message);
    }
}