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
//using UnityEditor.SearchService;
using Unity.Jobs;

public class UIController : MonoBehaviour
{
    string url = "https://docs.google.com/forms/d/e/1FAIpQLSe1sb1dvxVZLIG0wdQMp0D78u4RJpcvYDlaWjs45Ud-K78l1w/viewform";
    [HideInInspector] public bool gamePaused;
    [HideInInspector] public float panelAnimTime = 0.5f;
    [Header("Player Money Text")]
    public TextMeshPro money;
    [Header("Game Logo")]
    public GameObject gameLogo;
    [Header("Buttons")]
    public GameObject newGameButton;
    public GameObject continueButton;
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
    public GameObject gameNameUp;
    public GameObject gameNameDown;
    public float spinTime;
    [Header("3D Objects")]
    public List<GameObject> masks;
    [Header("Panels")]
    public GameObject playPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject changeLogPanel;
    public GameObject quitPanel;
    public GameObject pausePanel;
    public GameObject tutorialPanel;
    [Header("Panel Buttons")]
    public GameObject leaveSettingsPanelButton;
    public GameObject leaveCreditsPanelButton;
    public GameObject leaveQuitPanelButton;
    public GameObject confirmQuitButton;
    [Header("Settings Sliders and Toggles")]
    public Slider _masterVolumeSlider;
    public Slider _musicSlider;
    public Slider _sfxSlider;
    public Toggle vsyncToggle;
    public Image vsyncToggleImage;
    public Toggle cardDescToggle;
    public Image cardDescToggleImage;
    public Sprite toggleConfirm;
    public Sprite toggleDeny;
    [Header("Game Combat HUD")]
    public GameObject combatHUD;
    public TextMeshPro enemyName;
    [Header("ShopUI")]
    public GameObject shopObjectHUD;
    public TextMeshProUGUI shopObjectDescription;
    [Header("Ingame Popup")]
    public GameObject ingamePopup;
    public TextMeshProUGUI ingamePopupDescription;
    List<GameObject> popups = new List<GameObject>();
    [Header("Command Popup")]
    public GameObject commandPopup;
    List<GameObject> commands = new List<GameObject>();
    int shownCommandsCount = 0;
    Vector2 basePos;
    [Header("Resolution and ScreenMode")]
    public TMP_Dropdown resDropdown, screenModeDropdown;
    [Header("Camera")]
    public Camera mainCamera;
    [Header("Canvas")]
    public Transform canvas;
    Resolution[] allRes;
    List<Resolution> selectedResList = new List<Resolution>();
    public ConfigData data;
    GameObject activeMask;
    int currentMaskIndex = 0;
    bool isMaskRotating = false;
    //Vector3 relativeInfocardPos;
    public GameObject pageLeftArrow;
    public GameObject pageRightArrow;
    public TextMeshPro pageIndex;
    public GameObject LockObj;
    public Transform playerLockAnchor, enemyLockAnchor;
    [HideInInspector] public int currentPage = 0, cardsPerPage = 14, totalPages = 1, previousPage = 0;
    [HideInInspector] public bool isPageChanging = false;
    [HideInInspector] public List<CardDisplay> currentCards;
    [HideInInspector] public bool cardDescOn = false;

    [Header("Enemy Description")]
    public GameObject enemyDesc;
    public TextMeshPro enemyDescName;
    public TextMeshPro enemyHp;
    public TextMeshPro enemyShield;
    public TextMeshPro enemyEnergy;
    public TextMeshPro enemyDamageTaken;
    public TextMeshPro enemyBaseDamage;
    public TextMeshPro enemyShieldGain;
    public TextMeshPro enemyCardsInHand;
    public TextMeshPro enemyCardsInDiscard;
    public TextMeshPro enemyCardsInBuying;
    public Transform enemyDescPos;
    public Transform enemyDescOutPos;
    bool isEnemyDescOn = false;

    [Header("Tutorial Popup")]
    public GameObject tutorialPopup;
    public Transform tutorialPopupFather;
    TutorialPopup currentTutorialPopup;

