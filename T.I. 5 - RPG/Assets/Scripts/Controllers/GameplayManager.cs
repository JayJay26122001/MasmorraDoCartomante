using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;
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
    public SerializedMatrix<EnemyPool> enemyPools = new SerializedMatrix<EnemyPool>(5, 5);
    public Player player;
    int areaIndex = 0, battlePerArea = 0;

    public List<CardPack> packs = new List<CardPack>();
    public List<PackPool> packPools = new List<PackPool>();
    public bool canBuy, removingCards = false;

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

    public void SelectEnemy()
    {
        int aux = enemyPools.GetValue(battlePerArea, areaIndex).value.SelectIndex();
        ShowEnemy(aux);
        currentCombat.SetEnemy(aux);
    }

    public void ShowEnemy(int index)
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].gameObject.SetActive(i == index);
        }
    }
    public void HideAllEnemies()
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].gameObject.SetActive(false);
        }
    }

    public void PlayCutscene(int index)
    {
        if(index == 3 && !canBuy)
        {
            return;
        }
        timeline.playableAsset = cutscenes[index];
        timeline.Play();
        PauseInput((float)cutscenes[index].duration);
    }

    public void MoveBoard(Action act)
    {
        LeanTween.move(bg.gameObject, bg.gameObject.transform.position - (Vector3.forward * 20 * bg.gameObject.transform.localScale.z), 1).setOnComplete(act);
    }

    public void ChangeArea()
    {
        areaIndex = Mathf.Clamp(areaIndex + 1, 0, enemyPools.YLength - 1);
        battlePerArea = 0;
    }
    public void ChangeBattleCount()
    {
        battlePerArea = Mathf.Clamp(battlePerArea + 1, 0, enemyPools.XLength - 1);
    }

    public void DefineShop()
    {
        List<CardPackSO> aux = packPools[areaIndex].SelectPacks(packs.Count);
        for(int i = 0; i < packs.Count; i++)
        {
            packs[i].data = aux[i];
            packs[i].DefineCards();
        }
        canBuy = true;
    }

    public void DiscardBoughtCards(DiscardBell bell)
    {
        if(bell.pack != null)
        {
            bell.pack.DestroyBoughtCards();
        }
    }

    public void RemovingCards()
    {
        if(canBuy)
        {
            if(player.ChangeMoney(-3))
            {
                canBuy = false;
                removingCards = true;
                List<CardDisplay> cds = new List<CardDisplay>();
                foreach (Card c in player.decks[0].cards)
                {
                    CardDisplay cd = CardUIController.instance.InstantiateCard(c);
                    cds.Add(cd);
                    CardUIController.OrganizeRemovingCards(cds);
                }
            }
            else
            {
                Debug.Log("You don't have enough money.");
            }
        }
    }

    public void DestroyRemovingCards()
    {
        foreach(Transform t in player.combatSpace.playedCardSpace.transform)
        {
            var moveTween = LeanTween.move(t.gameObject, t.position + Vector3.up * 25, 0.05f);
            moveTween.setOnComplete(() =>
            {
                Destroy(t.gameObject);
            });
        }
        removingCards = false;
        canBuy = true;
    }
}
