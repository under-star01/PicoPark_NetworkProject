using UnityEngine;
using UnityEngine.UI;

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
        stageButtons[currentIndex].GetComponent<Button>().onClick.Invoke();
    }

    public int GetTotalStages() => stageButtons.Length;
}
