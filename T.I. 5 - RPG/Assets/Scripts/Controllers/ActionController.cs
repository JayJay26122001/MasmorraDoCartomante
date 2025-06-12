using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionController : MonoBehaviour
{
    //public PlayableDirector director;
    public static ActionController instance;
    List<SceneAction> ActionQueue = new List<SceneAction>();
    //public PlayableAsset EnterCombatScene, ExitCombatSecene, EnterShopScene, ExitShopScene;
    void Awake()
    {
        //director = GetComponent<PlayableDirector>();
        instance = this;
    }
    public void AddToQueue(SceneAction action)
    {
        //action.AnimEnded.AddListener(AdvanceQueue);
        ActionQueue.Add(action);
        UpdateQueueIndexes();
        if (ActionQueue.Count == 1)
        {
            ActionQueue[0].StartAction();
        }
    }
    public void AddToQueue(SceneAction action, int index)
    {
        //action.AnimEnded.AddListener(AdvanceQueue);
        ActionQueue.Insert(index, action);
        UpdateQueueIndexes();
        if (ActionQueue.Count == 1)
        {
            ActionQueue[0].StartAction();
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
        ActionQueue.Remove(ActionQueue[0]);
        UpdateQueueIndexes();
        if (ActionQueue.Count > 0)
        {
            ActionQueue[0].StartAction();
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
}
public abstract class SceneAction
{
    public float time;
    public int QueueIndex = 0;
    public void StartAction()
    {
        PerformAction();
        ActionController.instance.InvokeTimer(ActionController.instance.AdvanceQueue, time);
    }

    public abstract void PerformAction();
    public UnityEvent AnimStarted = new UnityEvent(), AnimEnded = new UnityEvent();
}
public class EnemyPlayCard : SceneAction
{
    Enemy enemy;
    public EnemyPlayCard(Enemy e, Card c)
    {
        enemy = e;
        foreach (AnimationClip a in enemy.anim.runtimeAnimatorController.animationClips)
        {
            if (a.name == "PlayCard")
            {
                time = a.length;
            }
        }
        AnimEnded.AddListener(() => e.PlayCard(c));
    }
    public override void PerformAction()
    {
        enemy.anim.SetTrigger("PlayCard");
        GameplayManager.instance.PauseInput(time);
        AnimStarted.Invoke();
        CameraController.instance.ChangeCamera(1);
        ActionController.instance.InvokeTimer(CameraController.instance.ChangeCamera, 0, time);
        ActionController.instance.InvokeTimer(AnimEnded.Invoke, time);

        WaitAction enemyCardAnim = new WaitAction(1f);
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
                    time = a.length;
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
            CameraController.instance.ChangeCamera(1);
            ActionController.instance.InvokeTimer(CameraController.instance.ChangeCamera, 0, time);
            ActionController.instance.InvokeTimer(AnimEnded.Invoke, time);
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
        ActionController.instance.InvokeTimer(AnimEnded.Invoke, time);
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
            AnimEnded.Invoke();
            GameplayManager.instance.ResumeInput();
        };
        ActionController.instance.InvokeTimer(action, time);
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
        UnityAction action = () =>
        {
            AnimEnded.Invoke();
            GameplayManager.instance.ResumeInput();
        };
        ActionController.instance.InvokeTimer(action, time);
    }
}
