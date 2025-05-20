using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;
public class GameplayManager : MonoBehaviour
{
    public static Combat currentCombat;
    public static GameplayManager instance;
    public PlayableDirector timeline;
    public List<TimelineAsset> cutscenes = new List<TimelineAsset>();
    public BoardGenerator bg;
    public GameObject InputBlocker;
    int PauseInstances = 0;
    public bool InputActive { get; private set; } = true;
    bool ManualPause = false;

    [HideInInspector]public BoardRoom currentRoom;

    //[SerializeField]int money;

    public List<Enemy> enemies = new List<Enemy>();
    public Player player;

    private void Awake()
    {
        instance = this;
        //events = EventSystem.current;
    }
    public void PauseInput(float time)
    {
        IPauseInput();
        SceneAnimationController.instance.InvokeTimer(IResumeInput, time);
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

    public void ShowEnemy(int index)
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].gameObject.SetActive(i == index);
        }
    }

    public void PlayCutscene(int index)
    {
        timeline.playableAsset = cutscenes[index];
        timeline.Play();
        PauseInput((float)cutscenes[index].duration);
    }

    public void MoveBoard(Action act)
    {
        LeanTween.move(bg.gameObject, bg.gameObject.transform.position - (Vector3.forward * 20 * bg.gameObject.transform.localScale.z), 2).setOnComplete(act);
    }
}
