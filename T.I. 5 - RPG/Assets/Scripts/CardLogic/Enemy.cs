using UnityEngine;
using System.Collections;
public class Enemy : Creature
{
    public Animator anim;
    public GameObject model;

    public override void CombatStartAction()
    {
        base.CombatStartAction();
        BuyCards(1);
        SetModel();
    }
    public override void TurnAction()
    {
        /*if (hand[0].Type == Card.CardType.Mind)
        {
            hand[0].hidden = true;
        }*/
        EnemyPlayCard anim = new EnemyPlayCard(this);
        EnemyCardAnimation playCardAnim = new EnemyCardAnimation(this);
        anim.AnimEnded.AddListener(TurnActionsDelayed);
        SceneAnimationController.instance.AddToQueue(anim);
        SceneAnimationController.instance.AddToQueue(playCardAnim);
    }

    void TurnActionsDelayed()
    {
        if (!GameplayManager.instance.CombatActive) return;
        if (hand.Count == 0)
        {
            BuyCards(1);
        }
        PlayCard(hand[0]);
        if (hand.Count == 0)
        {
            BuyCards(1);
        }
        //GameplayManager.currentCombat.AdvanceCombat();
    }
    public override void TakeDamage(int damage)
    {
        if (Health > 0)
        {
            damage -= (int)(BaseDamageReduction * damage);
            if (damage <= 0) { return; }
            int trueDamage = (int)Mathf.Clamp(damage - Shield, 0, Mathf.Infinity);
            Shield -= damage;
            if (trueDamage == 0)
            {
                DamageBlocked.Invoke();
            }
            else
            {
                Wounded.Invoke();
            }
            Health -= trueDamage;
            Damaged.Invoke();
            if (Health <= 0)
            {
                Die();
            }
            else
            {
                EnemyTakeDamage anim = new EnemyTakeDamage(this);
                SceneAnimationController.instance.AddToQueue(anim);
            }
        }
        else
        {
            Die();
        }
    }
    public override void TakeDamage(int damage, bool IgnoreDefense)
    {
        if (Health > 0)
        {
            damage -= (int)(BaseDamageReduction * damage);
            if (damage <= 0) { return; }
            int trueDamage;
            if (IgnoreDefense)
            {
                trueDamage = damage;
                Wounded.Invoke();
            }
            else
            {
                trueDamage = (int)Mathf.Clamp(damage - Shield, 0, Mathf.Infinity);
                Shield -= damage;
                if (trueDamage == 0)
                {
                    DamageBlocked.Invoke();
                }
                else
                {
                    Wounded.Invoke();
                }
            }
            Health -= trueDamage;
            Damaged.Invoke();
            if (Health <= 0)
            {
                Die();
            }
            else
            {
                EnemyTakeDamage anim = new EnemyTakeDamage(this);
                SceneAnimationController.instance.AddToQueue(anim);
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
        base.PlayCard(c);
    }
    public override void Die()
    {
        base.Die();
        GameplayManager.instance.player.ChangeMoney(money);
        EnemyDefeat anim = new EnemyDefeat(this);
        anim.AnimEnded.AddListener(SwitchToMap);
        SceneAnimationController.instance.AddToQueue(anim);
    }

    public void SwitchToMap()
    {
        GameplayManager.instance.PlayCutscene(0);
    }

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
