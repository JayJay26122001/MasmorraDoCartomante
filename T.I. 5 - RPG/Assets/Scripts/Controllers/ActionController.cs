using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static Effect;

public class ActionController : MonoBehaviour
{
    //public PlayableDirector director;
    public static ActionController instance;
    public float TimeBetweenActions;
    List<SceneAction> ActionQueue = new List<SceneAction>();
    //public PlayableAsset EnterCombatScene, ExitCombatSecene, EnterShopScene, ExitShopScene;
    void Awake()
    {
        //director = GetComponent<PlayableDirector>();
        instance = this;
    }
    public int NumberOfActionsInQueue()
    {
        return ActionQueue.Count;
    }
    public static void DebugAction(SceneAction action)
    {
        string actionName;
        if (action is ApplyEffectAction)
        {
            actionName = $"{action.GetType().Name}: {action.ConvertTo<ApplyEffectAction>().e.GetType().Name}";
        }
        else
        {
            actionName = action.GetType().Name;
        }
        Debug.Log($"{actionName}");
    }
    public static void DebugAction(SceneAction action, int index)
    {
        string actionName;
        if (action is ApplyEffectAction)
        {
            actionName = $"{action.GetType().Name}: {action.ConvertTo<ApplyEffectAction>().e.GetType().Name}";
        }
        else
        {
            actionName = action.GetType().Name;
        }
        Debug.Log($"Added to index {index} {actionName}");
    }
    public void AddToQueue(SceneAction action)
    {
        action.IsInQueue = true;
        ActionQueue.Add(action);
        UpdateQueueIndexes();
        if (ActionQueue.Count == 1)
        {
            action.isPlaying = true;
            action.StartAction();
        }
    }
    public void AddToQueue(SceneAction action, int index)
    {
        action.IsInQueue = true;
        if (index > ActionQueue.Count) index = ActionQueue.Count;
        ActionQueue.Insert(index, action);
        UpdateQueueIndexes();
        if (ActionQueue.Count == 1)
        {
            action.isPlaying = true;
            action.StartAction();
        }
    }
    public void AddToQueueAsNext(SceneAction action)
    {
        action.IsInQueue = true;
        bool inserted = false;
        for (int i = 0; i < ActionQueue.Count; i++)
        {
            if (!ActionQueue[i].isPlaying)
            {
                ActionQueue.Insert(i, action);
                i = ActionQueue.Count;
                inserted = true;
            }
        }
        if (!inserted)
        {
            AddToQueueBeforeAdvance(action);
            return;
        }
        UpdateQueueIndexes();
        if (ActionQueue.Count == 1)
        {
            action.isPlaying = true;
            action.StartAction();
        }
    }
    public void AddToQueueBeforeAdvance(SceneAction action)
    {
        int index = -1;
        for (int i = 0; i < ActionQueue.Count; i++)
        {
            if (ActionQueue[i] is ICombatTurnActions && !ActionQueue[i].isPlaying)
            {
                index = i;
                i = ActionQueue.Count;
            }
        }
        if (index >= 0)
        {
            AddToQueue(action, index);
        }
        else
        {
            AddToQueue(action);
        }
    }
    public void RemoveFromQueue(SceneAction action)
    {
        if (action.isPlaying)
        {
            action.CancelAction();
        }
        else
        {
            ActionQueue.Remove(action);
        }
    }
    public void RemoveAllActions(bool RemoveActive)
    {
        List<SceneAction> aux = ActionQueue.ToList();
        foreach (SceneAction a in aux)
        {
            if(RemoveActive || !a.isPlaying)
            {
                RemoveFromQueue(a);
            }
        }
    }
    public void UpdateQueueIndexes()
    {
        for (int i = 0; i < ActionQueue.Count; i++)
        {
            ActionQueue[i].QueueIndex = i;
        }
    }
    public void AdvanceQueue()
    {
        CardUIController.AttCardDescription(GameplayManager.currentCombat.combatents[0]);
        CardUIController.AttCardDescription(GameplayManager.currentCombat.combatents[1]);
        Debug.Log("-------------NextEffect-------------");
        if (ActionQueue.Count <= 0) return;
        ActionQueue[0].isPlaying = false;
        ActionQueue.Remove(ActionQueue[0]);
        UpdateQueueIndexes();
        UnityAction action = () =>
        {
            ActionQueue[0].isPlaying = true;
            ActionQueue[0].StartAction();
        };
        if (ActionQueue.Count > 0)
        {
            InvokeTimer(action, TimeBetweenActions);
            //ActionQueue[0].StartAction();
        }
    }
    public IEnumerator InvokeTimer(UnityAction action, float time)
    {
        IEnumerator aux = CallEvent(time, action);
        StartCoroutine(aux);
        return aux;
    }
    public IEnumerator InvokeTimer<T>(UnityAction<T> action, T arg, float time)
    {
        IEnumerator aux = CallEvent(time, () => action(arg));
        StartCoroutine(aux);
        return aux;
    }
    public IEnumerator InvokeTimer<T1, T2>(UnityAction<T1, T2> action, T1 arg1, T2 arg2, float time)
    {
        IEnumerator aux = CallEvent(time, () => action(arg1, arg2));
        StartCoroutine(aux);
        return aux;
    }

