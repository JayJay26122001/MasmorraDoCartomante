using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;
//using static UnityEditor.PlayerSettings;
public class GameplayManager : MonoBehaviour
{
    public static Bell TurnArrow;
    public static Combat currentCombat;
    public static GameplayManager instance;
    public PlayableDirector timeline;
    public List<TimelineAsset> cutscenes = new List<TimelineAsset>();
    public BoardGenerator bg;
    public GameObject InputBlocker;
    public GameObject moneyBag;
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
    [HideInInspector] public int areaIndex = 0, battlePerArea = 0;

    public List<CardPack> packs = new List<CardPack>();
    public List<PackPool> packPools = new List<PackPool>();
    public bool canBuy, removingCards = false, duplicatingCards = false, fightingBoss, atShop = false;
    public int rerollPrice, rerollBasePrice, potionBasePrice;
    public TextMeshPro rerollText, potionText;
    public DisappearingObject potion;
    public Stamp stamp;

    public ParticleSystem coinExplosion;
    public List<CardAttack> attacksPool = new List<CardAttack>();
    public List<CardAttack> attacksUsed = new List<CardAttack>();
    public List<CardAttack> coinsPool = new List<CardAttack>();
    public List<CardAttack> coinsUsed = new List<CardAttack>();
    public Volume hitVol, healVol;
    public GameObject shield, fracturedShield;
    public List<DamageVFX> damageVFXPool = new List<DamageVFX>();
    public List<DamageVFX> damageVFXUsed = new List<DamageVFX>();
    public List<ParticleSystem> hitVFXPool = new List<ParticleSystem>();
    public List<ParticleSystem> hitVFXUsed = new List<ParticleSystem>();

    public List<DisappearingObject> uiObjects = new List<DisappearingObject>();
    public List<GameObject> areas = new List<GameObject>();
    public SimpleInt moneyPrize;
    public TextMeshPro hpText, shieldText, energyText, damageTakenText, enemyHpText, enemyShieldText, enemyEnergyText, enemyDamageTakenText;
    public UnityEngine.UI.Image hpCircle, enemyHpCircle;

    public CardPack starterAttack, starterDefense, starterMind;
    public CardPool starterAttackPool, starterDefensePool, starterMindPool;
    int starterPacksOpened;
    public CardPack dropPack;
    public CardPackSO enemyDropPackSO, bossDropPackSO;
    public CardPool enemyDropPool, bossDropPool;
    public int dropChance;
    [HideInInspector] public bool dropped = false;
    private void Awake()
    {
        instance = this;
        //events = EventSystem.current;
    }

    private void Start()
    {
        foreach (Envelope<EnemyPool> e in enemyPools.matrix)
        {
            if (e.value != null)
            {
                e.value.SetupProbabilities();
            }
        }
        coinExplosion.gameObject.SetActive(false);
        foreach (ParticleSystem p in hitVFXPool)
        {
            p.gameObject.SetActive(false);
        }
        SaveManager.LoadUnlockedCards();
        if(File.Exists(Application.dataPath + "/boardSave.json"))
        {
            SaveManager.LoadBoard(bg);
            SaveManager.LoadPlayer(player);
            SaveStart(true);
        }
        else
        {
            if (player.decks[0].CardPresets.Count > 0)
            {
                SaveStart(false);
            }
            else
            {
                DefineStarterPacks();
            }
        }

    }

    public void SaveStart(bool loaded)
    {
        if(!loaded)
        {
            bg.GenerateBoard2();
            ActionController.instance.InvokeTimer(bg.AnimateBoard, false, 0.05f);
        }
        starterAttack.gameObject.SetActive(false);
        starterDefense.gameObject.SetActive(false);
        starterMind.gameObject.SetActive(false);
        bg.transform.parent.gameObject.SetActive(true);
        moneyBag.SetActive(true);
        moneyBag.transform.position = new Vector3(moneyBag.transform.position.x, moneyBag.transform.position.y - 25, moneyBag.transform.position.z);
        AppearOnlyPlayerHealth();
        //bg.AnimateBoard(false);
        CameraController.instance.ActivateAngledTopCamera();
    }

