using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Mirror;

public class OnlineMenu_UIManager : MonoBehaviour
{
    private IA_Player playerInput;

    // 기존 변수들 아래에 추가
    private PlayerInput _localPlayerController;


    private enum UIState
    {
        Entry,          // 엔트리 화면(Enter만 허용)
        LobbyEntry,     // 로비 엔트리 화면(Enter/Space 둘 다 허용, 로그인 실행)
        Title,          // 타이틀 화면
        StageSelect,    // 스테이지 선택 메뉴(Enter/Space 둘 다 허용, 선택 실행)
    }

    [SerializeField]
    private UIState state = UIState.Entry; // 시작 상태 초기화

    [Header("Entry")]
    [SerializeField] private GameObject Entry;

    [Header("Panel")] // Host, Join
    [SerializeField] private GameObject[] OnlineMenuButtons;

    [Header("LobbyEntry")] // HostMenu, JoinMenu
    [SerializeField] private GameObject Lobby;
    [SerializeField] private GameObject[] LobbyEntryPanels;
    [SerializeField] private HostMenuController hostMenuController;
    [SerializeField] private JoinMenuController joinMenuController;

    [Header("Title")] //Start Game, Return, Finish Game
    [SerializeField] private TitleMenuController titleMenuController;


    [Header("StagePanel")] // Stage1 ~ 6
    [SerializeField] private StageMenuController stageMenuController;
    [SerializeField] private GameObject StageSelectPanel;


    private int panelIndex = 0;   // 옵션 - 사운드 패널 수직 인덱스 번호 (0: Master, 1: BGM, 2: SE, 3: OK/Cancel 등)
    private const int STAGE_COLUMNS = 3;

    public static OnlineMenu_UIManager Instance = null;

