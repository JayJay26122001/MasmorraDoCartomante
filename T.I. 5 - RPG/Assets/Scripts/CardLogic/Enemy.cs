using UnityEngine;
using System.Collections;
public class Enemy : Creature
{
    public Animator anim;
    public GameObject model;
    public enum EnemySize { Small, Medium, Large };
    public EnemySize size;
    protected override void Awake()
    {
        base.Awake();
        Wounded.AddListener((DealDamage d) => GameplayManager.instance.EnemyHitVFX());
        DamageBlocked.AddListener((DealDamage d) => GameplayManager.instance.EnemyShieldVFX());
        ShieldBreak.AddListener(GameplayManager.instance.EnemyFracturedShieldVFX);
    }
    public override void CombatStartAction()
    {
        base.CombatStartAction();
        BuyCards(1);
        SetModel();
    }
    public override void TurnAction()
    {
        if (!GameplayManager.instance.CombatActive) return;
        base.TurnAction();
        bool playanim = true;
        for (int i = 0; i < hand.Count; i++)
        {
            EnemyPlayCard anim = new EnemyPlayCard(this, hand[i], playanim);
            ActionController.instance.AddToQueue(anim);
            playanim = false;
        }
        
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
            damage = (int)(damage* (BaseDamageTaken / 100));
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
            Damaged.Invoke(dmg);
            if (Health <= 0)
            {
                if(!GameplayManager.instance.figtingBoss)
                {
                    ActionController.instance.AddToQueue(new EnemyDefeat(this));
                }
                else
                {
                    ActionController.instance.AddToQueue(new BossDefeat(this));
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

        if (hand.Count == 0)
        {
            BuyCards(1);
        }
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
