using UnityEngine;

public class Bell : MonoBehaviour
{
    public AudioSource sfxSource;
    public bool pointedToPlayer;
    public float turnSpeed;
    public AnimationClip turnToPlayer, turnToEnemy;
    public Animation anim;
    public GameObject outline;
    Interactable interactable;
    public ControlUI passTurn;
    void Awake()
    {
        interactable = GetComponent<Interactable>();
        outline.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
        outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.25f);
        outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.25f);
        outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0.1f);
        outline.GetComponent<MeshRenderer>().material.SetFloat("_Offset", 0.2f);
        outline.SetActive(false);
        GameplayManager.TurnArrow = this;
    }
    void Start()
    {
        anim = GetComponent<Animation>();
        sfxSource = this.GetComponent<AudioSource>();
    }

    public void ChangeInteractions(bool active)
    {
        interactable.HideInteractions();
        interactable.interactions.Clear();
        if(active)
        {
            interactable.interactions.Add(passTurn);
        }
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
