using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [HideInInspector] public int areaIndex = 0, battlePerArea = 0;

    public List<CardPack> packs = new List<CardPack>();
    public List<PackPool> packPools = new List<PackPool>();
    public bool canBuy, removingCards = false, figtingBoss, atShop = false;
    public int rerollPrice, rerollBasePrice, potionBasePrice;
    public TextMeshPro rerollText, potionText;
    public DisappearingObject potion;

    public ParticleSystem coinExplosion;
    public List<CardAttack> attacksPool = new List<CardAttack>();
    public List<CardAttack> attacksUsed = new List<CardAttack>();
    public Volume hitVol, healVol;
    public GameObject shield, fracturedShield;
    public List<DamageVFX> damageVFXPool = new List<DamageVFX>();
    public List<DamageVFX> damageVFXUsed = new List<DamageVFX>();
    public List<ParticleSystem> hitVFXPool = new List<ParticleSystem>();
    public List<ParticleSystem> hitVFXUsed = new List<ParticleSystem>();

    public List<DisappearingObject> uiObjects = new List<DisappearingObject>();
    public List<GameObject> areas = new List<GameObject>();
    public SimpleInt moneyPrize;
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
        if (!figtingBoss)
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
        LeanTween.move(bg.playerPiece, bg.playerPiece.transform.position - (Vector3.forward * 20 * bg.gameObject.transform.localScale.z), 0.75f).setOnComplete(() => { bg.MovementChange(false); });
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
        potion.gameObject.GetComponent<BoxCollider>().enabled = true;
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
            packs[i].DefineCards();
        }
        canBuy = true;
    }

    public void DiscardBell(DiscardBell bell)
    {
        if (bell.pack != null)
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
                    CardUIController.OrganizeRemovingCards(cds);
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
            CardUIController.OrganizeRemovingCards(cds);
        }
    }

    public void DestroyRemovingCards()
    {
        if(atShop)
        {
            PlayCutscene(5);
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
        canBuy = true;
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

    public void StartMapMusic()
    {
        AudioController.instance.PlayMapMusic();
    }

    public void StartShopMusic()
    {
        AudioController.instance.PlayShopMusic();
    }

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

    float volumeTimeStart, vfxDuration = 0.5f;
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
                damageVFXPool[i].SetDamage(damage);
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
