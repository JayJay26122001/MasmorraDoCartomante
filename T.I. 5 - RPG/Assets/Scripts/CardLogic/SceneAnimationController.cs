using System.Collections;
using System.Collections.Generic;
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
        action.AnimEnded.AddListener(AdvanceQueue);
        AnimQueue.Add(action);
        if (AnimQueue.Count == 1)
        {
            AnimQueue[0].StartAction();
        }
    }
    public void AdvanceQueue()
    {
        AnimQueue.Remove(AnimQueue[0]);
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
    public abstract void StartAction();
    public UnityEvent AnimStarted = new UnityEvent(), AnimEnded = new UnityEvent();
}
public class EnemyPlayCard : AnimationAction
{
    Enemy c;
    //AnimationClip playCard;
    public EnemyPlayCard(Enemy e)
    {
        c = e;
        foreach (AnimationClip a in c.anim.runtimeAnimatorController.animationClips)
        {
            if (a.name == "PlayCard")
            {
                //playCard = a;
                time = a.length;
            }
        }
    }
    public override void StartAction()
    {
        c.anim.SetTrigger("PlayCard");
        GameplayManager.instance.PauseInput(time);
        AnimStarted.Invoke();
        SceneAnimationController.instance.InvokeTimer(AnimEnded.Invoke, time);
        CameraController.instance.ChangeCamera(1);
    }
}
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
    public override void StartAction()
    {
        c.anim.SetTrigger("TakeDamage");
        GameplayManager.instance.PauseInput(time);
        AnimStarted.Invoke();
        SceneAnimationController.instance.InvokeTimer(AnimEnded.Invoke, time);
        CameraController.instance.ChangeCamera(1);
    }
}
