using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public UIController uiController;
    public List<Card> GameCards, UnlockedCards;

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
    }

    [ContextMenu("Update Cards")]
    void UpdateCards()
    {
        Card[] aux = Resources.LoadAll<Card>("");
        GameCards = aux.ToList();
    }
    public void UnlockCard(Card cardPreset)
    {
        if (!UnlockedCards.Contains(cardPreset))
        {
            UnlockedCards.Add(cardPreset);
        }
    }

    public List<Card> DefineStarterPool(Card.CardType type)
    {
        List<Card> cards = new List<Card>();
        foreach(Card c in UnlockedCards)
        {
            if(c.Type == type)
            {
                cards.Add(c);
            }
        }
        return cards;
    }
}
