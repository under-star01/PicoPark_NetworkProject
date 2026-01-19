using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button[] buttons;

    private IA_Player playerInput;

    private PlayerMove localPlayerMove;

    private int panelIndex = 0;

    private void Start()
    {
        // 만약 전 씬의 UI 매니저가 살아있다면 이벤트를 일시적으로 꺼버림
        if (OnlineMenu_UIManager.Instance != null)
        {
            OnlineMenu_UIManager.Instance.enabled = false;
            // 혹은 해당 스크립트 내부에서 Input을 Disable 시키는 함수 호출
        }
    }

    private void Awake()
    {
        playerInput = new IA_Player();
    }

    private void OnEnable()
    {
        if (playerInput == null) return;

        playerInput.Enable();

        playerInput.MenuUI.Up.performed += MoveUp;
        playerInput.MenuUI.Down.performed += MoveDown;

        playerInput.MenuUI.Enter.performed += Select;
        playerInput.MenuUI.Space.performed += Select;

        playerInput.MenuUI.ESC.performed += ESC;
    }

    private void OnDisable()
    {
        if (playerInput == null) return;

        playerInput.MenuUI.Up.performed -= MoveUp;
        playerInput.MenuUI.Down.performed -= MoveDown;

        playerInput.MenuUI.Enter.performed -= Select;
        playerInput.MenuUI.Space.performed -= Select;

        playerInput.MenuUI.ESC.performed -= ESC;
    }


    private void Select(InputAction.CallbackContext context)
    {

        if (!menuPanel.activeSelf) return;

        buttons[panelIndex].onClick.Invoke();
        
    }

    private void ESC(InputAction.CallbackContext context)
    {

        if (menuPanel == null) return;

        if (!menuPanel.activeSelf)
        {
            menuPanel.SetActive(true);
            EnableUIMode();
        }
        else
        {
            menuPanel.SetActive(false);
            EnablePlayerMode();
        }
    }

    private void MoveUp(InputAction.CallbackContext context)
    {
        if (!menuPanel.activeSelf) return;

        panelIndex--;

        if (panelIndex < 0)
        {
            panelIndex = 2;
        }
        buttons[panelIndex].GetComponent<ButtonHover>().OnFocus();
    }

    private void MoveDown(InputAction.CallbackContext context)
    {
        if (!menuPanel.activeSelf) return;

        panelIndex++;

        if (panelIndex > 2)
        {
            panelIndex = 0;
        }
        buttons[panelIndex].GetComponent<ButtonHover>().OnFocus();
    }


    private void EnableUIMode()
    {
        if (localPlayerMove == null) return;
        localPlayerMove.CmdLockInput(true);
    }

    private void EnablePlayerMode()
    {
        if (localPlayerMove == null) return;
        localPlayerMove.CmdLockInput(false);
    }

    public void OnTitleButton()
    {
        SceneManager.LoadScene("1.Title");
    }

    public void OnRetryButton()
    {
        // 로컬 플레이어 찾기
        PlayerStageController player = FindLocalPlayer();
        if (player != null)
            player.CmdRequestRetry();
    }

    public void OnLobbyButton()
    {
        PlayerStageController player = FindLocalPlayer();
        if (player != null)
            player.CmdReturnToLobby();
    }

    // 로컬 플레이어 찾는 안전한 함수
    private PlayerStageController FindLocalPlayer()
    {
        // NetworkClient.localPlayer는 클라이언트에서만 접근 가능
        if (NetworkClient.localPlayer == null) return null;

        return NetworkClient.localPlayer.GetComponent<PlayerStageController>();
    }
}
