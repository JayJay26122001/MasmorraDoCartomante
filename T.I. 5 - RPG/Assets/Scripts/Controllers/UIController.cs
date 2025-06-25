using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.VisualScripting.Antlr3.Runtime;
using NUnit.Framework.Interfaces;
using UnityEditor;

public class UIController : MonoBehaviour
{
    [HideInInspector] public bool gamePaused;
    [Header("Player Money Text")]
    public TextMeshPro money;
    [Header("Game Logo")]
    public GameObject gameLogo;
    [Header("Buttons")]
    public GameObject playButton;
    public GameObject settingsButton;
    public GameObject creditsButton;
    public GameObject quitButton;
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject quitPanel;
    public GameObject pausePanel;
    [Header("Panel Buttons")]
    public GameObject leaveSettingsPanelButton;
    public GameObject leaveCreditsPanelButton;
    public GameObject leaveQuitPanelButton;
    public GameObject confirmQuitButton;
    [Header("Settings Slider")]
    public Slider _masterVolumeSlider;
    public Slider _musicSlider;
    public Slider _sfxSlider;

    /*bool gameStarted, gamePaused;
    public GameObject pausePanel, confirmReturnRoomPanel, confirmReturnMenuPanel, collectablesPanel;
    public GameObject collectablesButton, returnRoomButton, returnMenuButton, confirmReturnRoomButton, confirmReturnMenuButton;
    public Slider _masterVolumeSlider, _musicSlider, _sfxSlider, _brightnessSlider;
    public TMP_Dropdown resDropdown, screenModeDropdown;
    Resolution[] allRes;
    List<Resolution> selectedResList = new List<Resolution>();
    public ConfigData data;
    */

    private void Awake()
    {
        UiSetup();
    }

    private void Start()
    {
        GameManager.instance.uiController = this;
        gamePaused = false;
        /*ResolutionDropdown();
        ScreenModeDropdown();
        if (File.Exists(Application.dataPath + "/configSave.json"))
        {
            SaveManager.LoadConfig();
            ConfigUpdate();
        }
        else
        {
            data = new ConfigData(true, 0, 0, 0, 0);
        }
        EventSystem.current.SetSelectedGameObject(playButton);
        SetDefaultVol();*/
        AudioController.instance.StartMusic();
    }

    /*private void Update()
    {
        if (SceneManager.GetActiveScene().name == "PressToStartScene")
        {
            if (!gameStarted && Input.anyKeyDown)
            {
                gameStarted = true;
                ChangeScene("Menu");
            }
        }
        
    }

    public void ConfigUpdate()
    {
        if (resDropdown != null)
        {
            resDropdown.value = data.selectedRes;
        }
        if (screenModeDropdown != null)
        {
            if (data.fullScreen)
            {
                screenModeDropdown.value = 0;
            }
            else
            {
                screenModeDropdown.value = 1;
            }
        }
        AudioController.controller.ChangeMasterVol(data.master);
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.value = data.master;
        }
        AudioController.controller.ChangeMusicVol(data.music);
        if (_musicSlider != null)
        {
            _musicSlider.value = data.music;
        }
        AudioController.controller.ChangeSFXVol(data.sfx);
        if (_sfxSlider != null)
        {
            _sfxSlider.value = data.sfx;
        }
    }
    public void SaveConfigs()
    {
        SaveManager.SaveConfig();
    }*/

    public void ChangeScene(string scene)
    {
        if(gamePaused)
        {
            Time.timeScale = 1.0f;
            gamePaused = false;
        }
        SceneManager.LoadScene(scene);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting!");
    }

    public void UpdateMoney(int m)
    {
        money.text = m + "$";
    }

