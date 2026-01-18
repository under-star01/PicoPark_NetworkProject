using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class TitleMenu : MonoBehaviour
{
    public int currentIndex = 0;
    [SerializeField] GameObject[] menu;

    [System.Serializable]
    public class VolumeControl
    {
        public string name;          // 볼륨항목 라벨링
        public int value;               // 0~10
        public Button leftBtn;
        public Button rightBtn;
        public Image display;             // 0~10 값에 따른 스프라이트
        public Sprite[] valueSprites;     // NUM_0 ~ NUM_10
    }

    public VolumeControl masterVolume;
    public VolumeControl bgmVolume;
    public VolumeControl sfxVolume;

    // 기존 볼륨을 백업해둘 변수
    private int backupMaster, backupBGM, backupSFX;

    // 옵션/종료 패널
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Button OKButton;
    [SerializeField] private Button CancelButton;

    [SerializeField] private GameObject exitPanel;
    [SerializeField] private Button ExitOKButton;
    [SerializeField] private Button ExitCancelButton;

    [Header("Selection Colors")]
    [SerializeField] private Color selectedColor = Color.black;
    [SerializeField] private Color normalColor = Color.white;

    [Header("다음 씬")]
    // 전환용 씬 이름
    [SerializeField] private string nextSceneName = "NextScene";

    void Start()
    {
        menu[currentIndex].SetActive(true);

        // 초기 값 로드
        LoadVolumes();
        UpdateVolumeDisplay(masterVolume);
        UpdateVolumeDisplay(bgmVolume);
        UpdateVolumeDisplay(sfxVolume);
    }

    public void MoveRight()
    {
        menu[currentIndex].SetActive(false);
        currentIndex++;
        if (currentIndex >= menu.Length)
        {
            currentIndex = 0;
        }
        menu[currentIndex].SetActive(true);
    }

    public void MoveLeft()
    {
        menu[currentIndex].SetActive(false);
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = menu.Length - 1;
        }
        menu[currentIndex].SetActive(true);
    }

    // 각 항목 선택했을 때 작동할 메서드 연결
    public void Select()
    {
        switch (currentIndex)
        {
            case 0: Online(); break;
            case 1: OpenOption(); break;
            case 2: OpenExit(); break;
        }
    }

    //위에서 연결될 메서드들
    void Online() // 온라인 버튼 눌렀으면 씬넘겨
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void OpenOption() // 옵션 버튼 눌렀으면 옵션 패널 켜
    {
        if (optionPanel != null)
        {
            backupMaster = masterVolume.value;
            backupBGM = bgmVolume.value;
            backupSFX = sfxVolume.value;

            optionPanel.SetActive(true);
        }
    }

    public void OpenExit() // 종료 버튼 눌렀으면 종료 패널 켜
    {
        if (exitPanel != null) exitPanel.SetActive(true);
    }

    // 볼륨 컨트롤 로직
    void LoadVolumes() // 볼륨의 기본 값은 5로 주고, 0부터 10까지 조절 가능함.
    {
        masterVolume.value = Mathf.Clamp(PlayerPrefs.GetInt("MasterVolume", 5), 0, 10);
        bgmVolume.value = Mathf.Clamp(PlayerPrefs.GetInt("BGMVolume", 5), 0, 10);
        sfxVolume.value = Mathf.Clamp(PlayerPrefs.GetInt("SFXVolume", 5), 0, 10);
    }

    // 
    void UpdateVolumeDisplay(VolumeControl vc)
    {
        int idx = Mathf.Clamp(vc.value, 0, 10); // 볼륨의 범위
        if (vc.valueSprites.Length > idx)
        {
            vc.display.sprite = vc.valueSprites[idx]; // 볼륨에 맞춰 스프라이트 출력해!
        }
    }

    // 0~10 증가/감소
    public void VolumeLeft(VolumeControl vc) // 왼쪽 버튼(볼륨 감소)
    {
        vc.value = Mathf.Clamp(vc.value - 1, 0, 10);
        UpdateVolumeDisplay(vc); // 화면에서도 바꿔주고
        AudioManager.Instance.ApplyVolumes();
        //SaveVolume(vc); // 값도 저장해서 다시 켜도 유지되게 해줘
    }

    public void VolumeRight(VolumeControl vc) // 오른쪽 버튼(볼륨 증가)
    {
        vc.value = Mathf.Clamp(vc.value + 1, 0, 10);
        UpdateVolumeDisplay(vc); // 화면에 뜨는거 바꾸고!
        AudioManager.Instance.ApplyVolumes();
        //SaveVolume(vc); // 유지시켜!
    }

    public void MasterVolumeLeft()
    {
        VolumeLeft(masterVolume);
    }
    public void MasterVolumeRight()
    {
        VolumeRight(masterVolume);
    }
    public void BGMVolumeLeft()
    {
        VolumeLeft(bgmVolume);
    }
    public void BGMVolumeRight()
    {
        VolumeRight(bgmVolume);
    }
    public void SFXVolumeLeft()
    {
        VolumeLeft(sfxVolume);
    }
    public void SFXVolumeRight()
    {
        VolumeRight(sfxVolume);
    }

    void SaveVolume(VolumeControl vc)
    {
        if (vc == masterVolume) //마스터 볼륨 저장
        {
            PlayerPrefs.SetInt("MasterVolume", vc.value);
        }
        else if (vc == bgmVolume) // BGM 볼륨 저장
        {
            PlayerPrefs.SetInt("BGMVolume", vc.value);
        }
        else if (vc == sfxVolume) // sfx 볼륨 저장
        {
            PlayerPrefs.SetInt("SFXVolume", vc.value);
        }
        PlayerPrefs.Save();
    }

    // 닫기/확인 버튼
    public void CloseOptionPanel()
    {
            optionPanel.SetActive(false); // 옵션창 꺼!
    }
    public void CloseExitNo()
    {
            exitPanel.SetActive(false); // 종료창 꺼!
    }
    public void ConfirmExitYes()
    {
        Application.Quit(); // 게임 꺼!
    }

    public void UpdatePanelSelection(int panelIndex)
    {
        // 모든 항목을 일단 기본색으로 초기화
        ResetAllColors();
        if (optionPanel.activeSelf)
        {
            // 선택된 인덱스에 따라 색상 강조
            switch (panelIndex)
            {
                case 0: // Master
                    SetVolumeControlColor(masterVolume, selectedColor);
                    masterVolume.display.GetComponent<ButtonHover>().OnFocus();
                    break;
                case 1: // BGM
                    SetVolumeControlColor(bgmVolume, selectedColor);
                    bgmVolume.display.GetComponent<ButtonHover>().OnFocus();
                    break;
                case 2: // SFX
                    SetVolumeControlColor(sfxVolume, selectedColor);
                    sfxVolume.display.GetComponent<ButtonHover>().OnFocus();
                    break;
                case 3: // OK 버튼 (있다면)
                    OKButton.GetComponent<ButtonHover>().OnFocus();
                    break;
                case 4: // Cancel 버튼 (있다면)
                    CancelButton.GetComponent<ButtonHover>().OnFocus();
                    break;
            }
        }
        else if (exitPanel.activeSelf)
        {
            switch (panelIndex)
            {
                case 0: ExitOKButton.GetComponent<ButtonHover>().OnFocus(); break;
                case 1: ExitCancelButton.GetComponent<ButtonHover>().OnFocus(); break;
            }
        }

    }

    private void SetVolumeControlColor(VolumeControl vc, Color color)
    {
        if (vc.leftBtn != null) vc.leftBtn.GetComponent<Image>().color = color;
        if (vc.rightBtn != null) vc.rightBtn.GetComponent<Image>().color = color;
        
    }

    private void ResetAllColors()
    {
        SetVolumeControlColor(masterVolume, normalColor);
        SetVolumeControlColor(bgmVolume, normalColor);
        SetVolumeControlColor(sfxVolume, normalColor);
    }

    // [OK 버튼용] 실제로 파일에 저장
    public void ConfirmOption()
    {
        PlayerPrefs.SetInt("MasterVolume", masterVolume.value);
        PlayerPrefs.SetInt("BGMVolume", bgmVolume.value);
        PlayerPrefs.SetInt("SFXVolume", sfxVolume.value);
        PlayerPrefs.Save();
        CloseOptionPanel();
    }

    // [Cancel 버튼용] 변경된 값을 무시하고 백업 데이터로 복구
    public void CancelOption()
    {
        masterVolume.value = backupMaster;
        bgmVolume.value = backupBGM;
        sfxVolume.value = backupSFX;

        // UI 다시 업데이트
        UpdateVolumeDisplay(masterVolume);
        UpdateVolumeDisplay(bgmVolume);
        UpdateVolumeDisplay(sfxVolume);

        CloseOptionPanel();
    }

}
