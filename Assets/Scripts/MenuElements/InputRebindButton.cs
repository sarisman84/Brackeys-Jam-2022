using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public enum ComponsiteDirection { Left, Right, Up, Down }

public class InputRebindButton : Button {

    const string onRebindingText = "Waiting For Input";

    [SerializeField]
    public InputActionReference inputToRebind;
    public bool rebindMouse;
    public bool rebindComposite;
    public ComponsiteDirection componsiteDirection;

    private TextMeshProUGUI textMeshProUGUI;
    private string defaultText;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    private Image selectionIndicator;

    protected override void Awake()
    {
        base.Awake();
        textMeshProUGUI = GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshProUGUI && inputToRebind)
        {
            textMeshProUGUI.text = GetInputName();
        }





        selectionIndicator = GetComponentInChildren<Image>();
        if (selectionIndicator)
            selectionIndicator.enabled = false;
    }


    public override void OnPointerClick(PointerEventData eventData)
    {
        StartRebind();
        base.OnPointerClick(eventData);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (selectionIndicator)
            selectionIndicator.enabled = true;
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (selectionIndicator)
            selectionIndicator.enabled = false;
        base.OnPointerExit(eventData);
    }

    int GetBindingIndex()
    {
        string name = "";

        switch (componsiteDirection)
        {
            case ComponsiteDirection.Left:
                name = "Left";
                break;
            case ComponsiteDirection.Right:
                name = "Right";
                break;
            case ComponsiteDirection.Up:
                name = "Up";
                break;
            case ComponsiteDirection.Down:
                name = "Down";
                break;
            default:
                break;
        }
        int index = inputToRebind.action.bindings.IndexOf(x =>
        {
            return x.isPartOfComposite && x.name.ToLower().Contains(name.ToLower());
        });
        return index;
    }


    public void StartRebind()
    {
        if (inputToRebind == null) return;

        textMeshProUGUI.text = onRebindingText;

        inputToRebind.action.Disable();

        rebindingOperation = inputToRebind.action.PerformInteractiveRebinding();


        if (rebindComposite)
        {
            var bindingIndex = GetBindingIndex();
            rebindingOperation.WithTargetBinding(bindingIndex);
        }
        else if (!rebindMouse)
            rebindingOperation.WithControlsExcluding("Mouse");

        rebindingOperation.OnMatchWaitForAnother(0.1f);
        rebindingOperation.OnComplete(operation => RebindComplete());
        rebindingOperation.Start();


        inputToRebind.action.Enable();
    }

    private void RebindComplete()
    {
        textMeshProUGUI.text = GetInputName();
        rebindingOperation.Dispose();


    }


    public string GetInputName()
    {
        if (rebindComposite)
        {
            int index = GetBindingIndex();
            if (index == -1)
            {
                Debug.LogError("Couldnt find keybind");
                return "Unknown";
            }
            return InputControlPath.ToHumanReadableString(inputToRebind.action.bindings[index].effectivePath);
        }
        else
        {
            return InputControlPath.ToHumanReadableString(inputToRebind.action.bindings[0].effectivePath);
        }
    }
}