    public IEnumerator InvokeTimer(UnityEvent action, float time)
    {
        IEnumerator aux = CallEvent(time, action.Invoke);
        StartCoroutine(aux);
        return aux;
    }
    public IEnumerator InvokeTimer<T>(UnityEvent<T> action, T arg, float time)
    {
        IEnumerator aux = CallEvent(time, () => action.Invoke(arg));
        StartCoroutine(aux);
        return aux;
    }
    IEnumerator CallEvent(float time, UnityAction action)
    {
        yield return new WaitForSeconds(time);
        action.Invoke();
    }
    public void CancelTimer(IEnumerator routine)
    {
        StopCoroutine(routine);
    }
}
public abstract class SceneAction
{
    public float time;
    public int QueueIndex = 0;
    public bool IsInQueue;
    public bool isPlaying;
    protected IEnumerator TimerRoutine;
    public virtual void StartAction()
    {
        UnityAction finishAction = () =>
        {
            AnimEnded.Invoke();
            if (IsInQueue)
            {
                ActionController.instance.AdvanceQueue();
            }
        };
        ActionController.DebugAction(this);
        PerformAction();
        TimerRoutine = ActionController.instance.InvokeTimer(finishAction, time);
        //ActionController.instance.InvokeTimer(() => Debug.Log(Time.time), time);
    }
    public virtual void CancelAction() //Cancela a ação se ela estiver em execução
    {
        if (TimerRoutine != null)
        {
            ActionController.instance.CancelTimer(TimerRoutine);
            if (IsInQueue && isPlaying)
            {
                ActionController.instance.AdvanceQueue();
            }
        }
    }
    public abstract void PerformAction();
    public UnityEvent AnimStarted = new UnityEvent(), AnimEnded = new UnityEvent();
}
public class EnemyPlayCard : SceneAction
{
    Enemy enemy;
    Card card;
    bool playAnim;
    public EnemyPlayCard(Enemy e, Card c, bool PlayAnimation)
    {
        enemy = e;
        card = c;
        playAnim = PlayAnimation;
        foreach (AnimationClip a in enemy.anim.runtimeAnimatorController.animationClips)
        {
            if (a.name == "PlayCard")
            {
                if (playAnim)
                {
                    time = a.length;
                }
                else
                {
                    time = 1.5f;
                }
            }
        }
    }
    public override void PerformAction()
    {
        if (playAnim)
        {
            enemy.anim.SetTrigger("PlayCard");
            GameplayManager.instance.PauseInput(time);
            AnimStarted.Invoke();
            CameraController.instance.ChangeCamera(1);
            ActionController.instance.InvokeTimer(CameraController.instance.ChangeCamera, 0, time);


            WaitAction enemyCardAnim = new WaitAction(1.5f);
            enemyCardAnim.AnimStarted.AddListener(() => enemy.PlayCard(card));
            enemyCardAnim.AnimStarted.AddListener(() => CardUIController.OrganizeEnemyPlayedCards(enemy));
            ActionController.instance.AddToQueue(enemyCardAnim, QueueIndex + 1);
        }
        else
        {
            enemy.PlayCard(card);
            CardUIController.OrganizeEnemyPlayedCards(enemy);
        }

    }
}
public class DamageAction : SceneAction
{
    Creature c;
    DealDamage dmg;
    public DamageAction(Creature creature, DealDamage damage)
    {
        c = creature;
        dmg = damage;
        if (c is Enemy e)
        {
            foreach (AnimationClip a in e.anim.runtimeAnimatorController.animationClips)
            {
                if (a.name == "TakeDamage")
                {
                    time = a.length + 0.3f;
                }
            }
        }
    }
    public override void StartAction()
    {

        ActionController.DebugAction(this);
        PerformAction();
    }
    public override void PerformAction()
    {
        UnityAction takeDMGanim = () =>
        {
            UnityAction finishAction = () =>
            {
                AnimEnded.Invoke();
                if (IsInQueue)
                {
                    ActionController.instance.AdvanceQueue();
                }
            };
            ActionController.instance.InvokeTimer(finishAction, time);
            c.TakeDamage(dmg);
            if (c is Enemy e)
            {
                e.anim.SetTrigger("TakeDamage");
                GameplayManager.instance.PauseInput(time);
                AnimStarted.Invoke();
                //CameraController.instance.ChangeCamera(1);
                CameraController.instance.ActivateDamageEnemyCam();
                //ActionController.instance.InvokeTimer(CameraController.instance.ChangeCamera, 0, time - 0.3f);
                ActionController.instance.InvokeTimer(CameraController.instance.DeactivateDamageEnemyCam, time - 0.3f);
                //ActionController.instance.InvokeTimer(AnimEnded.Invoke, time);
            }
            else
            {
                //AnimEnded.Invoke();
            }
        };
        GameplayManager.instance.ActivateCardAttack(dmg.card.cardDisplay.transform.position, c).AddListener(takeDMGanim);

    }
}
public class EnemyDefeat : SceneAction
{
    Enemy c;
    public EnemyDefeat(Enemy e)
    {
        c = e;
        foreach (AnimationClip a in c.anim.runtimeAnimatorController.animationClips)
        {
            if (a.name == "Defeat")
            {
                time = a.length;
            }
        }
        int rand = UnityEngine.Random.Range(0, 100);
        if(rand >= GameplayManager.instance.dropChance)
        {
            AnimEnded.AddListener(() => GameplayManager.instance.PlayCutscene(0));
        }
        else
        {
            AnimEnded.AddListener(() => GameplayManager.instance.DefineDropPack());
            AnimEnded.AddListener(() => GameplayManager.instance.PlayCutscene(15));
        }
    }
    public override void PerformAction()
    {
        ActionController.instance.RemoveAllActions(false);
        c.Die();
        c.anim.SetTrigger("Defeat");
        GameplayManager.instance.PauseInput(time);
        AnimStarted.Invoke();
        CameraController.instance.ChangeCamera(1);
        //ActionController.instance.InvokeTimer(AnimEnded.Invoke, time);
    }
}
public class BossDefeat : SceneAction
{
    Enemy c;
    public BossDefeat(Enemy e)
    {
        c = e;
        foreach (AnimationClip a in c.anim.runtimeAnimatorController.animationClips)
        {
            if (a.name == "Defeat")
            {
                time = a.length;
            }
        }
        AnimEnded.AddListener(() => GameplayManager.instance.DefineDropPack());
        AnimEnded.AddListener(() => GameplayManager.instance.PlayCutscene(15));
        //AnimEnded.AddListener(() => GameplayManager.instance.PlayCutscene(6));
    }
    public override void PerformAction()
    {
        ActionController.instance.RemoveAllActions(false);
        c.Die();
        c.anim.SetTrigger("Defeat");
        GameplayManager.instance.PauseInput(time);
        AnimStarted.Invoke();
        CameraController.instance.ChangeCamera(1);
        //ActionController.instance.InvokeTimer(AnimEnded.Invoke, time);
    }
}
public interface ICombatTurnActions
{

}
public class AdvanceCombatAction : SceneAction, ICombatTurnActions
{
    public AdvanceCombatAction(float WaitBeforeAdvance)
    {
        time = WaitBeforeAdvance;
    }
    public override void PerformAction()
    {
        GameplayManager.instance.IPauseInput();
        AnimStarted.Invoke();
        UnityAction action = () =>
        {
            GameplayManager.currentCombat.AdvanceCombat();
            //AnimEnded.Invoke();
            GameplayManager.instance.IResumeInput();
        };
        //ActionController.instance.InvokeTimer(action, time);
        AnimEnded.AddListener(action);
    }
}
public class EndTurnAction : SceneAction, ICombatTurnActions
{
    public override void StartAction()
    {
        ActionController.DebugAction(this);
        PerformAction();
    }
    public override void PerformAction()
    {
        GameplayManager.instance.IPauseInput();
        AnimStarted.Invoke();
        GameplayManager.currentCombat.ActiveTurn.currentPhase.EndPhase();
        AnimEnded.Invoke();
        GameplayManager.instance.IResumeInput();
        if (IsInQueue)
        {
            ActionController.instance.AdvanceQueue();
        }
    }
}
public class WaitAction : SceneAction
{
    public WaitAction(float Time)
    {
        time = Time;
    }
    public override void PerformAction()
    {
        GameplayManager.instance.IPauseInput();
        AnimStarted.Invoke();
        /*UnityAction action = () =>
        {
            AnimEnded.Invoke();
            GameplayManager.instance.ResumeInput();
        };
        ActionController.instance.InvokeTimer(action, time);*/
        AnimEnded.AddListener(GameplayManager.instance.IResumeInput);
    }
}
public class ApplyEffectAction : SceneAction
{
    public readonly Effect e;
    //UnityAction finishAction;
    public ApplyEffectAction(Effect effect)
    {
        e = effect;
    }
    public override void StartAction()
    {
        e.state = EffectState.InProgress;
        foreach (ConfirmationCondition c in e.ConfirmationConditions)
        {
            if (!c.Confirm())
            {
                e.state = Effect.EffectState.Failled;
                e.EffectEnded();
                if (IsInQueue)
                {
                    ActionController.instance.AdvanceQueue();
                }
                return;
            }
        }
        ActionController.DebugAction(this);
        PerformAction();
    }
    public override void PerformAction()
    {
        GameplayManager.instance.IPauseInput();
        AnimEnded.AddListener(GameplayManager.instance.IResumeInput);
        AnimStarted.Invoke();
        UnityAction endingAction = () =>
        {
            UnityEvent scribedEvent;
            if (e is IProlongedEffect)
            {
                scribedEvent = e.ConvertTo<IProlongedEffect>().EffectApplied;
            }
            else
            {
                scribedEvent = e.EffectEnd;
            }
            UnityAction finishAction = null;
            finishAction = () =>
            {
                AnimEnded.Invoke();
                if (IsInQueue)
                {
                    ActionController.instance.AdvanceQueue();
                }
                scribedEvent.RemoveListener(finishAction);
            };
            scribedEvent.AddListener(finishAction);
            e.Apply();
        };
        if (e is IHiddenEffect)
        {
            endingAction();
            return;
        }
        ParticleSystem VFX = e.card.cardDisplay.SelectedActivationVFX;
        if (!VFX.isPlaying)
        {
            VFX.Play();
            float dur = VFX.main.duration;
            ActionController.instance.InvokeTimer(endingAction, dur);

        }
        else
        {
            float dur = VFX.main.duration - VFX.totalTime;
            ActionController.instance.InvokeTimer(endingAction, dur);
        }
        if (e is IProlongedEffect effect)
        {
            effect.EffectApplied.AddListener(() => e.card.cardDisplay.SetActivatedEffectVFX(true));
            e.EffectEnd.AddListener(() => e.card.cardDisplay.SetActivatedEffectVFX(false));
        }
        else
        {
            e.card.cardDisplay.PlayActivatedEffectOnce();
        }
    }
    public override void CancelAction() //Cancela a ação se ela estiver em execução
    {
        if (TimerRoutine != null)
        {
            ActionController.instance.CancelTimer(TimerRoutine);
            if (IsInQueue && isPlaying)
            {
                ActionController.instance.AdvanceQueue();
            }
            e.EffectEnded();
        }
    }
}