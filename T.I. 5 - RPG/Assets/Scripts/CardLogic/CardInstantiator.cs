using UnityEngine;

public class CardInstantiator : MonoBehaviour
{
    public static CardInstantiator instance;
    public CardDisplay cardPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    /*public void InstantiateDeck(Deck deck)
    {

    }*/

    public void InstantiateCard(Card card)
    {
        CardDisplay temp = Instantiate(cardPrefab.gameObject).GetComponent<CardDisplay>();
        temp.cardData = card;
        temp.CardSetup();
    }
}
