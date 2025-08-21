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
    [HideInInspector] public float panelAnimTime = 0.5f;
    [Header("Player Money Text")]
    public TextMeshPro money;
    [Header("Game Logo")]
    public GameObject gameLogo;
    [Header("Buttons")]
    public GameObject playButton;
    public GameObject settingsButton;
    public GameObject creditsButton;
    public GameObject quitButton;
    public GameObject tutorialButton;
    public GameObject featuresButton;
    [Header("3D Buttons")]
    public GameObject play3DButton;
    public GameObject settings3DButton;
    public GameObject quit3DButton;
    public GameObject rightArrow;
    public GameObject leftArrow;
    public float spinTime;
    [Header("3D Objects")]
    public List<GameObject> masks;
    [Header("Panels")]
    public GameObject playPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject quitPanel;
    public GameObject pausePanel;
    public GameObject tutorialPanel;
    [Header("Panel Buttons")]
    public GameObject leaveSettingsPanelButton;
    public GameObject leaveCreditsPanelButton;
    public GameObject leaveQuitPanelButton;
    public GameObject confirmQuitButton;
    [Header("Settings Slider")]
    public Slider _masterVolumeSlider;
    public Slider _musicSlider;
    public Slider _sfxSlider;
    [Header("Game Combat HUD")]
    public GameObject combatHUD;
    [Header("Resolution and ScreenMode")]
    public TMP_Dropdown resDropdown, screenModeDropdown;
    [Header("Camera")]
    public Camera mainCamera;
    Resolution[] allRes;
    List<Resolution> selectedResList = new List<Resolution>();
    public ConfigData data;
    GameObject activeMask;
    int currentMaskIndex = 0;
    bool isMaskRotating = false;

    /*bool gameStarted, gamePaused;
    public GameObject pausePanel, confirmReturnRoomPanel, confirmReturnMenuPanel, collectablesPanel;
    public GameObject collectablesButton, returnRoomButton, returnMenuButton, confirmReturnRoomButton, confirmReturnMenuButton;
    public Slider _masterVolumeSlider, _musicSlider, _sfxSlider, _brightnessSlider;
    public TMP_Dropdown resDropdown, screenModeDropdown;
    Resolution[] allRes;
    List<Resolution> selectedResList = new List<Resolution>();
    */

    private void Awake()
    {
        UiSetup();
    }

    private void Start()
    {
        GameManager.instance.uiController = this;
        gamePaused = false;
        ResolutionDropdown();
        ScreenModeDropdown();
        if (File.Exists(Application.dataPath + "/configSave.json"))
        {
            SaveManager.LoadConfig();
            ConfigUpdate();
            Debug.Log("Your Configs!");
        }
        else
        {
            data = new ConfigData(0, 0, -7.5f, -7.5f, -7.5f);
            Debug.Log("Game Configs");
        }
        EventSystem.current.SetSelectedGameObject(playButton);
        SetDefaultVol();
        AudioController.instance.StartMusic();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        DetectClick();
    }

    public void ConfigUpdate()
    {
        if (resDropdown != null)
        {
            resDropdown.value = data.selectedRes;
        }
        if (screenModeDropdown != null)
        {
            screenModeDropdown.value = data.screenMode;
            /*if (data.screenMode == 0)
            {
                screenModeDropdown.value = 0;
            }
            else if(data.screenMode == 1)
            {
                screenModeDropdown.value = 1;
            }
            else
            {
                screenModeDropdown.value = 2;
            }*/
        }
        AudioController.instance.ChangeMasterVol(data.master);
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.value = data.master;
        }
        AudioController.instance.ChangeMusicVol(data.music);
        if (_musicSlider != null)
        {
            _musicSlider.value = data.music;
        }
        AudioController.instance.ChangeSFXVol(data.sfx);
        if (_sfxSlider != null)
        {
            _sfxSlider.value = data.sfx;
        }
    }
    public void SaveConfigs()
    {
        SaveManager.SaveConfig();
        Debug.Log("Configs Saved!");
    }

    public void PlayButtonSFX()
    {
        AudioController.instance.RandomizeSfx(AudioController.instance.sfxSource, AudioController.instance.buttonClickSfx);
    }

    public void UpscaleButton(GameObject button)
    {
        LeanTween.scale(button, Vector3.one * 1.2f, 0.15f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);
    }

    public void DownscaleButton(GameObject button)
    {
        LeanTween.scale(button, Vector3.one, 0.15f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);
    }

    public void ChangeScene(string scene)
    {
        SceneFadeController.instance.FadeOutToScene(scene);
        /*if(gamePaused)
        {
            Time.timeScale = 1.0f;
            gamePaused = false;
        }*/
        //SceneManager.LoadScene(scene);
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
        /*if(SceneManager.GetActiveScene().name == "Menu")
        {
            if (panel == playPanel || panel == settingsPanel || panel == quitPanel)
            {
                HideMenuObjects();
            }
        }*/
        if(panel != pausePanel)
        {
            if (panel == playPanel || panel == settingsPanel || panel == quitPanel)
            {
                if (SceneManager.GetActiveScene().name == "Menu")
                {
                    HideMenuObjects();
                }
                panel.SetActive(true);
                panel.transform.localScale = Vector3.zero;
                LeanTween.scale(panel, Vector3.one, panelAnimTime).setEase(LeanTweenType.easeInOutSine).setIgnoreTimeScale(true);
            }
            else
            {
                if (panel == tutorialPanel) { InternPanel(playPanel, tutorialPanel); }
                else if (panel == creditsPanel) { InternPanel(settingsPanel, creditsPanel); }
            }
        }
        else
        {
            panel.SetActive(true);
        }
    }

    public void ClosePanel(GameObject panel)
    {
        if(panel == pausePanel)
        {
            panel.SetActive(false);
            ActivateChildrens(combatHUD);
            Time.timeScale = 1.0f;
            gamePaused = false;
            GameplayManager.instance.ResumeInput();
            Button[] buttons = panel.GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                DownscaleButton(btn.gameObject);
            }
        }
        else
        {
            if (panel == tutorialPanel || panel == creditsPanel)
            {
                if (panel == tutorialPanel) { InternPanel(tutorialPanel, playPanel); }
                else if (panel == creditsPanel) { InternPanel(creditsPanel, settingsPanel); }
            }
            else
            {
                LeanTween.scale(panel, Vector3.zero, panelAnimTime).setEase(LeanTweenType.easeInOutSine).setIgnoreTimeScale(true)
                .setOnComplete(() =>
                {
                    panel.SetActive(false);
                    if (SceneManager.GetActiveScene().name == "Menu")
                    {
                        if (panel != creditsPanel && panel != tutorialPanel)
                        {
                            ShowMenuObjects();
                        }
                    }
                });
            }
        }
    }

    public void InternPanel(GameObject closePanel, GameObject openPanel) 
    { 
        LeanTween.scale(closePanel, Vector3.zero, panelAnimTime).setEase(LeanTweenType.easeInOutSine).setIgnoreTimeScale(true)
            .setOnComplete(() => 
            { 
                closePanel.SetActive(false); openPanel.SetActive(true); openPanel.transform.localScale = Vector3.zero; 
                LeanTween.scale(openPanel, Vector3.one, panelAnimTime).setEase(LeanTweenType.easeInOutSine).setIgnoreTimeScale(true); 
            }); 
    }

    public void HideMenuObjects()
    {
        /*if (gameLogo != null) gameLogo.SetActive(false);
        if (playButton != null) playButton.SetActive(false);
        if (settingsButton != null) settingsButton.SetActive(false);
        if (creditsButton != null) creditsButton.SetActive(false);
        if (quitButton != null) quitButton.SetActive(false);
        if (tutorialButton != null) tutorialButton.SetActive(false);
        if (featuresButton != null) featuresButton.SetActive(false);*/
        CheckActiveMask();
        if (masks[0] != null) masks[0].SetActive(false);
        if (masks[1] != null) masks[1].SetActive(false);
        if (masks[2] != null) masks[2].SetActive(false);
        if (rightArrow != null) rightArrow.SetActive(false);
        if (leftArrow != null) leftArrow.SetActive(false);
    }

    public void CheckActiveMask() //verificar qual máscara está ativa antes de esconder os objetos do menu
    {
        if (masks[0] != null && masks[0].activeSelf) activeMask = masks[0];
        else if (masks[1] != null && masks[1].activeSelf) activeMask = masks[1];
        else if (masks[2] != null && masks[2].activeSelf) activeMask = masks[2];
    }

    public void ShowMenuObjects()
    {
        /*if (gameLogo != null) gameLogo.SetActive(true);
        if (playButton != null) playButton.SetActive(true);
        if (settingsButton != null) settingsButton.SetActive(true);
        if (creditsButton != null) creditsButton.SetActive(true);
        if (quitButton != null) quitButton.SetActive(true);
        if (tutorialButton != null) tutorialButton.SetActive(true);
        if (featuresButton != null) featuresButton.SetActive(true);*/
        if (activeMask != null) activeMask.SetActive(true); //ativar a máscara correta
        if (rightArrow != null) rightArrow.SetActive(true);
        if (leftArrow != null) leftArrow.SetActive(true);

    }

    public void SetMenuButtonsInteractable(bool interactable)
    {
        if (playButton != null) playButton.GetComponent<Button>().interactable = interactable;
        if (settingsButton != null) settingsButton.GetComponent<Button>().interactable = interactable;
        if (creditsButton != null) creditsButton.GetComponent<Button>().interactable = interactable;
        if (quitButton != null) quitButton.GetComponent<Button>().interactable = interactable;
        if (tutorialButton != null) tutorialButton.GetComponent<Button>().interactable = interactable;
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
        if (tutorialButton != null)
        {
            tutorialButton.SetActive(true);
        }
        if (featuresButton != null)
        {
            featuresButton.SetActive(true);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
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
        if (resDropdown != null)
        {
            resDropdown.gameObject.SetActive(true);
        }
        if (screenModeDropdown != null)
        {
            screenModeDropdown.gameObject.SetActive(true);
        }
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
    }*/

    public void ChangeMasterVolume()
    {
        data.master = _masterVolumeSlider.value;
        AudioController.instance.ChangeMasterVol(_masterVolumeSlider.value);
    }
    public void ChangeMusicVolume()
    {
        data.music = _musicSlider.value;
        AudioController.instance.ChangeMusicVol(_musicSlider.value);
    }
    public void ChangeSFXVolume()
    {
        data.sfx = _sfxSlider.value;
        AudioController.instance.ChangeSFXVol(_sfxSlider.value);
    }
    public void SetDefaultVol()
    {
        if(AudioController.instance.mixer != null)
        {
            AudioController.instance.mixer.GetFloat("MasterVol", out float aux1);
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.value = aux1;
            }
            AudioController.instance.mixer.GetFloat("MusicVol", out float aux2);
            if (_musicSlider != null)
            {
                _musicSlider.value = aux2;
            }
            AudioController.instance.mixer.GetFloat("SFXVol", out float aux3);
            if (_sfxSlider != null)
            {
                _sfxSlider.value = aux3;
            }
        }
    }

    public void ResolutionDropdown()
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
        Screen.SetResolution(selectedResList[data.selectedRes].width, selectedResList[data.selectedRes].height, Screen.fullScreenMode);
    }

    public void ScreenModeDropdown()
    {
        List<string> screenModes = new List<string> { "Fullscreen Mode", "Borderless Mode", "Window Mode" };
        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(screenModes);
            screenModeDropdown.onValueChanged.AddListener((int index) =>
            {
                //Debug.Log(index);
                if (index == 0)
                {
                    data.screenMode = 0;
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                }
                else if (index == 1)
                {
                    data.screenMode = 1;
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                }
                else if (index == 2)
                {
                    data.screenMode = 2;
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                }
            });
        }
    }

    public void PauseGameInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && SceneManager.GetActiveScene().name == "Game")
        {
            if (settingsPanel.activeSelf)
            {
                SaveConfigs();
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
                    //combatHUD.SetActive(false);
                    Time.timeScale = 0f;
                    DeactivateChildrens(combatHUD);
                    OpenPanel(pausePanel);
                    gamePaused = true;
                    GameplayManager.instance.PauseInput();
                }
            }
        }
    }

    public void DeactivateChildrens(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void ActivateChildrens(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void RotateRight()
    {
        if (isMaskRotating) return;
        isMaskRotating = true;
        PlayButtonSFX();
        int nextIndex = (currentMaskIndex + 1) % masks.Count;
        GameObject currentModel = masks[currentMaskIndex];
        GameObject nextModel = masks[nextIndex];
        float halfDuration = spinTime / 2f;
        LeanTween.rotateAroundLocal(currentModel, Vector3.down, 720f, halfDuration).setEase(LeanTweenType.easeInQuad);
        LeanTween.rotateAroundLocal(nextModel, Vector3.down, 720f, halfDuration).setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() =>
            {
                nextModel.SetActive(true);
                currentModel.SetActive(false);
                currentMaskIndex = nextIndex;
                LeanTween.rotateAroundLocal(nextModel, Vector3.down, 720f, halfDuration).setEase(LeanTweenType.easeOutQuad)
                    .setOnComplete(() => { isMaskRotating = false; });
            });
    }

    public void RotateLeft()
    {
        if (isMaskRotating) return;
        isMaskRotating = true;
        PlayButtonSFX();
        int prevIndex = (currentMaskIndex - 1 + masks.Count) % masks.Count;
        GameObject currentModel = masks[currentMaskIndex];
        GameObject prevModel = masks[prevIndex];
        float halfDuration = spinTime / 2f;

        LeanTween.rotateAroundLocal(currentModel, Vector3.up, 720f, halfDuration).setEase(LeanTweenType.easeInQuad);
        LeanTween.rotateAroundLocal(prevModel, Vector3.up, 720f, halfDuration).setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() =>
            {
                prevModel.SetActive(true);
                currentModel.SetActive(false);
                currentMaskIndex = prevIndex;
                LeanTween.rotateAroundLocal(prevModel, Vector3.up, 720f, halfDuration).setEase(LeanTweenType.easeOutQuad)
                    .setOnComplete(() => { isMaskRotating = false; }); ;
            });
    }
    public void DetectClick()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !isMaskRotating)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;

                if (clicked == play3DButton)
                {
                    OpenPanel(playPanel);
                    PlayButtonSFX();
                }
                else if (clicked == settings3DButton)
                {
                    OpenPanel(settingsPanel);
                    PlayButtonSFX();
                }
                else if (clicked == quit3DButton)
                {
                    OpenPanel(quitPanel);
                    PlayButtonSFX();
                }
                else if (clicked == rightArrow)
                {
                    RotateRight();
                }
                else if (clicked == leftArrow)
                {
                    RotateLeft();
                }
            }
        }
    }
}
