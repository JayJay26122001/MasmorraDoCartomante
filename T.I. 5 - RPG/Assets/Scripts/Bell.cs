using UnityEngine;

public class Bell : MonoBehaviour
{
    public AudioSource sfxSource;

    void Start()
    {
        sfxSource = this.GetComponent<AudioSource>();
    }

    public void PlayBellSFX()
    {
        sfxSource.PlayOneShot(AudioController.instance.soundEffects[0]);
    }
}
