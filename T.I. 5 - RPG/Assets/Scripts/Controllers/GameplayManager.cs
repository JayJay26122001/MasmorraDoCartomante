using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameplayManager : MonoBehaviour
{
    public static Combat currentCombat;
    public static GameplayManager instance;
    public GameObject InputBlocker;
    int PauseInstances = 0;
    public bool InputActive { get; private set; } = true;
    bool ManualPause = false;


    private void Awake()
    {
        instance = this;
        //events = EventSystem.current;
    }
    public void PauseInput(float time)
    {
        IPauseInput();
        SceneAnimationController.AnimController.InvokeTimer(IResumeInput, time);
    }

    public void PauseInput()
    {
        //events.enabled = false;
        InputBlocker.SetActive(true);
        Camera.main.GetComponent<PlayerInput>().actions.Disable();
        InputActive = false;
        ManualPause = true;
    }
    public void ResumeInput()
    {
        //events.enabled = true;
        InputBlocker.SetActive(false);
        Camera.main.GetComponent<PlayerInput>().actions.Enable();
        InputActive = true;
        ManualPause = false;
    }
    void IPauseInput()
    {
        PauseInstances++;
        InputBlocker.SetActive(true);
        Camera.main.GetComponent<PlayerInput>().actions.Disable();
        InputActive = false;
    }
    void IResumeInput()
    {
        PauseInstances--;
        if (PauseInstances > 0)
        {
            return;
        }
        else
        {
            PauseInstances = 0;
            if (ManualPause)
            {
                return;
            }
        }
        InputBlocker.SetActive(false);
        Camera.main.GetComponent<PlayerInput>().actions.Enable();
        InputActive = true;
    }
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Q))
        {
            if (InputActive)
            {
                PauseInput();
            }
            else
            {
                ResumeInput();
            }
        }*/
    }
}
