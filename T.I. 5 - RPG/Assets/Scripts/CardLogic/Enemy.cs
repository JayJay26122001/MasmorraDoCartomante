

using UnityEngine;

public class Enemy : Creature
{
    public Animator anim;
    public override void CombatStartAction()
    {
        base.CombatStartAction();
        BuyCards(1);
    }
    public override void TurnAction()
    {
        if (hand[0].Type == Card.CardType.Mind)
        {
            hand[0].hidden = true;
        }
        EnemyPlayCard anim = new EnemyPlayCard(this);
        EnemyCardAnimation playCardAnim = new EnemyCardAnimation(this);
        anim.AnimEnded.AddListener(TurnActionsDelayed);
        //anim.AnimEnded.AddListener(TurnActionsDelayed);
        SceneAnimationController.instance.AddToQueue(anim);
        SceneAnimationController.instance.AddToQueue(playCardAnim);
    }

    public override void TakeDamage(int damage)
    {
        if (Health > 0)
        {
            if (damage < 0) damage = 0;
            int trueDamage = (int)Mathf.Clamp(damage - Shield, 0, Mathf.Infinity);
            Shield -= damage;
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
    }
    public override void TakeDamage(int damage, bool IgnoreDefense)
    {
        if (Health > 0)
        {
            if (damage < 0) damage = 0;
            int trueDamage;
            if (IgnoreDefense)
            {
                trueDamage = damage;
            }
            else
            {
                trueDamage = (int)Mathf.Clamp(damage - Shield, 0, Mathf.Infinity);
                Shield -= damage;
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
    }
    void TurnActionsDelayed()
    {
        PlayCard(hand[0]);
        BuyCards(1);
        GameplayManager.currentCombat.AdvanceCombat();
    }
    public override void PlayCard(Card c)
    {
        base.PlayCard(c);
    }
    public override void Die()
    {
        base.Die();
        GameplayManager.instance.player.ChangeMoney(money);
        SceneAnimationController.instance.AddToQueue(new EnemyDefeat(this));
    }
}