    public void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
        if(SceneManager.GetActiveScene().name == "Menu")
        {
            SetMenuButtonsInteractable(false);
        }
        if(panel == quitPanel)
        {
            HideMenuObjects();
        }
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
        if(SceneManager.GetActiveScene().name == "Menu")
        {
            SetMenuButtonsInteractable(true);
        }
        if(panel == quitPanel)
        {
            ShowMenuObjects();
        }
        if (panel == pausePanel)
        {
            //GameManager.instance.player.movePaused = false;
            Time.timeScale = 1.0f;
            gamePaused = false;
        }
    }

    public void HideMenuObjects()
    {
        gameLogo.SetActive(false);
        playButton.SetActive(false);
        settingsButton.SetActive(false); 
        creditsButton.SetActive(false);
        quitButton.SetActive(false);
    }

    public void ShowMenuObjects()
    {
        gameLogo.SetActive(true);
        playButton.SetActive(true);
        settingsButton.SetActive(true);
        creditsButton.SetActive(true);
        quitButton.SetActive(true);
    }

    public void SetMenuButtonsInteractable(bool interactable)
    {
        playButton.GetComponent<Button>().interactable = interactable;
        settingsButton.GetComponent<Button>().interactable = interactable;
        creditsButton.GetComponent<Button>().interactable = interactable;
        quitButton.GetComponent<Button>().interactable = interactable;
    }

    public void UiSetup()
    {
        if (playButton != null)
        {
            playButton.SetActive(true);
        }
        if (settingsButton != null)
        {
            settingsButton.SetActive(true);
        }
        if (creditsButton != null)
        {
            creditsButton.SetActive(true);
        }
        if (quitButton != null)
        {
            quitButton.SetActive(true);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
        if (quitPanel != null)
        {
            quitPanel.SetActive(false);
        }
        if (leaveSettingsPanelButton != null)
        {
            leaveSettingsPanelButton.SetActive(true);
        }
        if (leaveCreditsPanelButton != null)
        {
            leaveCreditsPanelButton.SetActive(true);
        }
        if(leaveQuitPanelButton != null)
        {
            leaveQuitPanelButton.SetActive(true);
        }
        if (confirmQuitButton != null)
        {
            confirmQuitButton.SetActive(true);
        }
        /*
        if (resDropdown != null)
        {
            resDropdown.gameObject.SetActive(true);
        }
        if (screenModeDropdown != null)
        {
            screenModeDropdown.gameObject.SetActive(true);
        }
        if (itemWheel != null)
        {
            itemWheel.SetActive(false);
            Invoke("SetupItemWheelIcons", 0.1f);
        }
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        if (collectablesPanel != null)
        {
            collectablesPanel.SetActive(false);
        }
        if (confirmReturnRoomPanel != null)
        {
            confirmReturnRoomPanel.SetActive(false);
        }
        if (confirmReturnMenuPanel != null)
        {
            confirmReturnMenuPanel.SetActive(false);
        }*/
    }
    /*public void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
        if (panel == settingsPanel)
        {
            SetButtonsInteractable(false);
            EventSystem.current.SetSelectedGameObject(_masterVolumeSlider.gameObject);
        }
        if (panel == creditsPanel)
        {
            SetButtonsInteractable(false);
            EventSystem.current.SetSelectedGameObject(leaveCreditsPanelButton);
        }
        if (panel == quitPanel)
        {
            SetButtonsInteractable(false);
            EventSystem.current.SetSelectedGameObject(confirmQuitButton);
        }
        if (panel == pausePanel)
        {
            EventSystem.current.SetSelectedGameObject(settingsButton);
        }
        if(panel == collectablesPanel)
        {
            EventSystem.current.SetSelectedGameObject(xxx);
        }
        if (panel == confirmReturnRoomPanel)
        {
            EventSystem.current.SetSelectedGameObject(confirmReturnRoomButton);
        }
        if (panel == confirmReturnMenuPanel)
        {
            EventSystem.current.SetSelectedGameObject(confirmReturnMenuButton);
        }
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
        if (panel == settingsPanel)
        {
            SaveConfigs();
            SetButtonsInteractable(true);
            EventSystem.current.SetSelectedGameObject(settingsButton);
        }
        if (panel == creditsPanel)
        {
            SetButtonsInteractable(true);
            EventSystem.current.SetSelectedGameObject(creditsButton);
        }
        if (panel == quitPanel)
        {
            SetButtonsInteractable(true);
            EventSystem.current.SetSelectedGameObject(quitButton);
        }
        if (panel == pausePanel)
        {
            GameManager.instance.player.movePaused = false;
            gamePaused = false;
        }
        if(panel == collectablesPanel)
        {
            EventSystem.current.SetSelectedGameObject(xxx);
        }
        if (panel == confirmReturnRoomPanel)
        {
            EventSystem.current.SetSelectedGameObject(returnRoomButton);
        }
        if (panel == confirmReturnMenuPanel)
        {
            EventSystem.current.SetSelectedGameObject(returnMenuButton);
        }
    }

    public void ActivateItemWheel()
    {
        itemWheel.SetActive(true);
    }

    public void DeactivateItemWheel()
    {
        //GameManager.instance.player.movePaused = false;
        //GameManager.instance.player.clone.movePaused = false;
        GameManager.instance.UnpauseTime();
        itemWheel.SetActive(false);
    }*/

    public void ChangeMasterVolume()
    {
        //data.master = _masterVolumeSlider.value;
        AudioController.instance.ChangeMasterVol(_masterVolumeSlider.value);
    }
    public void ChangeMusicVolume()
    {
        //data.music = _musicSlider.value;
        AudioController.instance.ChangeMusicVol(_musicSlider.value);
    }
    public void ChangeSFXVolume()
    {
        //data.sfx = _sfxSlider.value;
        AudioController.instance.ChangeSFXVol(_sfxSlider.value);
    }
    /*public void SetDefaultVol()
    {
        AudioController.controller.mixer.GetFloat("MasterVol", out float aux1);
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.value = aux1;
        }
        AudioController.controller.mixer.GetFloat("MusicVol", out float aux2);
        if (_musicSlider != null)
        {
            _musicSlider.value = aux2;
        }
        AudioController.controller.mixer.GetFloat("SFXVol", out float aux3);
        if (_sfxSlider != null)
        {
            _sfxSlider.value = aux3;
        }
    }*/

    /*public void ResolutionDropdown()
    {
        allRes = Screen.resolutions;
        Array.Sort(allRes, (a, b) =>
        {
            int widthComparison = b.width.CompareTo(a.width);
            return widthComparison == 0 ? b.height.CompareTo(a.height) : widthComparison;
        });
        string newRes;
        List<string> resStringList = new List<string>();
        foreach (Resolution res in allRes)
        {
            float aspectRatio = (float)res.width / res.height;
            if (Math.Abs(aspectRatio - 16f / 9f) < 0.01f)
            {
                if (res.width >= 800)
                {
                    newRes = res.width.ToString() + "x" + res.height.ToString();
                    if (!resStringList.Contains(newRes))
                    {
                        resStringList.Add(newRes);
                        selectedResList.Add(res);
                    }
                }
            }
        }
        if (resDropdown != null)
        {
            resDropdown.ClearOptions();
            resDropdown.AddOptions(resStringList);
        }
    }

    public void ChangeRes()
    {
        data.selectedRes = resDropdown.value;
        Screen.SetResolution(selectedResList[data.selectedRes].width, selectedResList[data.selectedRes].height, data.fullScreen);
    }

    public void ScreenModeDropdown()
    {
        List<string> screenModes = new List<string> { "Fullscreen Mode", "Window Mode" };
        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(screenModes);
            screenModeDropdown.onValueChanged.AddListener((int index) =>
            {
                Debug.Log(index);
                if (index == 0)
                {
                    data.fullScreen = true;
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    Debug.Log("Fullscreen Mode");
                }
                else if (index == 1)
                {
                    data.fullScreen = false;
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    Debug.Log("Window Mode");
                }
            });
        }
    }

    public void SelectItemWheelButton(string selected)
    {
        if (selected == "lantern")
        {
            lanternButton.GetComponent<Image>().color = Color.red;
            remoteButton.GetComponent<Image>().color = Color.white;
            mirrorButton.GetComponent<Image>().color = Color.white;
        }
        if (selected == "remote")
        {
            remoteButton.GetComponent<Image>().color = Color.red;
            lanternButton.GetComponent<Image>().color = Color.white;
            mirrorButton.GetComponent<Image>().color = Color.white;
        }
        if (selected == "mirror")
        {
            mirrorButton.GetComponent<Image>().color = Color.red;
            remoteButton.GetComponent<Image>().color = Color.white;
            lanternButton.GetComponent<Image>().color = Color.white;
        }
        if (selected == "none")
        {
            mirrorButton.GetComponent<Image>().color = Color.white;
            remoteButton.GetComponent<Image>().color = Color.white;
            lanternButton.GetComponent<Image>().color = Color.white;
        }
    }*/

    public void PauseGameInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && SceneManager.GetActiveScene().name == "Game")
        {
            if (settingsPanel.activeSelf)
            {
                ClosePanel(settingsPanel);
            }
            else
            {
                if (gamePaused)
                {
                    // GameManager.instance.player.movePaused = false;
                    Time.timeScale = 1.0f;
                    ClosePanel(pausePanel);
                    gamePaused = false;
                    GameplayManager.instance.ResumeInput();
                }
                else
                {
                    //GameManager.instance.player.movePaused = true;
                    Time.timeScale = 0f;
                    OpenPanel(pausePanel);
                    gamePaused = true;
                    GameplayManager.instance.PauseInput();
                }
            }
        }
    }

    /*public void ShowPowerUpSelected(string selected)
    {
        if (selected == "lantern")
        {
            lanternHudImage.SetActive(true);
            remoteHudImage.SetActive(false);
            mirrorHudImage.SetActive(false);
        }
        if (selected == "remote")
        {
            lanternHudImage.SetActive(false);
            remoteHudImage.SetActive(true);
            mirrorHudImage.SetActive(false);
        }
        if (selected == "mirror")
        {
            lanternHudImage.SetActive(false);
            remoteHudImage.SetActive(false);
            mirrorHudImage.SetActive(true);
        }
    }

    public void AttPlayerHearts()
    {
        if (GameManager.instance.PlayerHP == 3)
        {
            firstHeart.isOn = true;
            secondHeart.isOn = true;
            thirdHeart.isOn = true;
        }
        if (GameManager.instance.PlayerHP == 2)
        {
            firstHeart.isOn = true;
            secondHeart.isOn = true;
            thirdHeart.isOn = false;
        }
        if (GameManager.instance.PlayerHP == 1)
        {
            firstHeart.isOn = true;
            secondHeart.isOn = false;
            thirdHeart.isOn = false;
        }
        if (GameManager.instance.PlayerHP <= 0)
        {
            firstHeart.isOn = false;
            secondHeart.isOn = false;
            thirdHeart.isOn = false;
        }
    }

    public void SetupItemWheelIcons()
    {
        if (itemWheel != null)
        {
            if (GameManager.instance.player.data.canReveal)
            {
                lanternButton.GetComponent<Image>().sprite = lanternOn;
                if (lanternUnlock != null)
                {
                    lanternUnlock.SetActive(false);
                }
            }
            else
            {
                lanternButton.GetComponent<Image>().sprite = lanternOff;
                if (lanternUnlock != null)
                {
                    lanternUnlock.SetActive(true);
                }
            }
            if (GameManager.instance.player.data.canDash)
            {
                remoteButton.GetComponent<Image>().sprite = remoteOn;
            }
            else
            {
                remoteButton.GetComponent<Image>().sprite = remoteOff;
            }
            if (GameManager.instance.player.data.canClone)
            {
                mirrorButton.GetComponent<Image>().sprite = mirrorOn;
                if (mirrorUnlock != null)
                {
                    mirrorUnlock.SetActive(false);
                }
            }
            else
            {
                mirrorButton.GetComponent<Image>().sprite = mirrorOff;
                if (mirrorUnlock != null)
                {
                    mirrorUnlock.SetActive(true);
                }
            }
        }
    }

    public void UnlockLantern()
    {
        lanternButton.GetComponent<Image>().sprite = lanternOn;
    }

    public void UnlockMirror()
    {
        mirrorButton.GetComponent<Image>().sprite = mirrorOn;
    }

    public void EnterChangeLevelConfirmationTrigger()
    {
        changeLevelConfirmation.SetActive(true);
    }

    public void LeaveChangeLevelConfirmationTrigger()
    {
        changeLevelConfirmation.SetActive(false);
    }

    public void AttFuel()
    {
        if (_fuelSlider != null)
        {
            _fuelSlider.value = GameManager.instance.player.revealFuel;
        }
    }
    
    public void ActivateObjectHud(GameObject obj)
    {
        obj.SetActive(true);
    }

    public void DeactivateObjectHud(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void IncreaseButtonScale(GameObject obj)
    {
        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        LeanTween.scale(obj, new Vector3(1.25f, 1.25f, 1.25f), 0.25f).setEase(LeanTweenType.easeOutQuad);
    }

    public void DecreaseButtonScale(GameObject obj)
    {
        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        LeanTween.scale(obj, new Vector3(1f, 1f, 1f), 0.25f).setEase(LeanTweenType.easeOutQuad);
    }*/
}