    private void Awake()
    {
        UiSetup();
        if(!popups.Contains(ingamePopup) && ingamePopup != null)
        {
            popups.Add(ingamePopup);
        }
        if(!commands.Contains(commandPopup) && commandPopup != null)
        {
            basePos = commandPopup.GetComponent<RectTransform>().anchoredPosition;
            commands.Add(commandPopup);
        }
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
        }
        else
        {
            data = new ConfigData(0, 0, -7.5f, -7.5f, -7.5f, false, false);
        }
        EventSystem.current.SetSelectedGameObject(continueButton);
        SetDefaultVol();
        //AudioController.instance.StartMusic();
        mainCamera = Camera.main;
        bool isOn = QualitySettings.vSyncCount == 1;
        if(vsyncToggle != null)
        {
            vsyncToggle.isOn = isOn;
            UpdateVsyncToggleImages(isOn);
            vsyncToggle.onValueChanged.AddListener(ChangeVsyncToggle);
        }
        cardDescOn = data.cardDescEnabled;
        if (cardDescToggle != null)
        {
            cardDescToggle.isOn = cardDescOn;
            UpdateCardDescToggleImages(cardDescOn);
            cardDescToggle.onValueChanged.AddListener(ChangeCardDescToggle);
        }
        if (File.Exists(Application.dataPath + "/boardSave.json") && File.Exists(Application.dataPath + "/playerSave.json"))
        {
            ContinueButton(true);
        }
        else
        {
            ContinueButton(false);
        }
        AudioController.instance.StartMusic();
        /*if(enemyDesc != null)
        {
            relativeInfocardPos = enemyDesc.transform.position - CameraController.instance.cameras[3].transform.position;
        }*/
    }

    private void Update()
    {
        DetectClick();
    }

    public void ContinueButton(bool isActive)
    {
        if(continueButton != null)
        {
            continueButton.GetComponent<Button>().interactable = isActive;
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
            screenModeDropdown.value = data.screenMode;
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
        if(vsyncToggle != null)
        {
            vsyncToggle.isOn = data.vsyncEnabled;
            QualitySettings.vSyncCount = data.vsyncEnabled ? 1 : 0;
            UpdateVsyncToggleImages(data.vsyncEnabled);
        }
        if(cardDescToggle != null)
        {
            cardDescToggle.isOn = data.cardDescEnabled;
            cardDescOn = data.cardDescEnabled;
            UpdateCardDescToggleImages(data.cardDescEnabled);
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
        if(button.GetComponent<Button>().interactable)
        {
            LeanTween.scale(button, Vector3.one * 1.2f, 0.15f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);
        }
    }

    public void DownscaleButton(GameObject button)
    {
        if (button.GetComponent<Button>().interactable)
        {
            LeanTween.scale(button, Vector3.one, 0.15f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);
        }
    }

    public void ChangeScene(string scene)
    {
        SceneFadeController.instance.FadeOutToScene(scene);
    }

    public void NewGame()
    {
        File.Delete(Application.dataPath + "/boardSave.json");
        File.Delete(Application.dataPath + "/playerSave.json");
        SceneFadeController.instance.FadeOutToScene("Game");
    }

    public void OpenFeedbackLink()
    {
        Application.OpenURL(url);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting!");
    }

    public void UpdateMoney(int m)
    {
        money.text = "$" + m;
    }

    public void OpenPanel(GameObject panel)
    {
        if (panel != pausePanel)
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
                else if(panel == changeLogPanel) { InternPanel(settingsPanel, changeLogPanel); }
            }
        }
        else
        {
            panel.SetActive(true);
        }
    }

    public void ClosePanel(GameObject panel)
    {
        if (panel == pausePanel)
        {
            panel.SetActive(false);
            ActivateChildrens(combatHUD);
            Time.timeScale = 1.0f;
            gamePaused = false;
            GameplayManager.instance.IResumeInput();
            Button[] buttons = panel.GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                DownscaleButton(btn.gameObject);
            }
        }
        else
        {
            if (panel == tutorialPanel || panel == creditsPanel || panel == changeLogPanel)
            {
                if (panel == tutorialPanel) { InternPanel(tutorialPanel, playPanel); }
                else if (panel == creditsPanel) { InternPanel(creditsPanel, settingsPanel); }
                else if (panel == changeLogPanel) { InternPanel(changeLogPanel, settingsPanel); }
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
        CheckActiveMask();
        if (masks[0] != null) masks[0].SetActive(false);
        if (masks[1] != null) masks[1].SetActive(false);
        if (masks[2] != null) masks[2].SetActive(false);
        if (rightArrow != null) rightArrow.SetActive(false);
        if (leftArrow != null) leftArrow.SetActive(false);
        if (gameNameUp != null) gameNameUp.SetActive(false);
        if (gameNameDown != null) gameNameDown.SetActive(false);
    }

    public void CheckActiveMask() //verificar qual m�scara est� ativa antes de esconder os objetos do menu
    {
        if (masks[0] != null && masks[0].activeSelf) activeMask = masks[0];
        else if (masks[1] != null && masks[1].activeSelf) activeMask = masks[1];
        else if (masks[2] != null && masks[2].activeSelf) activeMask = masks[2];
    }

    public void ShowMenuObjects()
    {
        if (activeMask != null) activeMask.SetActive(true); //ativar a m�scara correta
        if (rightArrow != null) rightArrow.SetActive(true);
        if (leftArrow != null) leftArrow.SetActive(true);
        if (gameNameUp != null) gameNameUp.SetActive(true);
        if (gameNameDown != null) gameNameDown.SetActive(true);
    }

    public void SetMenuButtonsInteractable(bool interactable)
    {
        if (continueButton != null) continueButton.GetComponent<Button>().interactable = interactable;
        if (settingsButton != null) settingsButton.GetComponent<Button>().interactable = interactable;
        if (creditsButton != null) creditsButton.GetComponent<Button>().interactable = interactable;
        if (quitButton != null) quitButton.GetComponent<Button>().interactable = interactable;
        if (tutorialButton != null) tutorialButton.GetComponent<Button>().interactable = interactable;
    }

    public void UiSetup()
    {
        if (continueButton != null)
        {
            continueButton.SetActive(true);
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
        if (changeLogPanel != null)
        {
            changeLogPanel.SetActive(false);
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
        if (leaveQuitPanelButton != null)
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
        if (shopObjectHUD != null)
        {
            shopObjectHUD.gameObject.SetActive(false);
        }
    }

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
        if (AudioController.instance.mixer != null)
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
                    Time.timeScale = 1.0f;
                    ClosePanel(pausePanel);
                    gamePaused = false;
                    GameplayManager.instance.IResumeInput();
                }
                else
                {
                    Time.timeScale = 0f;
                    DeactivateChildrens(combatHUD);
                    OpenPanel(pausePanel);
                    gamePaused = true;
                    HidePopups();
                    GameplayManager.instance.IPauseInput();
                }
            }
        }
        if(context.phase == InputActionPhase.Started && SceneManager.GetActiveScene().name == "Menu")
        {
            if (settingsPanel.activeSelf)
            {
                SaveConfigs();
                ClosePanel(settingsPanel);
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
        if(SceneManager.GetActiveScene().name == "Menu")
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
        else if(SceneManager.GetActiveScene().name == "Game")
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && !isPageChanging)
            {
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    GameObject clicked = hit.collider.gameObject;

                    if (clicked == pageLeftArrow)
                    {
                        OnClickPreviousPage();
                        PlayButtonSFX();
                    }
                    else if (clicked == pageRightArrow)
                    {
                        OnClickNextPage();
                        PlayButtonSFX();
                    }
                }
            }
        }
    }

    public void ChangeNamePlate(string name)
    {
        enemyName.text = name;
    }

    public void ShopDescHUD(bool active, string s)
    {
        shopObjectHUD.SetActive(active);
        shopObjectDescription.text = s;
    }

    public void ShowCommandPopup(ControlUI cUI)
    {
        if(shownCommandsCount >= commands.Count)
        {
            GameObject aux = Instantiate(commandPopup, canvas);
            commands.Add(aux);
        }
        commands[shownCommandsCount].SetActive(false);
        commands[shownCommandsCount].GetComponentInChildren<TextMeshProUGUI>().text = cUI.command;
        GameObject img = commands[shownCommandsCount].transform.GetChild(0).gameObject;
        img.GetComponent<Image>().sprite = cUI.image;
        img.GetComponent<RectTransform>().sizeDelta = new Vector2((cUI.image.bounds.size.x * 50) / cUI.image.bounds.size.y, 50);
        shownCommandsCount++;
        AdjustCommandsPositions();
    }

    public void HideCommandPopup(string command)
    {
        if (shownCommandsCount > 0)
        {
            bool found = false;
            for(int i = 0; i < shownCommandsCount - 1; i++)
            {
                if (!found)
                {
                    if (commands[i] != null && String.Compare(commands[i].GetComponentInChildren<TextMeshProUGUI>().text, command) == 0)
                    {
                        found = true;
                    }
                }
                if(found)
                {
                    commands[i].GetComponentInChildren<TextMeshProUGUI>().text = commands[i + 1].GetComponentInChildren<TextMeshProUGUI>().text;
                    GameObject img = commands[i].transform.GetChild(0).gameObject;
                    Sprite aux = commands[i + 1].transform.GetChild(0).gameObject.GetComponent<Image>().sprite;
                    img.GetComponent<Image>().sprite = aux;
                    img.GetComponent<RectTransform>().sizeDelta = new Vector2((aux.bounds.size.x * 50) / aux.bounds.size.y, 50);
                }
            }
            if (found || (commands[shownCommandsCount - 1] != null && String.Compare(commands[shownCommandsCount - 1].GetComponentInChildren<TextMeshProUGUI>().text, command) == 0))
            {
                shownCommandsCount--;
                AdjustCommandsPositions();
            }
        }
    }
    public void AdjustCommandsPositions()
    {
        commands[0].SetActive(shownCommandsCount != 0 && GameplayManager.instance.InputActive);
        float gap = 10;
        RectTransform aux;
        for (int i = 1; i < commands.Count; i++)
        {
            if(i < shownCommandsCount)
            { 
                aux = commands[i - 1].GetComponent<RectTransform>();
                commands[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(basePos.x, aux.anchoredPosition.y + aux.sizeDelta.y + gap);
                commands[i].SetActive(GameplayManager.instance.InputActive);
            }
            else
            {
                commands[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                commands[i].SetActive(false);
            }
        }
    }

    public void ShowPopups(CardDisplay card)
    {
        int quantity = card.VerifyCardPopupQuantity();
        if(quantity == 0) //no popups
        {
            HidePopups();
            return;
        }
        else if (quantity <= popups.Count) //hide some popups and show the ones based on the quantity
        {
            for (int i = 0; i < popups.Count; i++)
            {
                if (i < quantity)
                {
                    popups[i].SetActive(true);
                    UpdatePopupUI(popups[i], card, i);
                }
                else
                {
                    popups[i].SetActive(false);
                }
            }
            return;
        }
        else //create popup 
        {
            float width = 400f;
            float gap = 25f;
            RectTransform baseRect = ingamePopup.GetComponent<RectTransform>();
            float baseY = baseRect.anchoredPosition.y;
            float baseX = baseRect.anchoredPosition.x;
            for (int i = 0; i < quantity; i++)
            {
                if(i < popups.Count)
                {
                    popups[i].SetActive(true);
                    UpdatePopupUI(popups[i], card, i);
                }
                else
                {
                    float offsetX = - (i * (width + gap));
                    GameObject newPopup = Instantiate(ingamePopup, canvas);
                    RectTransform popupRect = newPopup.GetComponent<RectTransform>();
                    popupRect.anchoredPosition = new Vector2(baseX + offsetX, baseY);
                    popups.Add(newPopup);
                    UpdatePopupUI(newPopup, card, i);
                }
            }
            return;
        }
    }

    public void HidePopups()
    {
        foreach(var p in popups)
        {
            p.SetActive(false);
        }
    }

    public void UpdatePopupUI(GameObject popup, CardDisplay card, int index)
    {
        var text = popup.GetComponentInChildren<TextMeshProUGUI>();
        text.text = PopUpText(card)[index];
        LayoutRebuilder.ForceRebuildLayoutImmediate(text.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(popup.GetComponent<RectTransform>());
    }

    public List<string> PopUpText(CardDisplay card)
    {
        List<string> popupTexts = new List<string>();

        if(!string.IsNullOrEmpty(card.cardData.Description) && cardDescOn)
        {
            popupTexts.Add(card.ReplacePopupTokens(card.cardData.Description, card));
        }

        if (!string.IsNullOrEmpty(card.cardData.extraDescription))
        {
            popupTexts.Add(card.ReplacePopupTokens(card.cardData.extraDescription, card));
        }

        if (card.cardData.instantaneous)
        {
            popupTexts.Add("<color=#FFD700>Instantaneous</color>\nThis card activates instantaneously when used!");
        }

        if (card.cardData.limited)
        {
            popupTexts.Add("<color=#FFD700>Limited</color>\nThis card can be used only once!");
        }

        return popupTexts;
    }

    public void ChangeVsyncToggle(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        data.vsyncEnabled = isOn;
        UpdateVsyncToggleImages(isOn);
    }
    public void ChangeCardDescToggle(bool isOn)
    {
        cardDescOn = isOn;
        data.cardDescEnabled = isOn;
        UpdateCardDescToggleImages(isOn);
    }

    public void UpdateVsyncToggleImages(bool isOn)
    {
        if(isOn)
        {
            vsyncToggleImage.sprite = toggleConfirm;
        }
        else
        {
            vsyncToggleImage.sprite = toggleDeny;
        }
    }

    public void UpdateCardDescToggleImages(bool isOn)
    {
        if (isOn)
        {
            cardDescToggleImage.sprite = toggleConfirm;
        }
        else
        {
            cardDescToggleImage.sprite = toggleDeny;
        }
    }

    public void EnemyDescription()
    {
        var activeEnemy = GameplayManager.currentCombat.combatents[1];
        float multiplier = activeEnemy.BaseDamageTaken / 100f;
        int totalBuying = 0, totalDiscard = 0;

        enemyDescName.text = activeEnemy.name;
        enemyHp.text = "<color=#FF5555>" + activeEnemy.Health + "/" + activeEnemy.MaxHP + "</color>";
        enemyShield.text = "<color=#55AAFF>" + activeEnemy.Shield + "</color>";
        enemyEnergy.text  = "<color=#00A400>" + activeEnemy.Energy + "</color>";
        enemyBaseDamage.text = activeEnemy.BaseDamage.ToString();
        enemyShieldGain.text =activeEnemy.BaseShieldGain.ToString();
        enemyDamageTaken.text = "x" + multiplier.ToString("0.##");
        enemyCardsInHand.text = activeEnemy.hand.Count.ToString() + " Cards";
        foreach (var deck in activeEnemy.decks)
        {
            totalBuying += deck.BuyingPile.Count;
            totalDiscard += deck.DiscardPile.Count;
        }
        enemyCardsInBuying.text = totalBuying.ToString() + " Cards";
        enemyCardsInDiscard.text = totalDiscard.ToString() + " Cards";
    }

    public void ActivateEnemyDesc()
    {
        isEnemyDescOn = true;
        enemyDesc.SetActive(true);
        //Vector3 camPos = Camera.main.transform.position;
        //Vector3 worldPos = relativeInfocardPos + camPos;
        //Vector3 cardPos = new Vector3(enemyDescOutPos.transform.position.x, worldPos.y, worldPos.z);
        //enemyDesc.transform.position = cardPos;
        //enemyDescPos.position = cardPos + transform.right * -37.5f;
        LeanTween.move(enemyDesc, enemyDescPos, 0.2f);
    }

    public void HideEnemyDesc()
    {
        isEnemyDescOn = false;
        Vector3 outPos = enemyDesc.transform.position + transform.right * 37.5f;
        LeanTween.move(enemyDesc, outPos, 0.2f).setOnComplete(() =>
        {
            enemyDesc.SetActive(false);
        });
    }

    public void ShowEnemyDescription()
    {
        GameplayManager.instance.PauseInput(0.1f);
        EnemyDescription();
        if(isEnemyDescOn)
        {
            ActionController.instance.InvokeTimer(CameraController.instance.switchToLastCamera, 0.05f);
            if(GameplayManager.instance.player.enemy.gameObject.GetComponent<Enemy>() != null)
            {
                GameplayManager.instance.player.enemy.gameObject.GetComponent<Enemy>().ChangeInteraction(true);
            }
            else if(GameplayManager.instance.player.enemy.gameObject.GetComponent<Boss>() != null)
            {
                GameplayManager.instance.player.enemy.gameObject.GetComponent<Boss>().ChangeInteraction(true);
            }
            HideEnemyDesc();
        }
        else
        {
            if (GameplayManager.instance.player.enemy.gameObject.GetComponent<Enemy>() != null)
            {
                GameplayManager.instance.player.enemy.gameObject.GetComponent<Enemy>().ChangeInteraction(false);
            }
            else if (GameplayManager.instance.player.enemy.gameObject.GetComponent<Boss>() != null)
            {
                GameplayManager.instance.player.enemy.gameObject.GetComponent<Boss>().ChangeInteraction(false);
            }
            CameraController.instance.ChangeCamera(3);
            ActionController.instance.InvokeTimer(ActivateEnemyDesc, 0.15f);
        }
    }

    public void PageSystemCheck(int totalCards)
    {
        if(totalCards <= 16)
        {
            HidePageObjects();
        }
        else
        {
            ActivatePageObjects();
        }
    }

    public void ActivatePageObjects()
    {
        pageLeftArrow.SetActive(true);
        pageRightArrow.SetActive(true);
        pageIndex.gameObject.SetActive(true);
    }

    public void HidePageObjects()
    {
        pageLeftArrow.SetActive(false);
        pageRightArrow.SetActive(false);
        pageIndex.gameObject.SetActive(false);
    }

    public void AttPageIndex(int value)
    {
        pageIndex.text = value.ToString();
    }

    public void SetupPage(List<CardDisplay> cards)
    {
        currentCards = cards;
        currentPage = 0;
        totalPages = Mathf.CeilToInt((float)cards.Count / cardsPerPage);
        PageSystemCheck(cards.Count);
        AttPageIndex(currentPage + 1);
        previousPage = -1;
        ShowPage(currentPage, true);
    }

    public void ShowPage(int page, bool first)
    {
        if (currentCards == null || currentCards.Count == 0) return;
        isPageChanging = true;
        int totalCards = currentCards.Count;
        totalPages = Mathf.CeilToInt((float)totalCards / cardsPerPage);
        currentPage = Mathf.Clamp(page, 0, totalPages - 1);
        foreach (CardDisplay card in currentCards)
        {
            if(!first)
            {
                card.CardDisapearanceAnimation(true);
                ActionController.instance.InvokeTimer(card.gameObject.SetActive, false, 1);
            }
            else
            {
                card.gameObject.SetActive(false);
            }
        }
        int startIndex = currentPage * cardsPerPage;
        int endIndex = Mathf.Min(startIndex + cardsPerPage, totalCards);

        List<CardDisplay> visibleCards = new List<CardDisplay>();
        for (int i = startIndex; i < endIndex; i++)
        {
            if(!first)
            {
                ActionController.instance.InvokeTimer(currentCards[i].gameObject.SetActive, true, 1);
                ActionController.instance.InvokeTimer(currentCards[i].CardDisapearanceAnimation, false, 1);
            }
            else
            {
                currentCards[i].gameObject.SetActive(true);
            }
            visibleCards.Add(currentCards[i]);
        }
        CardUIController.OrganizeAllDeckCards(visibleCards);
        ActionController.instance.InvokeTimer(() => isPageChanging = false, 1);
        AttPageIndex(currentPage + 1);
        if (currentCards.Count <= 14)
        {
            HidePageObjects();
        }
        else
        {
            UpdatePageButtons(currentPage, totalPages, previousPage);
        }
    }

    public void UpdatePageButtons(int currentPage, int totalPages, int previousPage)
    {
        pageLeftArrow.GetComponent<BoxCollider>().enabled = false;
        pageRightArrow.GetComponent<BoxCollider>().enabled = false;
        ActionController.instance.InvokeTimer(pageLeftArrow.SetActive, currentPage > 0, 1);
        if(currentPage == 0)
        {
            pageLeftArrow.transform.GetChild(0).gameObject.GetComponent<DisappearingObject>().AnimateObject(true);
        }
        else
        {
            if(previousPage == 0)
            {
                ActionController.instance.InvokeTimer(pageLeftArrow.transform.GetChild(0).gameObject.GetComponent<DisappearingObject>().AnimateObject, false, 1);
            }
            ActionController.instance.InvokeTimer(() => pageLeftArrow.GetComponent<BoxCollider>().enabled = true, 1.5f);
        }
        ActionController.instance.InvokeTimer(pageRightArrow.SetActive, currentPage < totalPages - 1, 1);
        if(currentPage == totalPages - 1)
        {
            pageRightArrow.transform.GetChild(0).gameObject.GetComponent<DisappearingObject>().AnimateObject(true);
        }
        else
        {
            if(previousPage == -1)
            {
                pageRightArrow.transform.GetChild(0).gameObject.GetComponent<DisappearingObject>().AnimateObject(false);
                ActionController.instance.InvokeTimer(() => pageRightArrow.GetComponent<BoxCollider>().enabled = true, 0.5f);
            }
            else
            {
                if(previousPage == totalPages - 1)
                {
                    ActionController.instance.InvokeTimer(pageRightArrow.transform.GetChild(0).gameObject.GetComponent<DisappearingObject>().AnimateObject, false, 1);
                }
                ActionController.instance.InvokeTimer(() => pageRightArrow.GetComponent<BoxCollider>().enabled = true, 1.5f);
            }
        }
        pageIndex.gameObject.SetActive(true);
    }

    public void OnClickNextPage()
    {
        if (currentPage < totalPages - 1)
        {
            previousPage = currentPage;
            ShowPage(currentPage + 1, false);
        }
    }

    public void OnClickPreviousPage()
    {
        if (currentPage > 0)
        {
            previousPage = currentPage;
            ShowPage(currentPage - 1, false);
        }
    }

    public void LockerAnimation(Creature target, bool isActivating)
    {
        Transform anchor = (target is Player) ? playerLockAnchor : enemyLockAnchor;
        if (isActivating)
        {
            GameObject locker;
            float alphaValue = 0f;
            Vector3 spawnPos = anchor.position;
            if (target is Player)
            {
                int cardsInPile = target.decks[0].BuyingPile.Count;
                float cardHeight = 0.03f;
                float yOffset = Mathf.Abs(((cardsInPile - 1) * cardHeight) + 0.5f);
                spawnPos = anchor.position + new Vector3(0f, yOffset, 0f);
                alphaValue = 1f;
                locker = Instantiate(LockObj, spawnPos, Quaternion.Euler(10f, 45f, 0f), anchor);
                locker.transform.localScale = locker.transform.localScale * 0.4f;
            }
            else
            {
                alphaValue = 0.2f;
                locker = Instantiate(LockObj, spawnPos, Quaternion.identity, anchor);
            }
            //GameObject locker = Instantiate(LockObj, anchor.position, Quaternion.identity, anchor);
            Quaternion lockerRot = locker.transform.localRotation;
            SpriteRenderer sr = locker.GetComponent<SpriteRenderer>();
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
            LeanTween.value(locker, 0f, alphaValue, 1f).setOnUpdate((float val) =>
            {
                Color newColor = sr.color;
                newColor.a = val;
                sr.color = newColor;
            });
            LeanTween.value(locker.transform.gameObject, -10f, 10f, 0.1f).setEaseInOutSine().setLoopPingPong(5).setOnUpdate((float angle) =>
            {
                if (target is Player)
                {
                    locker.transform.localRotation = Quaternion.Euler(10f, 45f, angle);
                }
                else
                {
                    locker.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                }
            }).setOnComplete(() => { locker.transform.localRotation = lockerRot; });
        }
        else
        {
            if(anchor.childCount == 0)
            {
                return;
            }
            Transform lockerChild = anchor.GetChild(0);
            SpriteRenderer sr = lockerChild.GetComponent<SpriteRenderer>();
            GameObject locker = lockerChild.gameObject;
            if (target is Player)
            {
                LeanTween.value(locker, 1f, 0f, 1f).setOnUpdate((float val) =>
                {
                    Color c = sr.color;
                    c.a = val;
                    sr.color = c;
                }).setOnComplete(() => { Destroy(locker); });
            }
            else
            {
                LeanTween.value(locker, 0.2f, 0f, 1f).setOnUpdate((float val) =>
                {
                    Color c = sr.color;
                    c.a = val;
                    sr.color = c;
                }).setOnComplete(() => { Destroy(locker); });
            }
        }
    }

    public void ShowTutorialPopup(string title, string body, Vector2 anchoredPos)
    {
        GameObject newTutorialPopup = Instantiate(tutorialPopup, tutorialPopupFather);
        currentTutorialPopup = newTutorialPopup.GetComponent<TutorialPopup>();
        currentTutorialPopup.SetupPopup(title, body, anchoredPos);
    }

    public void DestroyTutorialPopup()
    {
        if (currentTutorialPopup != null)
        {
            currentTutorialPopup.DestroyPopup();
        }
    }
}
