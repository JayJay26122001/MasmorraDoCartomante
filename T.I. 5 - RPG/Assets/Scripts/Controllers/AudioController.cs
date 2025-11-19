using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;
    public AudioMixer mixer;
    public AudioSource musicSource, sfxSource, auxSource;
    public AudioClip[] menuMusics, combatMusics, idleMusics, shopMusics, bossMusics;
    public AudioClip[] bellSfx, receiveCardSfx, buttonClickSfx, shuffleDeckSfx, playCardSfx;
    //public AudioClip[] menuPlaylist;
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
        //musicSource = this.GetComponent<AudioSource>();
        //sfxSource = this.GetComponent<AudioSource> ();
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
        /*else if(SceneManager.GetActiveScene().name == "Game")
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
        }*/
    }

    public void StartGameplayMusic()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            PlayMapMusic();
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
        auxSource.Stop();
        musicSource.Stop();
        auxSource.clip = menuMusics[0];
        auxSource.PlayScheduled(startTime);
        musicSource.clip = menuMusics[1];
        musicSource.loop = true;
        musicSource.PlayScheduled(startTime + menuMusics[0].length);
    }

    /*public void PlayLoopMusic()
    {
        musicSource.clip = musics[1];
        musicSource.loop = true;
        musicSource.Play();
        //playingIntro = false;
    }*/

    public void PlayAfterBossMusic()
    {
        int level = GameplayManager.instance.areaIndex;
        auxSource.Stop();
        musicSource.Stop();
        musicSource.clip = idleMusics[level-1];
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log("Playing : " + idleMusics[level].name);
    }

    public void PlayMapMusic()
    {
        int level = GameplayManager.instance.areaIndex;
        auxSource.Stop();
        musicSource.Stop();
        musicSource.clip = idleMusics[level];
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log("Playing : " + idleMusics[level].name);
    }

    public void PlayCombatMusic()
    {
        int level = GameplayManager.instance.areaIndex;
        auxSource.Stop();
        musicSource.Stop();
        musicSource.clip = combatMusics[level];
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log("Playing : " + combatMusics[level].name);
    }

    public void PlayShopMusic()
    {
        int level = GameplayManager.instance.areaIndex;
        auxSource.Stop();
        musicSource.Stop();
        musicSource.clip = shopMusics[level];
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log("Playing : " + shopMusics[level].name);
    }

    public void PlayBossMusic()
    {
        int level = GameplayManager.instance.areaIndex;
        auxSource.Stop();
        musicSource.Stop();
        musicSource.clip = bossMusics[level];
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log("Playing : " + bossMusics[level].name);
    }

    public void RandomizeSfx(AudioSource s, AudioClip[] sfxArray)
    {
        int randomIndex = Random.Range(0, sfxArray.Length);
        AudioClip chosenClip = sfxArray[randomIndex];
        //Debug.Log(chosenClip.name);
        s.PlayOneShot(chosenClip);
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
