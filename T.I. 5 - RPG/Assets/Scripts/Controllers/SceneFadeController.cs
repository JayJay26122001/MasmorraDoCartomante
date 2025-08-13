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
        LeanTween.alphaCanvas(fadeImage, 0f, fadeDuration).setOnComplete(() => fadeImage.blocksRaycasts = false);
    }

    public void FadeOutToScene(string sceneName)
    {
        fadeImage.blocksRaycasts = true;
        LeanTween.alphaCanvas(fadeImage, 1f, fadeDuration)
            .setOnComplete(() =>
            {
                SceneManager.LoadScene(sceneName);
                LeanTween.delayedCall(0.05f, FadeIn);
            });
    }
}
