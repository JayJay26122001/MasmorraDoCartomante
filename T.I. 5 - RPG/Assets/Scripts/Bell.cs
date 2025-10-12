using UnityEngine;

public class Bell : MonoBehaviour
{
    public AudioSource sfxSource;
    public bool pointedToPlayer;
    public float turnSpeed;
    public AnimationClip turnToPlayer, turnToEnemy;
    public Animation anim;
    public GameObject outline;
    void Awake()
    {
        outline.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
        outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.25f);
        outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.25f);
        outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0.1f);
        outline.SetActive(false);
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
            outline.GetComponent<Animation>().clip = turnToEnemy;
        }
        else
        {
            anim.clip = turnToPlayer;
            outline.GetComponent<Animation>().clip = turnToPlayer;
        }
        anim.Play();
        outline.GetComponent<Animation>().Play();
        pointedToPlayer = !pointedToPlayer;
    }
}
