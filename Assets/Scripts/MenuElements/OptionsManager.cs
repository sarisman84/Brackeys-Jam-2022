using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;

using TMPro;
using Cinemachine;
public class OptionsManager : MonoBehaviour
{

    //Resolution Settings
    [System.Serializable]
    public struct Resolution
    {
        public int width, height, refreshRate;

        public void ApplyResolution(bool isFullscreen)
        {
            Screen.SetResolution(width, height, isFullscreen, refreshRate);
        }
    }



    [Header("Resolution Settings")]
    public List<Resolution> myResolutions;
    public TMP_Dropdown myResolutionDropdown;
    public bool isFullscreen;

    private int currentResolution;

    [Header("Audio Settings")]
    //Audio Settings
    public AudioMixer mixer;

    [Header("Input Settings")]
    //Mouse Sensitivity
    public float minSensitivity;
    public float maxSensitivity;
    public float currentSensitivity { private set; get; } = 0.25f;


    [Header("Return Button Options")]
    public GameObject mainMenuGroup;
    public GameObject pauseMenuGroup;





    private void Awake()
    {
        if (!myResolutionDropdown) return;


        myResolutionDropdown.AddOptions(GenerateOptions());
        myResolutionDropdown.onValueChanged.AddListener((int index) =>
        {
            myResolutions[index].ApplyResolution(isFullscreen);
            currentResolution = index;
        });
    }


    private void Start()
    {
        var pollingStation = PollingStation.Instance;
        var camera = pollingStation.cameraController;
        var povHolder = camera.GetCinemachineComponent<CinemachinePOV>();

        pollingStation.runtimeManager.onPostStateChangeCallback += (RuntimeManager.RuntimeState previousState, RuntimeManager.RuntimeState state) =>
        {
            if (state == RuntimeManager.RuntimeState.Playing)
            {
                povHolder.m_HorizontalAxis.m_MaxSpeed = currentSensitivity;
                povHolder.m_VerticalAxis.m_MaxSpeed = currentSensitivity;
            }
        };
    }


    public void SetFullscreen(bool aValue)
    {
        isFullscreen = aValue;
        myResolutions[currentResolution].ApplyResolution(isFullscreen);
    }


    public void SetMasterVolume(float aValue)
    {
        mixer.SetFloat("MasterVol", CalculateVolumeLog(aValue));
    }



    public void SetMusicVolume(float aValue)
    {
        mixer.SetFloat("MusicVol", CalculateVolumeLog(aValue));
    }



    public void SetSFXVolume(float aValue)
    {
        mixer.SetFloat("SFXVol", CalculateVolumeLog(aValue));
    }


    public void SetMouseSensitivity(float aValue)
    {
        currentSensitivity = Mathf.Clamp(aValue, minSensitivity, maxSensitivity);
    }



    private List<TMP_Dropdown.OptionData> GenerateOptions()
    {
        List<TMP_Dropdown.OptionData> data = new List<TMP_Dropdown.OptionData>();


        foreach (var resolution in myResolutions)
        {
            var d = new TMP_Dropdown.OptionData();

            d.text = $"{resolution.width} x {resolution.height} [{resolution.refreshRate} hz]";
            data.Add(d);
        }


        return data;
    }

    //Code taken by this: https://www.youtube.com/watch?v=xNHSGMKtlv4
    private float CalculateVolumeLog(float aValue)
    {
        return Mathf.Log10(aValue) * 20;
    }



    public void ExitOptionsMenu()
    {
        var currentState = PollingStation.Instance.runtimeManager.currentState;

        switch (currentState)
        {
            case RuntimeManager.RuntimeState.MainMenu:
                mainMenuGroup.SetActive(true);
                break;
            case RuntimeManager.RuntimeState.Paused:
                pauseMenuGroup.SetActive(true);
                break;
        }
    }

}
