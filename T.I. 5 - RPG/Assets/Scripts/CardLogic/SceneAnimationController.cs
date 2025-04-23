using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class SceneAnimationController : MonoBehaviour
{
    public PlayableDirector director;
    public static SceneAnimationController AnimController;
    public PlayableAsset EnterCombatScene, ExitCombatSecene, EnterShopScene, ExitShopScene;
    void Awake()
    {
        director = GetComponent<PlayableDirector>();
        AnimController = this;
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
    IEnumerator CallEvent(float time, UnityAction action)
    {
        yield return new WaitForSeconds(time);
        action.Invoke();
    }
    public void InvokeTimer(UnityEvent action, float time)
    {
        StartCoroutine(CallEvent(time, action));
    }
    IEnumerator CallEvent(float time, UnityEvent action)
    {
        yield return new WaitForSeconds(time);
        action.Invoke();
    }
}
