using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class SceneAnimationController : MonoBehaviour
{
    public PlayableDirector director;
    public static SceneAnimationController instance;
    List<AnimationAction> AnimQueue = new List<AnimationAction>();
    public PlayableAsset EnterCombatScene, ExitCombatSecene, EnterShopScene, ExitShopScene;
    void Awake()
    {
        director = GetComponent<PlayableDirector>();
        instance = this;
    }
    public void AddToQueue(AnimationAction action)
    {
        //action.AnimEnded.AddListener(AdvanceQueue);
        AnimQueue.Add(action);
        UpdateQueueIndexes();
        if (AnimQueue.Count == 1)
        {
            AnimQueue[0].StartAction();
        }
    }
    public void AddToQueue(AnimationAction action, int index)
    {
        //action.AnimEnded.AddListener(AdvanceQueue);
        AnimQueue.Insert(index,action);
        UpdateQueueIndexes();
        if (AnimQueue.Count == 1)
        {
            AnimQueue[0].StartAction();
        }
    }
    public void UpdateQueueIndexes()
    {
        for (int i = 0; i < AnimQueue.Count; i++) 
        {
            AnimQueue[i].QueueIndex = i;
        }
    }
    public void AdvanceQueue()
    {
        AnimQueue.Remove(AnimQueue[0]);
        UpdateQueueIndexes();
        if (AnimQueue.Count > 0)
        {
            AnimQueue[0].StartAction();
        }
    }

    public void PlayAnimation(PlayableAsset animation)
    {
        director.playableAsset = animation;
        director.Play();
    }
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
public abstract class AnimationAction
{
    public float time;
    public int QueueIndex = 0;
    public void StartAction()
    {
        PerformAction();
        SceneAnimationController.instance.InvokeTimer(SceneAnimationController.instance.AdvanceQueue, time);
    }

    public abstract void PerformAction();
    public UnityEvent AnimStarted = new UnityEvent(), AnimEnded = new UnityEvent();
}
public class EnemyPlayCard : AnimationAction
{
    Enemy enemy;
    //AnimationClip playCard;
    public EnemyPlayCard(Enemy e, Card c)
    {
        enemy = e;
        foreach (AnimationClip a in enemy.anim.runtimeAnimatorController.animationClips)
        {
            if (a.name == "PlayCard")
            {
                //playCard = a;
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
        SceneAnimationController.instance.InvokeTimer(CameraController.instance.ChangeCamera,0, time);
        SceneAnimationController.instance.InvokeTimer(AnimEnded.Invoke, time);

        WaitAction enemyCardAnim = new WaitAction(1f);
        enemyCardAnim.AnimStarted.AddListener(() => CardUIController.OrganizeEnemyPlayedCards(enemy));
        SceneAnimationController.instance.AddToQueue(enemyCardAnim, QueueIndex+1);
    }
}
/*public class EnemyCardAnimation : AnimationAction
{
    Enemy c;
    public EnemyCardAnimation(Enemy e)
    {
        c = e;
        time = 1f;
    }

    public override void PerformAction()
    {
        GameplayManager.instance.PauseInput(time);
        AnimStarted.Invoke();
        CardUIController.OrganizeEnemyPlayedCards(c);
        SceneAnimationController.instance.InvokeTimer(AnimEnded.Invoke, time);
    }
}*/
public class EnemyTakeDamage : AnimationAction
{
    Enemy c;
    //AnimationClip playCard;
    public EnemyTakeDamage(Enemy e)
    {
        c = e;
        foreach (AnimationClip a in c.anim.runtimeAnimatorController.animationClips)
        {
            if (a.name == "TakeDamage")
            {
                //playCard = a;
                time = a.length;
            }
        }
    }
    public override void PerformAction()
    {
        c.anim.SetTrigger("TakeDamage");
        GameplayManager.instance.PauseInput(time);
        AnimStarted.Invoke();
        CameraController.instance.ChangeCamera(1);
        SceneAnimationController.instance.InvokeTimer(CameraController.instance.ChangeCamera,0, time);
        SceneAnimationController.instance.InvokeTimer(AnimEnded.Invoke, time);
    }
}
public class EnemyDefeat : AnimationAction
{
    Enemy c;
    //AnimationClip playCard;
    public EnemyDefeat(Enemy e)
    {
        c = e;
        foreach (AnimationClip a in c.anim.runtimeAnimatorController.animationClips)
        {
            if (a.name == "Defeat")
            {
                //playCard = a;
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
        SceneAnimationController.instance.InvokeTimer(AnimEnded.Invoke, time);
    }
}

public class AdvanceCombatAction : AnimationAction
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
        SceneAnimationController.instance.InvokeTimer(action, time);
    }
}
public class WaitAction : AnimationAction
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
        SceneAnimationController.instance.InvokeTimer(action, time);
    }
}
