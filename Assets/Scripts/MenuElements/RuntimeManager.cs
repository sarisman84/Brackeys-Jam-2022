using System.Collections;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Cinemachine;

public class RuntimeManager : MonoBehaviour
{
    public enum RuntimeState
    {
        MainMenu, Paused, Playing
    }


    public RuntimeState currentState { private set; get; }
    public bool cameraFrozen { private set; get; }
    public event Action<RuntimeState> onStateChangeCallback;


    public InputActionReference pauseInput;
    public Canvas pauseUIElement;


    private CinemachinePOV povHandler;
    private OptionsManager optionsM;
    private MenuManager menuManager;
    private Vector2 inputVals;

    private void Start()
    {
        povHandler = PollingStation.Instance.cameraController.GetCinemachineComponent<CinemachinePOV>();
        menuManager = PollingStation.Instance.menuManager;
        optionsM = PollingStation.Instance.optionsManager;

        povHandler.m_HorizontalAxis.m_InputAxisValue = optionsM.currentSensitivity;
        povHandler.m_VerticalAxis.m_InputAxisValue = optionsM.currentSensitivity;
    }

    private void OnEnable()
    {
        pauseInput.action.Enable();
    }


    private void OnDisable()
    {
        pauseInput.action.Disable();
    }



    private void Update()
    {
        if (currentState != RuntimeState.Playing)
        {
            if (!cameraFrozen)
                FreezeCamera();

        }
        else
        {
            if (cameraFrozen)
                UnfreezeCamera();
        }


        if (pauseInput.action.ReadValue<float>() > 0 && pauseInput.action.triggered)
        {
            if (menuManager.IsCurrentCanvasOpen())
            {
                menuManager.ExitCurrentCanvas((Canvas currentCanvas) =>
                {
                    if (currentCanvas == pauseUIElement)
                    {
                        SetState(RuntimeState.Playing);
                    }
                });

            }
            else if (currentState == RuntimeState.Playing)
            {
                SetState(RuntimeState.Paused);
                menuManager.OpenCanvas(pauseUIElement);
            }
        }
    }

    private void FreezeCamera()
    {
        cameraFrozen = true;

        povHandler.m_HorizontalAxis.m_MaxSpeed = 0;
        povHandler.m_VerticalAxis.m_MaxSpeed = 0;


    }

    private void UnfreezeCamera()
    {
        cameraFrozen = false;

        povHandler.m_HorizontalAxis.m_MaxSpeed = optionsM.currentSensitivity;
        povHandler.m_VerticalAxis.m_MaxSpeed = optionsM.currentSensitivity;
    }






    void UpdateState()
    {
        switch (currentState)
        {
            case RuntimeState.MainMenu:
            case RuntimeState.Paused:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            case RuntimeState.Playing:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            default:
                break;
        }


        onStateChangeCallback?.Invoke(currentState);
    }



    public void SetState(RuntimeState aState)
    {
        currentState = aState;
        UpdateState();
    }

    public void SetStateToPlay()
    {
        SetState(RuntimeState.Playing);
    }

    public void SetStateToMenu()
    {
        SetState(RuntimeState.MainMenu);
    }




}
