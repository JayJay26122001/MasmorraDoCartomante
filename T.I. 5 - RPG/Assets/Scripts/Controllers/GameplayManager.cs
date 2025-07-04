using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;
using TMPro;
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
    [System.NonSerialized] public bool CombatActive = false;

    [HideInInspector] public BoardRoom currentRoom;

    //[SerializeField]int money;

    public List<Enemy> enemies = new List<Enemy>();
    public List<Enemy> bosses = new List<Enemy>();
    public SerializedMatrix<EnemyPool> enemyPools = new SerializedMatrix<EnemyPool>(5, 5);
    public Player player;
    int areaIndex = 0, battlePerArea = 0;

    public List<CardPack> packs = new List<CardPack>();
    public List<PackPool> packPools = new List<PackPool>();
    public bool canBuy, removingCards = false, figtingBoss;
    public int rerollPrice, rerollBasePrice;
    public TextMeshPro rerollText;
    private void Awake()
    {
        instance = this;
        //events = EventSystem.current;
    }
    public void PauseInput(float time)
    {
        IPauseInput();
        ActionController.instance.InvokeTimer(IResumeInput, time);
    }

    public void PauseInput()
    {
        //events.enabled = false;
        InputBlocker.SetActive(true);
        Camera.main.GetComponent<PlayerInput>().actions.FindActionMap("Camera").Disable();
        //Camera.main.GetComponent<PlayerInput>().actions.Disable();
        InputActive = false;
        ManualPause = true;
    }
    public void ResumeInput()
    {
        //events.enabled = true;
        InputBlocker.SetActive(false);
        Camera.main.GetComponent<PlayerInput>().actions.FindActionMap("Camera").Enable();
        //Camera.main.GetComponent<PlayerInput>().actions.Enable();
        InputActive = true;
        ManualPause = false;
    }

    void IPauseInput()
    {
        PauseInstances++;
        InputBlocker.SetActive(true);
        Camera.main.GetComponent<PlayerInput>().actions.FindActionMap("Camera").Disable();
        //Camera.main.GetComponent<PlayerInput>().actions.Disable();
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
        Camera.main.GetComponent<PlayerInput>().actions.FindActionMap("Camera").Enable();
        //Camera.main.GetComponent<PlayerInput>().actions.Enable();
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
        if(!figtingBoss)
        {
            int aux = enemyPools.GetValue(battlePerArea, areaIndex).value.SelectIndex();
            ShowEnemy(aux);
            AudioController.instance.PlayCombatMusic();
            currentCombat.SetEnemy(aux);
        }
        else
        {
            ShowBoss();
            AudioController.instance.PlayBossMusic();
            currentCombat.SetEnemy(areaIndex);
        }
    }

    public void ShowEnemy(int index)
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].gameObject.SetActive(i == index);
        }
    }
    public void ShowBoss()
    {
        for(int i = 0; i < bosses.Count; i++)
        {
            bosses[i].gameObject.SetActive(i == areaIndex);
        }
    }
    public void HideAllEnemies()
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].gameObject.SetActive(false);
        }
    }
    public void HideAllBosses()
    {
        for(int i = 0; i < bosses.Count; i++)
        {
            bosses[i].gameObject.SetActive(false);
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
        rerollPrice = rerollBasePrice;
        rerollText.text = "$" + rerollPrice;
        DefinePacks();
    }

    public void DefinePacks()
    {
        List<CardPackSO> aux = packPools[areaIndex].SelectPacks(packs.Count);
        for (int i = 0; i < packs.Count; i++)
        {
            packs[i].data = aux[i];
            packs[i].DefineCards();
        }
        canBuy = true;
    }

    public void DiscardBell(DiscardBell bell)
    {
        if(bell.pack != null)
        {
            DiscardBoughtCards(bell);
        }
        else
        {
            player.ChangeMoney(3);
            DestroyRemovingCards();
        }
    }
    public void DiscardBoughtCards(DiscardBell bell)
    {
        bell.pack.DestroyBoughtCards();
    }

    public void RemovingCards()
    {
        if(canBuy)
        {
            if(player.ChangeMoney(-3))
            {
                canBuy = false;
                removingCards = true;
                PlayCutscene(4);
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
        PlayCutscene(5);
        foreach (Transform t in player.combatSpace.playedCardSpace.transform)
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

    public void RerollPacks()
    {
        if(canBuy)
        {
            if(player.ChangeMoney(-rerollPrice))
            {
                bool disappear = false;
                canBuy = false;
                rerollPrice++;
                rerollText.text = "$" + rerollPrice;
                foreach(CardPack pack in packs)
                {
                    if(!pack.bought)
                    {
                        pack.AnimatePack(true);
                        disappear = true;
                    }
                }
                StartCoroutine(RerollAnimations(disappear));
            }
        }
    }

    IEnumerator RerollAnimations(bool disappear)
    {
        if(disappear)
        {
            yield return new WaitUntil(() => CheckPacks(true));
        }
        yield return new WaitForSeconds(0.5f);
        DefinePacks();
        foreach (CardPack pack in packs)
        {
            pack.AnimatePack(false);
        }
        yield return new WaitUntil(() => CheckPacks(false));
        canBuy = true;
    }

    bool CheckPacks(bool disappear)
    {
        bool aux = true;
        if(disappear)
        {
            foreach (CardPack pack in packs)
            {
                if (pack.mat.GetFloat("_DisappearTime") < 1)
                {
                    aux = false;
                }
            }
        }
        else
        {
            foreach (CardPack pack in packs)
            {
                if (pack.mat.GetFloat("_DisappearTime") > 0)
                {
                    aux = false;
                }
            }
        }
        return aux;
    }

    public void StartMapMusic()
    {
        AudioController.instance.PlayMapMusic();
    }

    public void StartShopMusic()
    {
        AudioController.instance.PlayShopMusic();
    }
}