    private bool isBound = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            playerInput = new IA_Player(); // 뉴인풋받기
        }
        else
        {
            Destroy(gameObject); // 중복된 매니저 삭제
            return;
        }


    }

    void Start()
    {
        SetEntryState(); // Press Enter 상태로 시작하도록 설정
    }

    public void RegisterLocalPlayer(PlayerInput player)
    {
        _localPlayerController = player;
        // 이제 플레이어의 IA_Player를 UI 매니저도 공유합니다.
        this.playerInput = player.GetPlayerInput();

        // UI 이벤트 연결 (기존 OnEnable에 있던 로직을 함수로 빼서 호출하면 좋습니다)
        BindUIEvents();
    }

    private void BindUIEvents()
    {
        if (isBound) return;
        isBound = true;

        playerInput.MenuUI.Left.performed += MoveLeft;
        playerInput.MenuUI.Right.performed += MoveRight;

        playerInput.MenuUI.Up.performed += MoveUp;
        playerInput.MenuUI.Down.performed += MoveDown;

        playerInput.MenuUI.Enter.performed += Select;
        playerInput.MenuUI.Space.performed += Select;

        playerInput.MenuUI.ESC.performed += ESC;

        playerInput.Enable();
    }

    private void OnEnable()
    {
        BindUIEvents();
    }

    private void OnDisable()
    {
        Debug.Log($"{gameObject.name}이(가) 비활성화되었습니다.");
        // 아래 코드는 에디터를 일시정지 시킵니다.
        // 로그가 찍히는 순간 왼쪽 하단의 Stack Trace를 보면 누가 껐는지 알 수 있습니다.
        Debug.Break();

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
        titleMenuController.HideAllPanels();
        if (StageSelectPanel != null)
        {
            StageSelectPanel.SetActive(false);
        }
        panelIndex = 0;
        OnlineMenuButtons[panelIndex].GetComponent<ButtonHover>().OnFocus();
    }

    public void ShowLobbyEntryPanelUI(int panelIdx)
    {
        panelIndex = 0; // 항상 처음 항목부터 시작
        //DisablePanel();
        LobbyEntryPanels[panelIdx].SetActive(true);
        if (panelIdx.Equals(0))
        {
            hostMenuController.UpdateHostPanelSelection(panelIndex);
        }
        else
        {
            joinMenuController.UpdateJoinPanelSelection(panelIndex);
        }
        state = UIState.LobbyEntry;
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
                    else if (panelIndex.Equals(3)) // Color
                    {
                        hostMenuController.OnColorLeft();
                    }
                    else if (panelIndex.Equals(5))
                    {
                        panelIndex = 4;
                        hostMenuController.UpdateHostPanelSelection(panelIndex);
                    }
                }
                else
                {
                    if (panelIndex.Equals(1)) // Hat
                    {
                        joinMenuController.OnHatLeft();
                    }
                    else if (panelIndex.Equals(2)) // Color
                    {
                        joinMenuController.OnColorLeft();
                    }
                    else if (panelIndex.Equals(4))
                    {
                        panelIndex = 3;
                        joinMenuController.UpdateJoinPanelSelection(panelIndex);
                    }
                }
                break;
            case UIState.Title:
                titleMenuController.MoveLeft();
                break;
            case UIState.StageSelect:
                if (panelIndex % STAGE_COLUMNS > 0)
                    panelIndex--;

                stageMenuController.UpdateSelection(panelIndex);
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
                    else if (panelIndex.Equals(3)) // Color
                    {
                        hostMenuController.OnColorRight();
                    }else if (panelIndex.Equals(4)){
                        panelIndex = 5;
                        hostMenuController.UpdateHostPanelSelection(panelIndex);
                    }
                }
                else
                {
                    if (panelIndex.Equals(1)) // Hat
                    {
                        joinMenuController.OnHatRight();
                    }
                    else if (panelIndex.Equals(2)) // Color
                    {
                        joinMenuController.OnColorRight();
                    }else if (panelIndex.Equals(3))
                    {
                        panelIndex = 4;
                        joinMenuController.UpdateJoinPanelSelection(panelIndex);
                    }
                }
                break;
            case UIState.Title:
                titleMenuController.MoveRight();
                break;
            case UIState.StageSelect:
                if (panelIndex % STAGE_COLUMNS < STAGE_COLUMNS - 1 &&
                  panelIndex + 1 < stageMenuController.GetTotalStages())
                    panelIndex++;

                stageMenuController.UpdateSelection(panelIndex);
                break;
        }
    }

    private void MoveUp(InputAction.CallbackContext context)
    {
        if (state != UIState.LobbyEntry && state != UIState.StageSelect) return;
        panelIndex--;
        switch (state)
        {
            case UIState.LobbyEntry:
                if (LobbyEntryPanels[0].activeSelf)
                {
                    if (panelIndex < 0)
                    {
                        panelIndex = 5;
                    }
                    hostMenuController.UpdateHostPanelSelection(panelIndex);

                }
                else
                {
                    if (panelIndex < 0)
                    {
                        panelIndex = 4;
                    }
                    joinMenuController.UpdateJoinPanelSelection(panelIndex);
                }
                break;
            case UIState.StageSelect:
                int next = panelIndex - STAGE_COLUMNS;
                if (next >= 0)
                    panelIndex = next;

                stageMenuController.UpdateSelection(panelIndex);
                break;
        }

    }

    private void MoveDown(InputAction.CallbackContext context)
    {
        if (state != UIState.LobbyEntry && state != UIState.StageSelect) return;

        panelIndex++;

        switch (state)
        {
            case UIState.LobbyEntry:
                if (LobbyEntryPanels[0].activeSelf)
                {
                    if (panelIndex >= 6)
                    {
                        panelIndex = 0;
                    }
                    hostMenuController.UpdateHostPanelSelection(panelIndex);

                }else
                {
                    if (panelIndex > 4)
                    {
                        panelIndex = 0;
                    }
                    joinMenuController.UpdateJoinPanelSelection(panelIndex);
                }
                break;
            case UIState.StageSelect:
                int next = panelIndex + STAGE_COLUMNS;
                if (next < stageMenuController.GetTotalStages())
                    panelIndex = next;

                stageMenuController.UpdateSelection(panelIndex);
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
                if (LobbyEntryPanels[0].activeSelf)
                {
                    if (panelIndex.Equals(4))
                    {
                        hostMenuController.InvokeCreate();

                    }
                    else if (panelIndex.Equals(5))
                    {
                        hostMenuController.InvokeCancel();
                        SetEntryState();
                    }
                }
                else
                {
                    if (panelIndex.Equals(0))
                    {
                        joinMenuController.FocusInputField();
                    }
                    else if (panelIndex.Equals(3))
                    {
                        joinMenuController.InvokeJoin();
                    }
                    else if (panelIndex.Equals(4))
                    {
                        joinMenuController.InvokeCancel();
                    }
                }
                break;
            case UIState.Title:
                // 1. 만약 "Press Enter" 글자가 켜져 있는 상태라면? -> 메뉴 패널을 연다.
                if (titleMenuController.pressbutton.activeSelf)
                {
                    titleMenuController.SetPressButtonActive(false);
                    titleMenuController.InitTitleMenu();
                    EnableUIMode();
                    return;
                }
                // 2. 이미 메뉴 패널이 열려 있는 상태라면? -> 현재 인덱스의 기능을 실행한다.
                else
                {
                    titleMenuController.ExecuteSelection();
                }
                break;
            case UIState.StageSelect:
                stageMenuController.ExecuteSelection();
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
                if (hostMenuController.gameObject.activeSelf)
                {
                    hostMenuController.SetActive(false);
                    hostMenuController.InvokeCancel();

                }
                else if (joinMenuController.gameObject.activeSelf)
                {
                    joinMenuController.SetActive(false);
                    joinMenuController.InvokeCancel();
                }
                SetEntryState();
                break;
            case UIState.Title:
                if (titleMenuController.IsUIActive())
                {
                    // 패널이 켜져 있다면: 패널을 끄고, PressButton을 다시 활성화
                    titleMenuController.SetActive(false);
                    titleMenuController.SetPressButtonActive(true);

                    //캐릭터 움직이기
                    EnablePlayerMode();
                }
                else
                {
                    // 2. 패널이 꺼져 있다면(즉, PressButton이 켜져 있는 상태라면)
                    titleMenuController.SetPressButtonActive(false);
                    titleMenuController.SetActive(true);

                    //UI 조작
                    EnableUIMode();
                }
                break;
            case UIState.StageSelect:
                StageSelectPanel.SetActive(false);
                changeState(2); // Title 상태로 복귀
                break;
        }

    }

    public void ShowStageSelect()
    {
        state = UIState.StageSelect;
        StageSelectPanel.SetActive(true);
        panelIndex = 0;
        stageMenuController.UpdateSelection(panelIndex);
        EnableUIMode(); // 캐릭터 조작 대신 UI 조작 활성화
    }

    public void changeState(int state)
    {
        if (state.Equals(0))
        {
            SetEntryState();
            if (!Entry.activeSelf)
            {
                Entry.SetActive(true);
            }
        }else if (state.Equals(2))
        {
            hostMenuController.SetActive(false);
            joinMenuController.SetActive(false);
            this.state = UIState.Title;
            Lobby.SetActive(true);
            titleMenuController.SetPressButtonActive(true);
        }else if (state.Equals(3))
        {
            this.state = UIState.StageSelect;
            StageSelectPanel.SetActive(true);
        }
    }

    private void EnableUIMode()
    {
        playerInput.Player.Disable(); // 캐릭터 이동 맵 끄기
        playerInput.MenuUI.Enable();  // UI 조작 맵 켜기
    }

    private void EnablePlayerMode()
    {
        playerInput.MenuUI.Disable(); // UI 조작 맵 끄기
        playerInput.Player.Enable();  // 캐릭터 이동 맵 켜기
    }
    public void RestorePlayerMode()
    {
        this.state = UIState.Title; // 상태는 유지하되
        EnablePlayerMode();        // 조작만 캐릭터로 변경
    }
}
