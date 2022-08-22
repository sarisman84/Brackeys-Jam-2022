using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;

using TMPro;

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
    public float currentSensitivity { private set; get; }


    [Header("Return Button Options")]
    public GameObject mainMenuGroup;
    public GameObject pauseMenuGroup;
    public enum RuntimeState
    {
        MainMenu, Paused, Playing
    }



    public RuntimeState currentState { private set; get; }
    public event Action<RuntimeState> onStateChangeCallback;

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
        switch (currentState)
        {
            case RuntimeState.MainMenu:
                mainMenuGroup.SetActive(true);
                break;
            case RuntimeState.Paused:
                pauseMenuGroup.SetActive(true);
                break;
        }
    }


}
