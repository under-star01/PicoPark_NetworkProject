using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class UIInputController : MonoBehaviour
{
    public InputActionReference nextFieldAction;

    public void MoveNext()
    {
        GameObject currentObject = EventSystem.current.currentSelectedGameObject;
        if (currentObject == null) return;

        Selectable selectable = currentObject.GetComponent<Selectable>();
        if (selectable == null) return;

        Selectable next = selectable.FindSelectableOnDown();
        if (next != null)
        {
            next.Select();
            TMP_InputField tmpInput = next.GetComponent<TMP_InputField>();
            if (tmpInput != null) tmpInput.ActivateInputField();

        }
    }


    void OnEnable()
    {
        if (nextFieldAction != null && nextFieldAction.action != null)
        {
            // ⭐ 핵심: 액션을 반드시 활성화해야 performed 이벤트가 발생합니다.
            nextFieldAction.action.Enable();
            nextFieldAction.action.performed += OnTab;
        }
        else
        {
            Debug.LogError("Next Field Action 에셋이 할당되지 않았습니다!");
        }
    }

    void OnDisable()
    {
        if (nextFieldAction != null && nextFieldAction.action != null)
        {
            nextFieldAction.action.performed -= OnTab;
            nextFieldAction.action.Disable(); // 끌 때는 비활성화
        }
    }

    void OnTab(InputAction.CallbackContext ctx)
    {
        MoveNext();
    }

}