    public void UpdateCreatureUI(Creature c)
    {
        if(c is Player)
        {
            if (hpText != null)
            {
                hpText.text = $"{c.Health}";
            }
            if (shieldText != null)
            {
                shieldText.text = $"{c.Shield}";
            }
            if (energyText != null)
            {
                energyText.text = $"{c.Energy}";
            }
            if (damageTakenText != null)
            {
                float multiplier = c.BaseDamageTaken / 100f;
                damageTakenText.text = $"x{multiplier.ToString("0.##")}"; //duas casa decimais, se precisar
            }
            if (hpCircle != null)
            {
                hpCircle.fillAmount = (float)c.Health / (float)c.MaxHP;
            }
        }
        else
        {
            if (enemyHpText != null)
            {
                enemyHpText.text = $"{c.Health}";
            }
            if (enemyShieldText != null)
            {
                enemyShieldText.text = $"{c.Shield}";
            }
            if (enemyEnergyText != null)
            {
                enemyEnergyText.text = $"{c.Energy}";
            }
            if (enemyDamageTakenText != null)
            {
                float multiplier = c.BaseDamageTaken / 100f;
                enemyDamageTakenText.text = $"x{multiplier.ToString("0.##")}"; //duas casa decimais, se precisar
            }
            if (enemyHpCircle != null)
            {
                enemyHpCircle.fillAmount = (float)c.Health / (float)c.MaxHP;
            }
        }
    }

    public void UpdateStatsUI(Creature c)
    {
        if(c is Player)
        {
            if (damageTakenText != null)
            {
                float multiplier = c.BaseDamageTaken / 100f;
                damageTakenText.text = $"x{multiplier.ToString("0.##")}"; //duas casa decimais, se precisar
            }
        }
        else
        {
            if (enemyDamageTakenText != null)
            {
                float multiplier = c.BaseDamageTaken / 100f;
                enemyDamageTakenText.text = $"x{multiplier.ToString("0.##")}"; //duas casa decimais, se precisar
            }
        }
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
        GameManager.instance.uiController.AdjustCommandsPositions();
        ManualPause = true;
    }
    public void ResumeInput()
    {
        //events.enabled = true;
        InputBlocker.SetActive(false);
        Camera.main.GetComponent<PlayerInput>().actions.FindActionMap("Camera").Enable();
        //Camera.main.GetComponent<PlayerInput>().actions.Enable();
        InputActive = true;
        GameManager.instance.uiController.AdjustCommandsPositions();
        ManualPause = false;
    }

