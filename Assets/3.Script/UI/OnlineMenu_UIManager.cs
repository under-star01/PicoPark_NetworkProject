using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OnlineMenu_UIManager : MonoBehaviour
{
    private IA_Player playerInput;

    private enum UIState
    {
        Entry,          // 엔트리 화면(Enter만 허용)
        LobbyEntry,     // 로비 엔트리 화면(Enter/Space 둘 다 허용, 로그인 실행)
        Title,          // 타이틀 화면
        StageSelect,    // 스테이지 선택 메뉴(Enter/Space 둘 다 허용, 선택 실행)
    }

    [SerializeField]
    private UIState state = UIState.Entry; // 시작 상태 초기화


    [Header("Panel")] // Host, Join
    [SerializeField] private GameObject[] OnlineMenuButtons;

    [Header("Panel")] // HostMenu, JoinMenu
    [SerializeField] private GameObject[] LobbyEntryPanels;
    [SerializeField] private HostMenuController hostMenuController;

    [Header("SelectPanel")] //Start Game, Return, Finish Game
    [SerializeField] private GameObject[] TitlePanels;

    [Header("StagePanel")] // Stage1 ~ 6
    [SerializeField] private GameObject StageSelectPanel;

    private int panelIndex = 0;   // 옵션 - 사운드 패널 수직 인덱스 번호 (0: Master, 1: BGM, 2: SE, 3: OK/Cancel 등)
    private void Awake()
    {
        playerInput = new IA_Player(); // 뉴인풋받기

    }

    void Start()
    {
        SetEntryState(); // Press Enter 상태로 시작하도록 설정
    }

    private void OnEnable()
    {

        playerInput.MenuUI.Left.performed += MoveLeft;
        playerInput.MenuUI.Right.performed += MoveRight;

        playerInput.MenuUI.Up.performed += MoveUp;
        playerInput.MenuUI.Down.performed += MoveDown;

        playerInput.MenuUI.Enter.performed += Select;
        playerInput.MenuUI.Space.performed += Select;

        playerInput.MenuUI.ESC.performed += ESC;

        playerInput.Enable();
    }

    private void OnDisable()
    {

        playerInput.MenuUI.Left.performed -= MoveLeft;
        playerInput.MenuUI.Right.performed -= MoveRight;

        playerInput.MenuUI.Up.performed -= MoveUp;
        playerInput.MenuUI.Down.performed -= MoveDown;

        playerInput.MenuUI.Enter.performed -= Select;
        playerInput.MenuUI.Space.performed -= Select;

        playerInput.MenuUI.ESC.performed -= ESC;

        playerInput.Disable();
    }

    // ========== 상태 전환 ==========

    private void SetEntryState()
    {
        state = UIState.Entry; // Press Enter 상태일 때

        foreach (GameObject Panel in LobbyEntryPanels)
        {
            Panel.SetActive(false);
        }
        foreach (GameObject SelectPanel in TitlePanels)
        {
            SelectPanel.SetActive(false);
        }
        if (StageSelectPanel != null)
        {
            StageSelectPanel.SetActive(false);
        }
        panelIndex = 0;
        OnlineMenuButtons[panelIndex].GetComponent<ButtonHover>().OnFocus();
    }

    private void ShowPanelUI(int panelIdx)
    {

        panelIndex = 0; // 항상 처음 항목부터 시작
        DisablePanel();
       // LobbyEntryPanels[panelIdx].SetActive(true);
    }

    private void DisablePanel()
    {
        if (LobbyEntryPanels != null)
        {
            foreach (GameObject Panel in LobbyEntryPanels)
            {
                Panel.SetActive(false);
            }
        }
    }



    // ========== 입력 처리 ==========

    private void MoveLeft(InputAction.CallbackContext context)
    {

        switch (state)
        {
            case UIState.Entry:
                if (panelIndex == 1)
                {
                    panelIndex = 0;
                    OnlineMenuButtons[panelIndex].GetComponent<ButtonHover>().OnFocus();
                }
                break;
            case UIState.LobbyEntry:
                if (LobbyEntryPanels[0].activeSelf) // 호스트 패널이 켜져 있으면
                {
                    if (panelIndex.Equals(0)) // MaxPlayer
                    {
                        hostMenuController.OnMaxPlayerLeft();
                    }
                    else if (panelIndex.Equals(1)) // Join
                    {
                        hostMenuController.OnJoinProgress();
                    }
                    else if (panelIndex.Equals(2)) // Hat
                    {
                        hostMenuController.OnHatLeft();
                    }
                    else // Color
                    {
                        hostMenuController.OnColorLeft();
                    }
                }
                else
                {
                    if (panelIndex.Equals(1) || panelIndex.Equals(2)) //
                    {
                        panelIndex = 0;
                        // 입력 포커스
                    }
                }
                break;
            case UIState.Title:

                break;
            case UIState.StageSelect:
                break;
        }


    }


    private void MoveRight(InputAction.CallbackContext context)
    {

        switch (state)
        {
            case UIState.Entry:
                if (panelIndex.Equals(0))
                {
                    panelIndex = 1;
                    OnlineMenuButtons[panelIndex].GetComponent<ButtonHover>().OnFocus();
                }
                break;
            case UIState.LobbyEntry:
                if (LobbyEntryPanels[0].activeSelf) // 호스트 패널이 켜져 있으면
                {
                    if (panelIndex.Equals(0)) // MaxPlayer
                    {
                        hostMenuController.OnMaxPlayerRight();
                    }
                    else if (panelIndex.Equals(1)) // Join
                    {
                        hostMenuController.OnJoinProgress();
                    }
                    else if (panelIndex.Equals(2)) // Hat
                    {
                        hostMenuController.OnHatRight();
                    }
                    else // Color
                    {
                        hostMenuController.OnColorRight();
                    }
                }
                else
                {
                    if (panelIndex.Equals(1) || panelIndex.Equals(2)) //
                    {
                        panelIndex = 0;
                        // 입력 포커스
                    }
                }
                break;
            case UIState.Title:

                break;
            case UIState.StageSelect:
                break;
        }
    }

    private void MoveUp(InputAction.CallbackContext context)
    {
        if (state != UIState.LobbyEntry || state != UIState.StageSelect) return;
        panelIndex--;

        int maxIndex = LobbyEntryPanels[0].activeSelf ? 4 : 1;

        if (panelIndex < 0)
        {
            panelIndex = maxIndex;
        }
    }

    private void MoveDown(InputAction.CallbackContext context)
    {
        if (state != UIState.LobbyEntry || state != UIState.StageSelect) return;
        panelIndex++;
        switch (state)
        {
            case UIState.LobbyEntry:
                if (LobbyEntryPanels[0].activeSelf)
                {
                    if (panelIndex > 6)
                    {
                        panelIndex = 0;
                    }

                }
                break;
            case UIState.StageSelect:
                break;
        }

    }

    private void Select(InputAction.CallbackContext context)
    {
        switch (state)
        {
            case UIState.Entry:
                OnlineMenuButtons[panelIndex].GetComponent<Button>().onClick.Invoke();
                break;

            case UIState.LobbyEntry:
                // 로그인 화면: Enter/Space 둘 다 로그인 실행

                break;


        }
    }

    private void ESC(InputAction.CallbackContext context)
    {
        //TitleMenu에서 뒤로가기 등 기능 추가
        switch (state)
        {
            case UIState.Entry:
                break;
            case UIState.LobbyEntry:
                break;
            case UIState.Title:
                break;
            case UIState.StageSelect:
                break;
        }

    }
}
