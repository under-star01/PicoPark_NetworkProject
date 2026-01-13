using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIMANAGER : MonoBehaviour
{
    private IA_Player playerInput;
    private TitleMenu titleMenu;

    private enum UIState
    {
        PressEnter,     // 첫 화면(Enter만 허용)
        Login,          // 로그인 화면(Enter/Space 둘 다 허용, 로그인 실행)
        TitleMenu       // 타이틀 메뉴(Enter/Space 둘 다 허용, 선택 실행)
    }

    private UIState state = UIState.PressEnter; // 시작 상태 초기화

    [Header("UI References")]
    [SerializeField] private GameObject pressEnterUI;
    [SerializeField] private GameObject loginUI;
    [SerializeField] private GameObject titleMenuUI;

    [Header("Login")]
    [SerializeField] private Button loginButton;  // 로그인 실행 버튼

    [Header("TitleMenu")]
    [SerializeField] private Button[] submitButtons; // 타이틀UI에서 실행 버튼

    private int currentIndex = 0; // 인덱스 번호(UI순서) 초기화

    private void Awake()
    {
        playerInput = new IA_Player(); // 뉴인풋받기
        TryGetComponent(out titleMenu); // 타이틀 메뉴 스크립트 참조
    }

    void Start()
    {
        SetPressEnterState(); // Press Enter 상태로 시작하도록 설정
    }

    private void OnEnable()
    {
        playerInput.MenuUI.Left.performed += MoveLeft;
        playerInput.MenuUI.Right.performed += MoveRight_input;

        playerInput.MenuUI.Enter.performed += Select;
        playerInput.MenuUI.Space.performed += Select;

        playerInput.MenuUI.ESC.performed += ESC;

        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.MenuUI.Left.performed -= MoveLeft;
        playerInput.MenuUI.Right.performed -= MoveRight_input;

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
        currentIndex = 0;
        // TitleMenu는 Start()에서 첫 항목 켜짐
    }

    // ========== 입력 처리 ==========

    private void MoveLeft(InputAction.CallbackContext context)
    {
        if (state != UIState.TitleMenu) return;

        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = (submitButtons != null ? submitButtons.Length : 3) - 1;
        }

        if (titleMenu != null)
        {
            titleMenu.MoveLeft();
        }
    }


    private void MoveRight_input(InputAction.CallbackContext context)
    {
        if (state != UIState.TitleMenu) return;

        currentIndex++;
        if (currentIndex >= (submitButtons != null ? submitButtons.Length : 3))
        {
            currentIndex = 0;
        }

        if (titleMenu != null)
        {
            titleMenu.MoveRight();
        }
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
                if (loginButton != null)
                {
                    loginButton.onClick.Invoke();
                }
                break;

            case UIState.TitleMenu:
                // TitleMenu: Enter/Space 둘 다 현재 선택 실행
                SubmitCurrent();
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
            submitButtons[currentIndex].onClick.Invoke();
        }
    }

    private void ESC(InputAction.CallbackContext context)
    {
        //TitleMenu에서 뒤로가기 등 기능 추가
    }
}
