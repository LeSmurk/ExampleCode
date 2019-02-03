using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class script_Options : MonoBehaviour
{
    [Header("Display")]
    public Dropdown resDropdown;
    private List<string> allResOptions = new List<string>();
    private List<Resolution> allResolutions = new List<Resolution>();
    private int currentRes = 0;
    //fov
    public Dropdown fovDropdown;
    private float currentFov = 60;
    //fps
    public Dropdown fpsDropdown;
    private int currentFps = 60;
    //fullscreen
    private bool isFullscreen;
    public Toggle fullscreenToggle;

    [Header("Audio")]
    public Slider musicSlider;
    private float musicVolume;
    public Slider soundSlider;
    private float soundsVolume;
    private bool initialised = false;
    public script_AudioManager audioManager_scr;

    public void Init()
    {
        //honestly don't know why I put if statements since I could just set the default value to max
        //read from player prefs volumes
        float temp = -1;
        temp = PlayerPrefs.GetFloat("musicvol", -1);
        if (temp >= 0)
        {
            musicSlider.value = temp;
        }
        temp = PlayerPrefs.GetFloat("sfxvol", -1);
        if (temp >= 0)
        {
            soundSlider.value = temp;
            initialised = true;
        }


        //create all resolutions possible
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            bool addRes = true;

            //check for duplicate resolutions (fps means there are more)
            if(allResolutions.Count > 0)
            {
                if (Screen.resolutions[i].height == allResolutions[allResolutions.Count - 1].height && Screen.resolutions[i].width == allResolutions[allResolutions.Count - 1].width)
                    addRes = false;
            }

            if(addRes)
            {
                //add resolution to options
                allResolutions.Add(Screen.resolutions[i]);
                allResOptions.Add(allResolutions[allResolutions.Count - 1].width + "x" + allResolutions[allResolutions.Count - 1].height);

            }
        }

        //set dropdown options
        resDropdown.ClearOptions();
        resDropdown.AddOptions(allResOptions);

        //check the res we are at now (this will be overwritten when using local data)
        for(int i = 0; i < allResolutions.Count; i++)
        {
            if (Screen.width == allResolutions[i].width && Screen.height == allResolutions[i].height)
                resDropdown.value = i;
        }

        //set fullscreen to start
        isFullscreen = Screen.fullScreen;
        fullscreenToggle.isOn = isFullscreen;

        //Init settings to wanted, reading player prefs for preferences
        int Res = -1, FOV = -1, FPS = -1;
        Res = PlayerPrefs.GetInt("RES", -1);
        if (Res >= 0 && Res < allResolutions.Count)
            resDropdown.value = Res;
        FOV = PlayerPrefs.GetInt("FOV", -1);
        if (FOV >= 0 && FOV < fovDropdown.options.Count)
            fovDropdown.value = FOV;
        FPS = PlayerPrefs.GetInt("FPS", -1);
        if (FPS >= 0 && FPS < fpsDropdown.options.Count)
            fpsDropdown.value = FPS;

        //set to settings wanted
        ChangeFOV();
        ChangeFPS();
        ChangeRes();
    }

    // Use this for initialization
    void Start ()
    {
    }

    public void ChangeRes()
    {
        currentRes = resDropdown.value;
        Screen.SetResolution(allResolutions[currentRes].width, allResolutions[currentRes].height, isFullscreen, currentFps);

        //store in local data
        PlayerPrefs.SetInt("RES", currentRes);
    }

    public void ChangeFOV()
    {
        switch (fovDropdown.value)
        {
            case(0):
                currentFov = 60;
                break;
            case (1):
                currentFov = 65;
                break;
            case (2):
                currentFov = 69;
                break;
            case (3):
                currentFov = 75;
                break;
            case (4):
                currentFov = 78.5f;
                break;
        }

        Camera.main.fieldOfView = currentFov;

        //store in local data
        PlayerPrefs.SetInt("FOV", fovDropdown.value);
    }

    public void ChangeFPS()
    {
        switch (fpsDropdown.value)
        {
            case (0):
                currentFps = 60;
                break;
            case (1):
                currentFps = 120;
                break;
            case (2):
                currentFps = 200;
                break;
        }

        //store in local data
        PlayerPrefs.SetInt("FPS", fpsDropdown.value);

        ChangeRes();
    }

    public void SetFullscreen()
    {
        isFullscreen = fullscreenToggle.isOn;
        Screen.fullScreen = isFullscreen;
    }

    public void ChangeMusicVol()
    {
        musicVolume = musicSlider.value;
        audioManager_scr.ChangeMusicVol(musicVolume);

        //store in player prefs
        PlayerPrefs.SetFloat("musicvol", musicVolume);
    }

    public void ChangeSoundVol()
    {
        soundsVolume = soundSlider.value;
        audioManager_scr.ChangeSFXVol(soundsVolume);

        //store in player prefs
        PlayerPrefs.SetFloat("sfxvol", soundsVolume);

        //play ding to show sound changing
        if(initialised)
        {
            audioManager_scr.PlayDingSFX();
        }
    }
}
