using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

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
        //action.AnimEnded.AddListener(AdvanceQueue);
        //DebugAction(action);
        /*action.finishAction = () =>
        {
            action.AnimEnded.Invoke();
            AdvanceQueue();
        };*/
        action.IsInQueue = true;
        ActionQueue.Add(action);
        UpdateQueueIndexes();
        if (ActionQueue.Count == 1)
        {
            action.StartAction();
        }
    }
    public void AddToQueue(SceneAction action, int index)
    {
        //DebugAction(action, index);
        /*action.finishAction = () =>
        {
            action.AnimEnded.Invoke();
            AdvanceQueue();
        };*/
        action.IsInQueue = true;
        if (index > ActionQueue.Count) index = ActionQueue.Count;
        ActionQueue.Insert(index, action);
        UpdateQueueIndexes();
        if (ActionQueue.Count == 1)
        {
            action.StartAction();
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
        Debug.Log("-------------NextEffect-------------");
        if (ActionQueue.Count <= 0) return;
        ActionQueue.Remove(ActionQueue[0]);
        UpdateQueueIndexes();
        if (ActionQueue.Count > 0)
        {
            InvokeTimer(ActionQueue[0].StartAction, TimeBetweenActions);
            //ActionQueue[0].StartAction();
        }
    }

    /*public void PlayAnimation(PlayableAsset animation)
    {
        director.playableAsset = animation;
        director.Play();
    }*/
    public void InvokeTimer(UnityAction action, float time)
    {
        StartCoroutine(CallEvent(time, action));
    }
    public void InvokeTimer<T>(UnityAction<T> action, T arg, float time)
    {
        StartCoroutine(CallEvent(time, () => action(arg)));
    }
    public void InvokeTimer<T1, T2>(UnityAction<T1, T2> action, T1 arg1, T2 arg2, float time)
    {
        StartCoroutine(CallEvent(time, () => action(arg1, arg2)));
    }

    public void InvokeTimer(UnityEvent action, float time)
    {
        StartCoroutine(CallEvent(time, action.Invoke));
    }
    public void InvokeTimer<T>(UnityEvent<T> action, T arg, float time)
    {
        StartCoroutine(CallEvent(time, () => action.Invoke(arg)));
    }
    IEnumerator CallEvent(float time, UnityAction action)
    {
        yield return new WaitForSeconds(time);
        action.Invoke();
    }
    /*void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach (SceneAction a in ActionQueue)
            {
                Debug.Log($"{a.QueueIndex}: {a.GetType().Name}");
            }
        }
    }*/
}
public abstract class SceneAction
{
    public float time;
    public int QueueIndex = 0;
    public bool IsInQueue;
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
        ActionController.instance.InvokeTimer(finishAction, time);
        //ActionController.instance.InvokeTimer(() => Debug.Log(Time.time), time);
    }

    public abstract void PerformAction();
    public UnityEvent AnimStarted = new UnityEvent(), AnimEnded = new UnityEvent();
}
public class EnemyPlayCard : SceneAction
{
    Enemy enemy;
    Card card;
    public EnemyPlayCard(Enemy e, Card c)
    {
        enemy = e;
        card = c;
        foreach (AnimationClip a in enemy.anim.runtimeAnimatorController.animationClips)
        {
            if (a.name == "PlayCard")
            {
                time = a.length;
            }
        }
    }
    public override void PerformAction()
    {
        enemy.anim.SetTrigger("PlayCard");
        GameplayManager.instance.PauseInput(time);
        AnimStarted.Invoke();
        CameraController.instance.ChangeCamera(1);
        ActionController.instance.InvokeTimer(CameraController.instance.ChangeCamera, 0, time);
        //ActionController.instance.InvokeTimer(AnimEnded.Invoke, time);
        //AnimEnded.AddListener(() => Debug.Log(Time.time));

        WaitAction enemyCardAnim = new WaitAction(1f);
        enemyCardAnim.AnimStarted.AddListener(() => enemy.PlayCard(card));
        enemyCardAnim.AnimStarted.AddListener(() => CardUIController.OrganizeEnemyPlayedCards(enemy));
        ActionController.instance.AddToQueue(enemyCardAnim, QueueIndex + 1);
    }
}
public class DamageAction : SceneAction
{
    Creature c;
    int dmg;
    bool ignoreDfns;
    public DamageAction(Creature creature, int damage, bool IgnoreDefense)
    {
        c = creature;
        dmg = damage;
        ignoreDfns = IgnoreDefense;
        if (c.GetType() == typeof(Enemy))
        {
            foreach (AnimationClip a in c.GetComponent<Enemy>().anim.runtimeAnimatorController.animationClips)
            {
                if (a.name == "TakeDamage")
                {
                    time = a.length + 0.3f;
                }
            }
        }
    }
    public override void PerformAction()
    {
        c.TakeDamage(dmg, ignoreDfns);
        if (c.GetType() == typeof(Enemy))
        {
            c.GetComponent<Enemy>().anim.SetTrigger("TakeDamage");
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
        AnimEnded.AddListener(() => GameplayManager.instance.PlayCutscene(0));
    }
    public override void PerformAction()
    {
        c.Die();
        c.anim.SetTrigger("Defeat");
        GameplayManager.instance.PauseInput(time);
        AnimStarted.Invoke();
        CameraController.instance.ChangeCamera(1);
        //ActionController.instance.InvokeTimer(AnimEnded.Invoke, time);
    }
}

public class AdvanceCombatAction : SceneAction
{
    public AdvanceCombatAction(float WaitBeforeAdvance)
    {
        time = WaitBeforeAdvance;
    }
    public override void PerformAction()
    {
        GameplayManager.instance.PauseInput();
        AnimStarted.Invoke();
        UnityAction action = () =>
        {
            GameplayManager.currentCombat.AdvanceCombat();
            //AnimEnded.Invoke();
            GameplayManager.instance.ResumeInput();
        };
        //ActionController.instance.InvokeTimer(action, time);
        AnimEnded.AddListener(action);
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
        GameplayManager.instance.PauseInput();
        AnimStarted.Invoke();
        /*UnityAction action = () =>
        {
            AnimEnded.Invoke();
            GameplayManager.instance.ResumeInput();
        };
        ActionController.instance.InvokeTimer(action, time);*/
        AnimEnded.AddListener(GameplayManager.instance.ResumeInput);
    }
}
public class ApplyEffectAction : SceneAction
{
    public readonly Effect e;
    UnityAction finishAction;
    public ApplyEffectAction(Effect effect)
    {
        e = effect;
    }
    public override void StartAction()
    {
        /*finishAction = () =>
        {
            AnimEnded.Invoke();
            if (IsInQueue)
            {
                ActionController.instance.AdvanceQueue();
            }
        };*/
        ActionController.DebugAction(this);
        PerformAction();
    }
    public override void PerformAction()
    {
        AnimStarted.Invoke();
        /*if (e is ActionEffect)
        {
            e.Apply();
            AnimEnded.Invoke();
            ActionController.instance.AdvanceQueue();
        }
        else
        {
            e.EffectEnd.AddListener(AnimEnded.Invoke);
            e.EffectEnd.AddListener(ActionController.instance.AdvanceQueue);
            e.Apply();
        }*/
        UnityAction endingAction = () =>
        {
            UnityEvent scribedEvent;
            if (e is IProlongedEffect)
            {
                scribedEvent = e.ConvertTo<IProlongedEffect>().EffectApplied;
                //e.ConvertTo<IProlongedEffect>().EffectApplied.AddListener(AnimEnded.Invoke);
                //e.ConvertTo<IProlongedEffect>().EffectApplied.AddListener(ActionController.instance.AdvanceQueue);
            }
            else
            {
                scribedEvent = e.EffectEnd;
                //e.EffectEnd.AddListener(AnimEnded.Invoke);
                //e.EffectEnd.AddListener(ActionController.instance.AdvanceQueue);
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
    }
}