    public void IPauseInput()
    {
        PauseInstances++;
        InputBlocker.SetActive(true);
        Camera.main.GetComponent<PlayerInput>().actions.FindActionMap("Camera").Disable();
        //Camera.main.GetComponent<PlayerInput>().actions.Disable();
        InputActive = false;
        GameManager.instance.uiController.AdjustCommandsPositions();
    }
    public void IResumeInput()
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
        GameManager.instance.uiController.AdjustCommandsPositions();
    }
    public void ResetPauseInstances()
    {
        PauseInstances = 0;
    }
    public void ResetManualPause()
    {
        ManualPause = false;
    }
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Q))
        {
            SkipTurnVFX(currentCombat.combatents[0]);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            SkipTurnVFX(currentCombat.combatents[1]);
        }*/
    }

    public void SelectEnemy()
    {
        if (!fightingBoss)
        {
            int aux = enemyPools.GetValue(battlePerArea, areaIndex).value.SelectIndex();
            if (battlePerArea < enemyPools.XLength - 1)
            {
                enemyPools.GetValue(battlePerArea + 1, areaIndex).value.ModifyMultipliers(enemyPools.GetValue(battlePerArea, areaIndex).value.baseProbabilities[enemyPools.GetValue(battlePerArea, areaIndex).value.CheckIndexAt(aux)].type);
            }
            else
            {
                enemyPools.GetValue(battlePerArea, areaIndex).value.ModifyMultipliers(enemyPools.GetValue(battlePerArea, areaIndex).value.baseProbabilities[enemyPools.GetValue(battlePerArea, areaIndex).value.CheckIndexAt(aux)].type);
            }
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
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].gameObject.SetActive(i == index);
        }
    }
    public void ShowBoss()
    {
        for (int i = 0; i < bosses.Count; i++)
        {
            bosses[i].gameObject.SetActive(i == areaIndex);
        }
    }
    public void HideAllEnemies()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].gameObject.SetActive(false);
        }
    }
    public void HideAllBosses()
    {
        for (int i = 0; i < bosses.Count; i++)
        {
            bosses[i].gameObject.SetActive(false);
        }
    }

    public void PlayCutscene(int index)
    {
        if (index == 3 && !canBuy)
        {
            return;
        }
        timeline.playableAsset = cutscenes[index];
        timeline.Play();
        PauseInput((float)cutscenes[index].duration);
    }

    /*public void MoveBoard(Action act)
    {
        LeanTween.move(bg.gameObject, bg.gameObject.transform.position - (Vector3.forward * 20 * bg.gameObject.transform.localScale.z), 1).setOnComplete(act);
    }*/
    public void MoveBoard()
    {
        bg.MovementChange(true);
        LeanTween.move(bg.gameObject, bg.gameObject.transform.position - (Vector3.forward * 20 * bg.gameObject.transform.localScale.z), 0.75f);
        LeanTween.move(bg.playerPiece, bg.playerPiece.transform.position - (Vector3.forward * 20 * bg.gameObject.transform.localScale.z), 0.75f).setOnComplete(() => 
        { 
            bg.MovementChange(false);
            SaveManager.SavePlayer();
            SaveManager.SaveUnlockedCards();
        });
    }
    public void MovePiece(Action act, Vector3 pos)
    {
        bg.pieceAux.GetComponent<Animation>().Play();
        LeanTween.moveLocal(bg.playerPiece, pos, 0.75f).setOnComplete(act);
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
        atShop = true;
        potion.mat.SetFloat("_DisappearTime", 0);
        potion.gameObject.transform.parent.GetComponent<BoxCollider>().enabled = true;
        potionText.gameObject.SetActive(true);
        potionText.text = "$" + potionBasePrice;
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
            packs[i].gameObject.GetComponent<BoxCollider>().enabled = true;
            packs[i].DefineCards();
        }
        canBuy = true;
    }

    public void DefineStarterPacks()
    {
        starterPacksOpened = 0;
        starterAttackPool.pool = GameManager.instance.DefineStarterPool(Card.CardType.Attack);
        starterAttack.data.possibleCards = starterAttackPool;
        starterAttack.gameObject.GetComponent<BoxCollider>().enabled = true;
        starterAttack.DefineCards();
        starterDefensePool.pool = GameManager.instance.DefineStarterPool(Card.CardType.Defense);
        starterDefense.data.possibleCards = starterDefensePool;
        starterDefense.gameObject.GetComponent<BoxCollider>().enabled = true;
        starterDefense.DefineCards();
        starterMindPool.pool = GameManager.instance.DefineStarterPool(Card.CardType.Mind);
        starterMind.data.possibleCards = starterMindPool;
        starterMind.gameObject.GetComponent<BoxCollider>().enabled = true;
        starterMind.DefineCards();
        canBuy = true;
    }

    public void DefineDropPack()
    {
        List<Card> aux = new List<Card>();
        foreach (Card c in player.enemy.decks[0].CardPresets)
        {
            if (!aux.Contains(c))
            {
                aux.Add(c);
            }
        }
        if (!fightingBoss)
        {
            enemyDropPool.pool = aux;
            enemyDropPackSO.possibleCards = enemyDropPool;
            dropPack.data = enemyDropPackSO;
        }
        else
        {
            bossDropPool.pool = aux;
            bossDropPackSO.possibleCards = bossDropPool;
            dropPack.data = bossDropPackSO;
        }
        dropPack.DefineCards();
        dropPack.gameObject.GetComponent<BoxCollider>().enabled = true;
        dropped = true;
        canBuy = true;
    }

    public void OpenedStarterPack()
    {
        starterPacksOpened++;
        if(starterPacksOpened == 3)
        {
            PlayCutscene(14);
        }
    }

    public void OpenedDropPack()
    {
        dropped = false;
        if(!fightingBoss)
        {
            PlayCutscene(16);
        }
        else
        {
            PlayCutscene(17);
        }
    }

    public void DiscardBell(DiscardBell bell)
    {
        if (bell.pack != null)
        {
            DiscardBoughtCards(bell);
        }
        else
        {
            if(removingCards)
            {
                if(atShop)
                {
                    player.ChangeMoney(3);
                }
                DestroyRemovingCards();
            }
            else if(duplicatingCards)
            {
                CameraController.instance.DeActivateZoomedCamera();
                CameraController.instance.ChangeCamera(0);
                DestroyDuplicatingCards();
            }
        }
    }
    public void DiscardBoughtCards(DiscardBell bell)
    {
        bell.pack.DestroyBoughtCards(null);
    }

    public void RemovingCards(GameObject go)
    {
        if (canBuy)
        {
            if (player.ChangeMoney(-3))
            {
                ExplodeCoins(go.transform.position);
                canBuy = false;
                removingCards = true;
                PlayCutscene(4);
                List<CardDisplay> cds = new List<CardDisplay>();
                foreach (Card c in player.decks[0].cards)
                {
                    CardDisplay cd = CardUIController.instance.InstantiateCard(c);
                    cds.Add(cd);
                    cd.UpdateCard();
                    CardUIController.OrganizeAllDeckCards(cds);
                }
            }
            else
            {
                Debug.Log("You don't have enough money.");
            }
        }
    }

    public void RemovingCards()
    {
        removingCards = true;
        PlayCutscene(4);
        List<CardDisplay> cds = new List<CardDisplay>();
        foreach (Card c in player.decks[0].cards)
        {
            CardDisplay cd = CardUIController.instance.InstantiateCard(c);
            cds.Add(cd);
            cd.UpdateCard();
            CardUIController.OrganizeAllDeckCards(cds);
        }
    }

    public void DuplicatingCards()
    {
        duplicatingCards = true;
        PlayCutscene(4);
        List<CardDisplay> cds = new List<CardDisplay>();
        foreach (Card c in player.decks[0].cards)
        {
            CardDisplay cd = CardUIController.instance.InstantiateCard(c);
            cds.Add(cd);
            cd.UpdateCard();
            CardUIController.OrganizeAllDeckCards(cds);
        }
    }

    public void DestroyRemovingCards()
    {
        if(atShop)
        {
            PlayCutscene(5);
            canBuy = true;
        }
        else
        {
            PlayCutscene(11);
        }
        foreach (Transform t in player.combatSpace.playedCardSpace.transform)
        {
            var moveTween = LeanTween.move(t.gameObject, t.position + Vector3.up * 25, 0.05f);
            moveTween.setOnComplete(() =>
            {
                Destroy(t.gameObject);
            });
        }
        removingCards = false;
    }

    public void DestroyDuplicatingCards()
    {
        PlayCutscene(13);
        foreach (Transform t in player.combatSpace.playedCardSpace.transform)
        {
            var moveTween = LeanTween.move(t.gameObject, t.position + Vector3.up * 25, 0.05f);
            moveTween.setOnComplete(() =>
            {
                Destroy(t.gameObject);
            });
        }
        LeanTween.move(stamp.emptyCard, stamp.emptyCard.transform.position + Vector3.up * 25, 0.05f).setOnComplete(() =>
        {
            stamp.emptyCard.SetActive(false);
        });
        duplicatingCards = false;
    }
    public void ExitShop()
    {
        atShop = false;
    }

    public void RerollPacks(GameObject go)
    {
        if (canBuy)
        {
            if (player.ChangeMoney(-rerollPrice))
            {
                if (potion.mat.GetFloat("_DisappearTime") >= 1)
                {
                    potion.AnimateObject(false);
                }
                potion.transform.parent.gameObject.GetComponent<BoxCollider>().enabled = true;
                ExplodeCoins(go.transform.position);
                bool disappear = false;
                canBuy = false;
                rerollPrice++;
                rerollText.text = "$" + rerollPrice;
                foreach (CardPack pack in packs)
                {
                    if (!pack.bought)
                    {
                        pack.AnimatePack(true);
                        disappear = true;
                    }
                    pack.GetComponent<BoxCollider>().enabled = true;
                }
                StartCoroutine(RerollAnimations(disappear));
            }
        }
    }

    IEnumerator RerollAnimations(bool disappear)
    {
        if (disappear)
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
        if (disappear)
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

    //SONS
    public void StartMapMusic()
    {
        AudioController.instance.PlayMapMusic();
    }

    public void StartShopMusic()
    {
        AudioController.instance.PlayShopMusic();
    }
    float volumeTimeStart, vfxDuration = 0.5f;
    IEnumerator BlendVolumeIn(Volume v)
    {
        v.weight = Mathf.Clamp((Time.time - volumeTimeStart) / (vfxDuration / 2), 0, 1);
        if (v.weight < 1)
        {
            yield return new WaitForSeconds(0.001f);
            StartCoroutine(BlendVolumeIn(v));
        }
        else
        {
            volumeTimeStart = Time.time;
            StartCoroutine(BlendVolumeOut(v));
        }
    }
    IEnumerator BlendVolumeOut(Volume v)
    {
        v.weight = Mathf.Clamp(1 - ((Time.time - volumeTimeStart) / (vfxDuration / 2)), 0, 1);
        if (v.weight > 0)
        {
            yield return new WaitForSeconds(0.001f);
            StartCoroutine(BlendVolumeOut(v));
        }
    }
    
    //VFX
    public void ExplodeCoins(Vector3 pos)
    {
        coinExplosion.gameObject.transform.position = pos;
        coinExplosion.gameObject.SetActive(true);
        coinExplosion.Play();
    }

    public UnityEvent ActivateCardAttack(Vector3 pos, Creature target)
    {
        for (int i = 0; i < attacksPool.Count; i++)
        {
            if (!attacksUsed.Contains(attacksPool[i]))
            {
                UnityEvent temp = attacksPool[i].HitTarget;
                attacksPool[i].transform.position = pos;
                attacksPool[i].SetTarget(target.transform);
                attacksPool[i].BezierCurve();
                attacksUsed.Add(attacksPool[i]);
                i = attacksPool.Count;
                return temp;
            }
        }
        return null;
    }

    public UnityEvent ActivateCoin(Vector3 pos)
    {
        for (int i = 0; i < coinsPool.Count; i++)
        {
            if (!coinsUsed.Contains(coinsPool[i]))
            {
                UnityEvent temp = coinsPool[i].HitTarget;
                coinsPool[i].transform.position = pos;
                coinsPool[i].BezierCurve();
                coinsUsed.Add(coinsPool[i]);
                i = coinsPool.Count;
                return temp;
            }
        }
        return null;
    }

    public void HealVFX()
    {
        volumeTimeStart = Time.time;
        StartCoroutine(BlendVolumeIn(healVol));
    }
    public void HurtVFX()
    {
        volumeTimeStart = Time.time;
        StartCoroutine(BlendVolumeIn(hitVol));
    }


    public void EnemyShieldVFX()
    {
        Vector3 auxPos = player.enemy.GetComponent<Enemy>().model.transform.position;
        shield.transform.position = new Vector3(auxPos.x, 0, auxPos.z - player.enemy.GetComponent<Enemy>().GetComponent<CapsuleCollider>().radius - 10);
        shield.SetActive(true);
        Invoke("HideShield", 0.5f);
    }
    void HideShield()
    {
        shield.SetActive(false);
    }
    public void EnemyFracturedShieldVFX()
    {
        /*Vector3 auxPos = player.enemy.GetComponent<Enemy>().model.transform.position;
        GameObject fShield = Instantiate(fracturedShield, new Vector3(auxPos.x, 0, auxPos.z - player.enemy.GetComponent<Enemy>().model.GetComponent<CapsuleCollider>().radius - 10), Quaternion.Euler(0, 180, 0));
        Destroy(fShield, 3f);*/
        Vector3 auxPos = player.enemy.GetComponent<Enemy>().model.transform.position;
        fracturedShield.transform.position = new Vector3(auxPos.x, 0, auxPos.z - player.enemy.GetComponent<Enemy>().GetComponent<CapsuleCollider>().radius - 10);
        for(int i = 0; i < fracturedShield.transform.childCount; i++)
        {
            fracturedShield.transform.GetChild(i).gameObject.GetComponent<ShieldPiece>().ResetTransform();
            fracturedShield.transform.GetChild(i).gameObject.GetComponent<ShieldPiece>().Force();
        }
        fracturedShield.SetActive(true);
        Invoke("HideFracturedShield", 3f);
    }
    void HideFracturedShield()
    {
        for (int i = 0; i < fracturedShield.transform.childCount; i++)
        {
            fracturedShield.transform.GetChild(i).gameObject.GetComponent<ShieldPiece>().DisableGravity();
        }
        fracturedShield.SetActive(false);
    }
    public void EnemyHitVFX()
    {
        for (int i = 0; i < hitVFXPool.Count; i++)
        {
            if (!hitVFXUsed.Contains(hitVFXPool[i]))
            {
                Vector3 auxPos = player.enemy.GetComponent<Enemy>().model.transform.position;
                hitVFXPool[i].transform.position = new Vector3(auxPos.x, 5, auxPos.z - player.enemy.GetComponent<Enemy>().GetComponent<CapsuleCollider>().radius);
                hitVFXPool[i].gameObject.SetActive(true);
                hitVFXPool[i].Play();
                hitVFXUsed.Add(hitVFXPool[i]);
                StartCoroutine(HitVFXStop(hitVFXPool[i]));
                i = attacksPool.Count;
            }
        }
    }

    IEnumerator HitVFXStop(ParticleSystem p)
    {
        yield return new WaitForSeconds(0.5f);
        GameplayManager.instance.hitVFXUsed.Remove(p);
    }
    public void DamageNumber(int damage)
    {
        for (int i = 0; i < damageVFXPool.Count; i++)
        {
            if (!damageVFXUsed.Contains(damageVFXPool[i]))
            {
                damageVFXPool[i].SetText("-" + damage, DamageVFX.VFXType.Damage);
                damageVFXUsed.Add(damageVFXPool[i]);
                i = damageVFXPool.Count;
            }
        }
    }
    public void SpawnVFX(string text,DamageVFX.VFXType type)
    {
        for (int i = 0; i < damageVFXPool.Count; i++)
        {
            if (!damageVFXUsed.Contains(damageVFXPool[i]))
            {
                damageVFXPool[i].SetText(text, type);
                damageVFXUsed.Add(damageVFXPool[i]);
                i = damageVFXPool.Count;
            }
        }
        //currentCombat.CombatUI();
        //ActionController.instance.InvokeTimer(currentCombat.CombatUI, 0.75f);
    }
    public void HealthModifiedVFX(Creature target, int Amount)
    {
        DamageVFX.VFXType Type;
        if(target is Player)
        {
            Type = DamageVFX.VFXType.PlayerHP;
        }
        else if (target is Enemy)
        {
            Type = DamageVFX.VFXType.EnemyHP;
        }
        else return;
        string sign;
        if (Amount < 0)
        {
            sign = "";
        }
        else if (Amount > 0)
        {
            sign = "+";
        }
        else return;
        SpawnVFX(sign + Amount, Type);
    }
    public void ShieldModifiedVFX(Creature target, int Amount)
    {
        DamageVFX.VFXType Type;
        if (target is Player)
        {
            Type = DamageVFX.VFXType.PlayerShield;
        }
        else if (target is Enemy)
        {
            Type = DamageVFX.VFXType.EnemyShield;
        }
        else return;
        string sign;
        if (Amount < 0)
        {
            sign = "";
        }
        else if (Amount > 0)
        {
            sign = "+";
        }
        else return;
        SpawnVFX(sign + Amount, Type);
    }
    public void EnergyModifiedVFX(Creature target, int Amount)
    {
        DamageVFX.VFXType Type;
        if (target is Player)
        {
            Type = DamageVFX.VFXType.PlayerEnergy;
        }
        else if (target is Enemy)
        {
            Type = DamageVFX.VFXType.EnemyEnergy;
        }
        else return;
        string sign;
        if (Amount < 0)
        {
            sign = "";
        }
        else if (Amount > 0)
        {
            sign = "+";
        }
        else return;
        SpawnVFX(sign + Amount, Type);
    }
    public void SkipTurnVFX(Creature target)
    {
        string s;
        if (target is Player)
        {
            s = "You lost your next turn";
        }
        else if (target is Enemy e)
        {
            s = $"{e.name} lost the next turn";
        }
        else
        {
            return;
        }
        for (int i = 0; i < damageVFXPool.Count; i++)
        {
            if (!damageVFXUsed.Contains(damageVFXPool[i]))
            {
                damageVFXPool[i].SetText(s, DamageVFX.VFXType.Other, Color.magenta, true, Vector3.up, quaternion.identity, 5);
                damageVFXUsed.Add(damageVFXPool[i]);
                i = damageVFXPool.Count;
            }
        }
    }
    public void BlockDrawnVFX(Creature target)
    {
        string s;
        if (target is Player)
        {
            s = "You cant draw next turn";
        }
        else if (target is Enemy e)
        {
            s = $"{e.name} cant draw next turn";
        }
        else
        {
            return;
        }
        for (int i = 0; i < damageVFXPool.Count; i++)
        {
            if (!damageVFXUsed.Contains(damageVFXPool[i]))
            {
                damageVFXPool[i].SetText(s, DamageVFX.VFXType.Other, Color.magenta, true, Vector3.up, quaternion.identity, 5);
                damageVFXUsed.Add(damageVFXPool[i]);
                i = damageVFXPool.Count;
            }
        }
    }
    
    public void PrizeMoney()
    {
        player.ChangeMoney(moneyPrize.GetValue() + areaIndex);
    }

    public void DisappearUI(bool includePlayerHP)
    {
        int i = 0;
        if (!includePlayerHP)
        {
            i++;
        }
        for(; i < uiObjects.Count; i++)
        {
            uiObjects[i].AnimateObject(true);
        }
    }

    public void AppearUI(bool includePlayerHP)
    {
        int i = 0;
        if (!includePlayerHP)
        {
            i++;
        }
        for(; i < uiObjects.Count; i++)
        {
            uiObjects[i].AnimateObject(false);
        }
    }

    public void AppearOnlyPlayerHealth()
    {
        uiObjects[0].AnimateObject(false);
    }

    public void DisappearOnlyPlayerHealth()
    {
        uiObjects[0].AnimateObject(true);
    }

    public void SwitchArea()
    {
        for(int i = 0; i < areas.Count; i++)
        {
            if(i != areaIndex)
            {
                areas[i].SetActive(false);
            }
            else
            {
                areas[i].SetActive(true);
            }
        }
    }

    public void SceneFadeOut()
    {
        SceneFadeController.instance.FadeOutToArea();
    }
}
