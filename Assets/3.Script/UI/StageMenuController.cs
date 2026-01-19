using UnityEngine;

public class StageMenuController : MonoBehaviour
{
    [SerializeField] private GameObject[] stageButtons;

    private int currentIndex = 0;

    public void UpdateSelection(int index)
    {
        index = Mathf.Clamp(index, 0, stageButtons.Length - 1);
        currentIndex = index;

        stageButtons[currentIndex].GetComponent<ButtonHover>().OnFocus();

        Debug.Log($"현재 선택된 스테이지: {currentIndex + 1}");
    }

    public void ExecuteSelection()
    {
        Debug.Log($"{currentIndex + 1} 스테이지를 시작합니다!");
        // 여기에 씬 전환 로직 추가 (예: SceneManager.LoadScene)
    }

    public int GetTotalStages() => stageButtons.Length;
}
