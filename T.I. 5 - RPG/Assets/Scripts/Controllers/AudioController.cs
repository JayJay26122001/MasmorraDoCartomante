using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    public static AudioController controller;
    public AudioMixer mixer;
    public AudioSource musicSource;
    public AudioClip[] musics;
    public AudioClip[] playerSFX;

    private void Awake()
    {
        if (controller == null)
        {
            controller = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
        musicSource = this.GetComponent<AudioSource>();
    }

    /*public void SwitchMusic(string scene)
    {
        switch (scene)
        {
            case "Game":
            case "Level1Theater":
                if (musicSource.clip != musics[1])
                {
                    musicSource.clip = musics[1];
                    musicSource.Play();
                }
                break;
            default:
                if (musicSource.clip != musics[0])
                {
                    musicSource.clip = musics[0];
                    musicSource.Play();
                }
                break;
        }
    }*/

    public void ChangeMasterVol(float vol)
    {
        if (vol > -20)
        {
            mixer.SetFloat("MasterVol", vol);
        }
        else
        {
            mixer.SetFloat("MasterVol", -80);
        }
    }
    public void ChangeMusicVol(float vol)
    {
        if (vol > -20)
        {
            mixer.SetFloat("MusicVol", vol);
        }
        else
        {
            mixer.SetFloat("MusicVol", -80);
        }
    }
    public void ChangeSFXVol(float vol)
    {
        if (vol > -20)
        {
            mixer.SetFloat("SFXVol", vol);
        }
        else
        {
            mixer.SetFloat("SFXVol", -80);
        }
    }
}
