using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIMANAGER : MonoBehaviour
{
    private IA_Player playerInput;

    private enum UIState
    {
        PressEnter,     // 첫 화면(Enter만 허용)
        Login,          // 로그인 화면(Enter/Space 둘 다 허용, 로그인 실행)
        TitleMenu,       // 타이틀 메뉴(Enter/Space 둘 다 허용, 선택 실행)
        Panel,           // 패널 선택(Enter/Space 둘 다 허용, 선택 실행)
    }

    [SerializeField]
    private UIState state = UIState.PressEnter; // 시작 상태 초기화

    [Header("UI References")]
    [SerializeField] private GameObject pressEnterUI;
    [SerializeField] private GameObject loginUI;
    [SerializeField] private GameObject titleMenuUI;

    [Header("Login")]
    [SerializeField] private Button loginButton;  // 로그인 실행 버튼

    [Header("TitleMenu")]
    [SerializeField] private TitleMenu titleMenu;
    [SerializeField] private Button[] submitButtons; // 타이틀UI에서 실행 버튼


    [Header("Panel")]
    [SerializeField] private GameObject[] Panels;


    [Header("Auth")]
    [SerializeField] private AuthService authService;

    private int currentIndex = 0; // 인덱스 번호(UI순서) 초기화
    private int panelIndex = 0;   // 옵션 - 사운드 패널 수직 인덱스 번호 (0: Master, 1: BGM, 2: SE, 3: OK/Cancel 등)
    private void Awake()
    {
        playerInput = new IA_Player(); // 뉴인풋받기

    }

    void Start()
    {
        SetPressEnterState(); // Press Enter 상태로 시작하도록 설정
        AudioManager.Instance.PlayBGM("Title");
    }

    private void OnEnable()
    {
        authService.OnLoginResult += HandleLoginResult;

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
        authService.OnLoginResult -= HandleLoginResult;

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

    private void SetPressEnterState()
    {
        state = UIState.PressEnter; // Press Enter 상태일 때

        if (pressEnterUI != null)
        {
            pressEnterUI.SetActive(true);
        }
        if (loginUI != null)
        {
            loginUI.SetActive(false);
        }
        if (titleMenuUI != null)
        {
            titleMenuUI.SetActive(false);
        }
    }

    private void ShowLoginUI()
    {
        state = UIState.Login;

        if (pressEnterUI != null)
        {
            pressEnterUI.SetActive(false);
        }
        if (loginUI != null)
        {
            loginUI.SetActive(true);
        }
        if (titleMenuUI != null)
        {
            titleMenuUI.SetActive(false);
        }
    }

    public void OnLoginSuccess()
    {
        state = UIState.TitleMenu;

        if (loginUI != null)
        {
            loginUI.SetActive(false);
        }
        if (titleMenuUI != null)
        {
            titleMenuUI.SetActive(true);
        }

    }

    private void ShowPanelUI(int panelIdx)
    {
        state = UIState.Panel;
        panelIndex = 0; // 항상 처음 항목부터 시작
        DisablePanel();
        Panels[panelIdx].SetActive(true);
        titleMenu.UpdatePanelSelection(panelIndex); // 시각적 초기화
    }

    private void DisablePanel()
    {
        if (Panels != null)
        {
            foreach (GameObject Panel in Panels)
            {
                Panel.SetActive(false);
            }
        }
    }

    private void HandleLoginResult(bool success)
    {
        if (success)
        {
            OnLoginSuccess();
        }
        else
        {
            authService.LogText_viewing("정확한 ID이나 PASSWORD를\n 다시 입력하세요");
            // 실패 시 UI는 그대로 두거나, 효과음/진동 등
        }
    }


    // ========== 입력 처리 ==========

    private void MoveLeft(InputAction.CallbackContext context)
    {
        if (!(state == UIState.TitleMenu || state == UIState.Panel)) return;

        switch (state)
        {
            case UIState.TitleMenu:
                titleMenu.MoveLeft();
                currentIndex = titleMenu.currentIndex;
                break;
            case UIState.Panel:
                if (Panels[0].activeSelf) // 옵션 패널이 켜져 있으면
                {
                    if (panelIndex.Equals(0)) // 마스터
                    {
                        titleMenu.MasterVolumeLeft();
                    }
                    else if (panelIndex.Equals(1)) // BGM
                    {
                        titleMenu.BGMVolumeLeft();
                    }
                    else if (panelIndex.Equals(2)) // SE
                    {
                        titleMenu.SFXVolumeLeft();
                    }
                    else // cancel
                    {
                        panelIndex = 3;
                        titleMenu.UpdatePanelSelection(panelIndex);
                    }
                }
                else
                {
                    if (panelIndex.Equals(1)) // No
                    {
                        panelIndex = 0;
                        titleMenu.UpdatePanelSelection(panelIndex);
                    }
                }
                break;
        }


    }


    private void MoveRight(InputAction.CallbackContext context)
    {
        if (!(state == UIState.TitleMenu || state == UIState.Panel)) return;

        switch (state)
        {
            case UIState.TitleMenu:
                titleMenu.MoveRight();
                currentIndex = titleMenu.currentIndex;
                break;
            case UIState.Panel:
                if (Panels[0].activeSelf) // 옵션 패널이 켜져 있으면
                {
                    if (panelIndex.Equals(0)) // 마스터
                    {
                        titleMenu.MasterVolumeRight();
                    }
                    else if (panelIndex.Equals(1)) // BGM
                    {
                        titleMenu.BGMVolumeRight();
                    }
                    else if (panelIndex.Equals(2)) // SE
                    {
                        titleMenu.SFXVolumeRight();
                    }
                    else// OK
                    {
                        panelIndex = 4;
                        titleMenu.UpdatePanelSelection(panelIndex);
                    }
                }
                else
                {
                    if (panelIndex.Equals(0)) // YES
                    {
                        panelIndex = 1;
                        titleMenu.UpdatePanelSelection(panelIndex);
                    }
                }
                break;
        }
    }

    private void MoveUp(InputAction.CallbackContext context)
    {
        if (state != UIState.Panel) return;
        panelIndex--;

        int maxIndex = Panels[0].activeSelf ? 4 : 1;

        if (panelIndex < 0)
        {
            panelIndex = maxIndex;
        }
        titleMenu.UpdatePanelSelection(panelIndex);
    }

    private void MoveDown(InputAction.CallbackContext context)
    {
        if (state != UIState.Panel) return;
        panelIndex++;

        int maxIndex = Panels[0].activeSelf ? 4 : 1;

        if (panelIndex > maxIndex)
        {
            panelIndex = 0;
        }
        titleMenu.UpdatePanelSelection(panelIndex);
    }

    private void Select(InputAction.CallbackContext context)
    {
        switch (state)
        {
            case UIState.PressEnter:
                // ★ PressEnter 화면에선 **Enter만** 허용
                if (context.action == playerInput.MenuUI.Enter)
                {
                    ShowLoginUI();
                }
                // Space는 무시(아무 일도 안 일어남)
                break;

            case UIState.Login:
                // 로그인 화면: Enter/Space 둘 다 로그인 실행
                if (loginButton != null && loginButton.IsActive())
                {
                    loginButton.onClick.Invoke();
                }
                break;

            case UIState.TitleMenu:
                // TitleMenu: Enter/Space 둘 다 현재 선택 실행

                SubmitCurrent();
                break;
            case UIState.Panel:
                if (Panels[0].activeSelf) //Option
                {

                    if (panelIndex == 3) // OK 버튼
                    {
                        titleMenu.ConfirmOption(); // 여기서 저장 로직 호출
                        state = UIState.TitleMenu;
                    }
                    else if (panelIndex == 4) // Cancel 버튼
                    {
                        titleMenu.CancelOption(); // 여기서 복구 로직 호출
                        state = UIState.TitleMenu;
                    }
                }
                else if (Panels[1].activeSelf)// Exit
                {
                    // 종료 패널은 보통 버튼이 2개이므로 panelIndex 0: YES, 1: NO 라고 가정합니다.
                    if (panelIndex == 0) // YES (종료 실행)
                    {
                        titleMenu.ConfirmExitYes();
                    }
                    else if (panelIndex == 1) // NO (취소 후 돌아가기)
                    {
                        titleMenu.CloseExitNo();
                        state = UIState.TitleMenu; // 상태 복구
                    }
                }
                break;
        }
    }

    private void SubmitCurrent()
    {
        if (submitButtons == null || submitButtons.Length == 0) return;

        if (currentIndex < 0 || currentIndex >= submitButtons.Length)
        {
            currentIndex = 0;
        }
        if (submitButtons[currentIndex] != null)
        {
            if (currentIndex == 1)
            {
                titleMenu.OpenOption(); // 여기서 backupMaster = value 로직이 실행됨
                ShowPanelUI(0); // 인덱스 전달하도록 수정 추천
            }
            else if (currentIndex == 2) // 종료
            {
                titleMenu.OpenExit();
                ShowPanelUI(1);
            }
            else
            {
                submitButtons[currentIndex].onClick.Invoke();
            }
        }
    }

    private void ESC(InputAction.CallbackContext context)
    {
        //TitleMenu에서 뒤로가기 등 기능 추가
        switch (state)
        {
            case UIState.PressEnter:
                // ★ PressEnter 화면에선 **Enter만** 허용
                if (context.action == playerInput.MenuUI.ESC)
                {
                    titleMenu.OpenExit();
                }
                break;

            case UIState.Login:
                SetPressEnterState();   // 되돌아가기
                break;

            case UIState.TitleMenu:
                ShowLoginUI();   // 되돌아가기
                break;
            case UIState.Panel:
                DisablePanel();
                state = UIState.TitleMenu; // 되돌아가기
                break;
        }

    }
}
