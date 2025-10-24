using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFadeController : MonoBehaviour
{
    public static SceneFadeController instance;
    [Header("Fade Settings")]
    public CanvasGroup fadeImage;
    public float fadeDuration;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    public void FadeIn()
    {
        fadeImage.blocksRaycasts = true;
        fadeImage.alpha = 1f;
        LeanTween.alphaCanvas(fadeImage, 0f, fadeDuration)
            .setIgnoreTimeScale(true)
            .setOnComplete(() => fadeImage.blocksRaycasts = false);
    }

    public void FadeOutToScene(string sceneName)
    {
        fadeImage.blocksRaycasts = true;
        LeanTween.alphaCanvas(fadeImage, 1f, fadeDuration)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                if(GameManager.instance.uiController.gamePaused)
                {
                    Time.timeScale = 1f;
                    GameManager.instance.uiController.gamePaused = false;
                }
                SceneManager.LoadScene(sceneName);
                LeanTween.delayedCall(0.5f, FadeIn);
            });
    }

    public void FadeOutToArea()
    {
        fadeImage.blocksRaycasts = true;
        LeanTween.alphaCanvas(fadeImage, 1f, fadeDuration)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                if(GameManager.instance.uiController.gamePaused)
                {
                    Time.timeScale = 1f;
                    GameManager.instance.uiController.gamePaused = false;
                }
                GameplayManager.instance.SwitchArea();
                LeanTween.delayedCall(0.15f, FadeIn);
            });
    }
}
