using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;
    public AudioMixer mixer;
    public AudioSource musicSource, auxSource;
    public AudioClip[] musics;
    public AudioClip[] soundEffects;
    public AudioClip[] menuPlaylist;
    //private bool playingIntro;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
        musicSource = this.GetComponent<AudioSource>();
    }

    public void StartMusic()
    {
        /*switch (scene)
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
        }*/
        if(SceneManager.GetActiveScene().name == "Menu")
        {
            PlayMenuMusic();
        }
        else if(SceneManager.GetActiveScene().name == "Game")
        {
            PlayMapMusic();
        }
        else if(SceneManager.GetActiveScene().name == "Victory")
        {
            //Música de Cena de Vitória
        }
        else
        {
            //Música de Cena de Derrota
        }
    }

    public void PlayMenuMusic()
    {
        /*playingIntro = true;
        musicSource.clip = musics[0];
        musicSource.loop = false;
        musicSource.Play();
        Invoke(nameof(PlayLoopMusic), musics[0].length - 0.75f);*/
        double startTime = AudioSettings.dspTime;
        auxSource.clip = musics[0];
        auxSource.PlayScheduled(startTime);
        musicSource.clip = musics[1];
        musicSource.loop = true;
        musicSource.PlayScheduled(startTime + musics[0].length);
    }

    /*public void PlayLoopMusic()
    {
        musicSource.clip = musics[1];
        musicSource.loop = true;
        musicSource.Play();
        //playingIntro = false;
    }*/

    public void PlayMapMusic()
    {
        musicSource.clip = musics[2];
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log("Playing : " + musics[2].name);
    }

    public void PlayCombatMusic()
    {
        musicSource.clip = musics[3];
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log("Playing : " + musics[3].name);
    }

    public void PlayShopMusic()
    {
        musicSource.clip = musics[4];
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log("Playing : " + musics[4].name);
    }

    public void PlayBossMusic()
    {
        musicSource.clip = musics[5];
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log("Playing : " + musics[5].name);
    }


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
