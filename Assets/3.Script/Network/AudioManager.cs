using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource systemSource;

    [Header("BGM")]
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip lobbyBGM;
    [SerializeField] private AudioClip stageBGM;
    [SerializeField] private AudioClip stage2BGM;
    [SerializeField] private AudioClip stage3BGM;

    [Header("SFX")]
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip clearSFX;
    [SerializeField] private AudioClip buttonSFX;
    [SerializeField] private AudioClip deadSFX;
    [SerializeField] private AudioClip canonHitSFX;

    [Header("System")]
    [SerializeField] private AudioClip uiSFX;

    Dictionary<string, AudioClip> sfxMap;

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
            return;
        }

        bgmSource.loop = true;
        InitSFX();
        ApplyVolumes();
    }

    private void InitSFX()
    {
        sfxMap = new()
        {
            { "Jump", jumpSFX },
            { "Clear", clearSFX },
            { "Button", buttonSFX },
            { "Dead", deadSFX },
            { "CanonHit", canonHitSFX }
        };
    }
    //BGM (로컬)
    #region BGM (Local)

    public void PlayBGM(string name)
    {
        bgmSource.clip = name switch
        {
            "Title" => titleBGM,
            "Lobby" => lobbyBGM,
            "Stage" => stageBGM,
            "Stage2" => stage2BGM,
            "Stage3" => stage3BGM,
            _ => null
        };

        if (bgmSource.clip != null)
            bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    #endregion

    #region LocalSFX (Local)

    public void PlayLocalSFX(string name)
    {
        if (!sfxMap.TryGetValue(name, out var clip)) return;
        sfxSource.PlayOneShot(clip);
    }

    #endregion

    #region System (Local)

    public void PlayUI() =>
        systemSource.PlayOneShot(uiSFX);

    #endregion

    //볼륨 적용
    public void ApplyVolumes()
    {
        float master = PlayerPrefs.GetInt("MasterVolume", 5) / 10f;
        float bgm = PlayerPrefs.GetInt("BGMVolume", 5) / 10f;
        float sfx = PlayerPrefs.GetInt("SFXVolume", 5) / 10f;

        Apply(master, bgm, sfx);
    }

    public void ApplyVolumesRealtime(int masterInt, int bgmInt, int sfxInt)
    {
        float master = masterInt / 10f;
        float bgm = bgmInt / 10f;
        float sfx = sfxInt / 10f;

        Apply(master, bgm, sfx);
    }

    // 실제 적용 로직 통합
    private void Apply(float m, float b, float s)
    {
        bgmSource.volume = m * b;
        sfxSource.volume = m * s;
        systemSource.volume = m * s;
    }
}
