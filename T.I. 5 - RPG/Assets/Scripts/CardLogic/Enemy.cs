using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
public class Enemy : Creature, IPointerClickHandler
{
    public Animator anim;
    public GameObject model;
    public enum EnemySize { Small, Medium, Large };
    public EnemySize size;
    public UnityEvent FinishedPlaying;
    Interactable interactable;
    public ControlUI zoomIn, zoomOut;
    protected override void Awake()
    {
        base.Awake();
        Wounded.AddListener((DealDamage d) => GameplayManager.instance.EnemyHitVFX());
        DamageBlocked.AddListener((DealDamage d) => GameplayManager.instance.EnemyShieldVFX());
        ShieldBreak.AddListener(GameplayManager.instance.EnemyFracturedShieldVFX);
        interactable = this.gameObject.GetComponent<Interactable>();
    }
    public override void CombatStartAction()
    {
        base.CombatStartAction();
        //BuyCards(1);
        SetModel();
        float time = 0;
        foreach (AnimationClip a in anim.runtimeAnimatorController.animationClips)
        {
            if (a.name == "Intro")
            {
                time = a.length;
            }
        }
        CameraController.instance.ChangeCamera(1);
        ActionController.instance.InvokeTimer(CameraController.instance.ChangeCamera, 0, time);
    }

    public void ChangeInteraction(bool zoom)
    {
        interactable.HideInteractions();
        interactable.interactions.Clear();
        if(zoom)
        {
            interactable.interactions.Add(zoomIn);
        }
        else
        {
            interactable.interactions.Add(zoomOut);
        }
    }
    public override void TurnAction()
    {
        if (!GameplayManager.instance.CombatActive) return;
        base.TurnAction();
        if (skipTurn > 0)
        {
            skipTurn--;
            FinishedPlaying.Invoke();
            GameplayManager.TurnArrow.NextTurn();
            return;
        }
        StartCoroutine(PlayAllCardsBehaviour());
        /*bool playanim = true;
        for (int i = 0; i < hand.Count; i++)
        {
            EnemyPlayCard anim = new EnemyPlayCard(this, hand[i], playanim);
            ActionController.instance.AddToQueue(anim);
            playanim = false;
        }*/

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            GameManager.instance.uiController.ShowEnemyDescription();
        }
    }

    protected virtual IEnumerator PlayAllCardsBehaviour()
    {
        yield return new WaitUntil(() => ActionController.instance.NumberOfActionsInQueue() <= 0);
        bool playanim = true;
        for (int i = 0; i < hand.Count;)
        {
            if (hand[i].cost <= energy)
            {
                Card played = hand[i];
                EnemyPlayCard anim = new EnemyPlayCard(this, played, playanim);
                ActionController.instance.AddToQueue(anim);
                playanim = false;
                //yield return new WaitUntil(() => !hand.Contains(played));
                bool CardPlayed = false;
                UnityAction<Card> check = (Card c) => CardPlayed = true;
                PlayedCard.AddListener(check);
                yield return new WaitUntil(() => CardPlayed);
                PlayedCard.RemoveListener(check);

                yield return new WaitForSeconds(0.8f);
                i = 0;
            }
            else
            {
                i++;
            }

        }
        yield return new WaitForSeconds(1f);
        FinishedPlaying.Invoke();
    }

    /*void TurnActionsDelayed()
    {
        if (!GameplayManager.instance.CombatActive) return;
        PlayCard(hand[0]);
    }*/
    public override void TakeDamage(DealDamage dmg)
    {
        if (Health > 0)
        {
            int damage = dmg.GetDamage();
            bool IgnoreDefense = dmg.IgnoreDefense;
            //damage -= (int)(BaseDamageTaken/100 * damage);
            damage = (int)(damage * (BaseDamageTaken / 100));
            if (damage <= 0) { return; }
            int trueDamage;
            if (IgnoreDefense)
            {
                trueDamage = damage;
                //GameplayManager.instance.EnemyHitVFX();
                Wounded.Invoke(dmg);
            }
            else
            {
                trueDamage = (int)Mathf.Clamp(damage - Shield, 0, Mathf.Infinity);
                int OGshield = Shield;
                Shield -= damage;
                GameplayManager.instance.ShieldModifiedVFX(this, -Mathf.Clamp(damage, 0, OGshield));
                if (OGshield > 0 && Shield == 0)
                {
                    //GameplayManager.instance.EnemyFracturedShieldVFX();
                    ShieldBreak.Invoke();
                }
                if (trueDamage == 0)
                {
                    //GameplayManager.instance.EnemyShieldVFX();
                    DamageBlocked.Invoke(dmg);
                }
                else
                {
                    //GameplayManager.instance.EnemyHitVFX();
                    Wounded.Invoke(dmg);
                }
            }
            Health -= trueDamage;

            GameplayManager.instance.DamageNumber(damage);
            GameplayManager.instance.HealthModifiedVFX(this, -trueDamage);
            Damaged.Invoke(dmg);
            if (Health <= 0)
            {
                if (!GameplayManager.instance.fightingBoss)
                {
                    ActionController.instance.AddToQueueAsNext(new EnemyDefeat(this));
                }
                else
                {
                    ActionController.instance.AddToQueueAsNext(new BossDefeat(this));
                }
            }
        }

        //N�O PODE TER ESSE ELSE AQUI, ESSE IF � PRA EVITAR DAR DANO DEPOIS DO INIMIGO MORRER E CHAMAR AS FUN��O DENOVO
        /*else
        {
            Die();
        }*/
    }
    public override void PlayCard(Card c)
    {
        if (!GameplayManager.instance.CombatActive) return;
        base.PlayCard(c);

        /*if (hand.Count == 0)
        {
            BuyCards(1);
        }*/
    }
    public override void Die()
    {
        base.Die();
        GameplayManager.instance.PrizeMoney();
        GameManager.instance.uiController.ChangeNamePlate("");
        //EnemyDefeat anim = new EnemyDefeat(this);
        //anim.AnimEnded.AddListener(SwitchToMap);
        //SceneAnimationController.instance.AddToQueue(anim);
    }

    /*public void SwitchToMap()
    {
        GameplayManager.instance.PlayCutscene(0);
    }*/

    public void SetModel()
    {
        model.SetActive(true);
        anim = model.GetComponentInChildren<Animator>();
    }

    public IEnumerator DisableModel()
    {
        yield return new WaitForSeconds(1);
        model.SetActive(false);
    }

    public override void EndCombat()
    {
        base.EndCombat();
        ResetHP();
    }
}
