using System;
using UnityEngine;

[Serializable]
public class PlayerCustomizeData
{
    public int colorIndex;
    public int hatIndex;
}

public class LobbyCustomCache : MonoBehaviour
{
    public static LobbyCustomCache Instance;

    public PlayerCustomizeData myCustomizeData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
