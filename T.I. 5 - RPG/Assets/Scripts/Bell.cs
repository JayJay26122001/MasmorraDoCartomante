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
        sfxSource.PlayOneShot(AudioController.instance.bellSfx[0], 0.25f);
    }
}
