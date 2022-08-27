using System.Collections;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Cinemachine;

public class RuntimeManager : MonoBehaviour {
    public enum RuntimeState {
        MainMenu, Paused, Playing, GameOver
    }


    public RuntimeState currentState { private set; get; }
    public bool cameraFrozen { private set; get; }
    public event Action<RuntimeState, RuntimeState> onPostStateChangeCallback;


    public InputActionReference pauseInput;
    public Canvas pauseUIElement;

    private RuntimeState previousState;
    private CinemachinePOV povHandler;
    private OptionsManager optionsM;
    private MenuManager menuManager;
    private Vector2 inputVals;

    private AudioManager audioManager;

    private void Start()
    {
        povHandler = PollingStation.Instance.cameraController.GetCinemachineComponent<CinemachinePOV>();
        audioManager = PollingStation.Instance.audioManager;
        menuManager = PollingStation.Instance.menuManager;
        optionsM = PollingStation.Instance.optionsManager;

        povHandler.m_HorizontalAxis.m_InputAxisValue = optionsM.currentSensitivity;
        povHandler.m_VerticalAxis.m_InputAxisValue = optionsM.currentSensitivity;

        //end the game if the player dies
        PollingStation.Instance.player.GetComponent<HealthHandler>().onEntityDeath.AddListener(SetStateToGameOver);

        UpdateState();

    }

    private void OnEnable()
    {
        pauseInput.action.Enable();
    }


    private void OnDisable()
    {
        pauseInput.action.Disable();
    }


    bool IsValidCanvas(Canvas currentCanvas)
    {
        return currentCanvas != menuManager.startingCanvas;
    }

    void OnExitingCanvas(Canvas currentCanvas)
    {
        if (currentCanvas == pauseUIElement)
        {
            SetState(RuntimeState.Playing);
        }
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
                menuManager.ExitCurrentCanvas(IsValidCanvas, OnExitingCanvas);

            }
            else if (currentState == RuntimeState.Playing)
            {
                SetState(RuntimeState.Paused);
                menuManager.HardOpenCanvas(pauseUIElement);
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
            case RuntimeState.GameOver:
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


        onPostStateChangeCallback?.Invoke(previousState, currentState);
    }



    public void SetState(RuntimeState aState)
    {
        previousState = currentState;
        currentState = aState;
        UpdateState();
    }

    public void SetStateToPlay()
    {
        SetState(RuntimeState.Playing);
    }

    public void SetStateToMenu()
    {
        if (menuManager.startingCanvas)
            menuManager.HardOpenCanvas(menuManager.startingCanvas);

        SetState(RuntimeState.MainMenu);
    }

    public void SetStateToGameOver()
    {
        if (menuManager.IsCurrentCanvasOpen())
            menuManager.ExitCurrentCanvas(IsValidCanvas, OnExitingCanvas);
        menuManager.OpenCanvas(menuManager.gameOverCanvas);

        SetState(RuntimeState.GameOver);
    }


}
