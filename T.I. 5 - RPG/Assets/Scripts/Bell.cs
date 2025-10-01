using UnityEngine;

public class Bell : MonoBehaviour
{
    public AudioSource sfxSource;
    public bool pointedToPlayer;
    public float turnSpeed;
    public AnimationClip turnToPlayer, turnToEnemy;
    public Animation anim;
    void Awake()
    {
        GameplayManager.TurnArrow = this;
    }
    void Start()
    {
        anim = GetComponent<Animation>();
        sfxSource = this.GetComponent<AudioSource>();
    }
    public void NextTurn()
    {
        GameplayManager.currentCombat.SetBellActive(false);
        GameplayManager.currentCombat.EndTurn();
        PlayBellSFX();
        Turn();
    }

    public void PlayBellSFX()
    {
        sfxSource.PlayOneShot(AudioController.instance.bellSfx[0], 0.25f);
    }

    public void Turn()
    {
        if (pointedToPlayer)
        {
            anim.clip = turnToEnemy;
        }
        else
        {
            anim.clip = turnToPlayer;
        }
        anim.Play();
        pointedToPlayer = !pointedToPlayer;
    }
}
