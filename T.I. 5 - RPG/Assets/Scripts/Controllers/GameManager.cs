using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public UIController uiController;

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
        //DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
    }
    /*public CardVar card = new CardVar();
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (Card c in card.GetCardsWithStats(GameplayManager.currentCombat.combatents[0])) {
                Debug.Log(c.name);
            }
            
        }
    }*/
}
