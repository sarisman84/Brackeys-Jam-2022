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
            if (rebindComposite)
            {
                textMeshProUGUI.text = inputToRebind.action.bindings[0].name;
            }
            else
            {
                textMeshProUGUI.text = inputToRebind.action.bindings[0].name;
            }
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

    }


    public void StartRebind()
    {
        if (inputToRebind == null) return;

        textMeshProUGUI.text = onRebindingText;

        inputToRebind.action.Disable();

        rebindingOperation = inputToRebind.action.PerformInteractiveRebinding();


        if (rebindComposite)
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

            var bindingIndex = inputToRebind.action.bindings.IndexOf(x => x.isPartOfComposite && x.name == name);
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
        textMeshProUGUI.text = inputToRebind.action.bindings[0].name;
        rebindingOperation.Dispose();


    }
}
