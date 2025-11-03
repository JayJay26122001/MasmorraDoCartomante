using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public UIController uiController;
    public List<Card> GameCards, StarterUnlockedCards,UnlockedCards;

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
        if (UnlockedCards.Count == 0)
        {
            UnlockedCards = StarterUnlockedCards.ToList();
        }
        DontDestroyOnLoad(this.gameObject);
    }
    #if UNITY_EDITOR
    [ContextMenu("Update Cards")]
    void UpdateCards()
    {
        foreach (Card c in GameCards)
        {
            c.ChangeID(-1);

            EditorUtility.SetDirty(c); // Marks the card as modified so Unity saves it
        }
        Card[] aux = Resources.LoadAll<Card>("");
        GameCards = aux.ToList();
        UpdateIDs();
    }
    void UpdateIDs()
    {
        for (int i = 0; i < GameCards.Count; i++)
        {
            GameCards[i].ChangeID(i);
            EditorUtility.SetDirty(GameCards[i]); // Marks the card as modified so Unity saves it

        }

    }
    void OnValidate()
    {
        UpdateIDs();
    }
    #endif

    public void UnlockCard(Card cardPreset)
    {
        if (!UnlockedCards.Contains(cardPreset) && GameCards.Contains(cardPreset))
        {
            UnlockedCards.Add(cardPreset);
        }
    }
    public void ResetUnlockedCards()
    {
        UnlockedCards.Clear();
        UnlockedCards = StarterUnlockedCards.ToList();
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
