using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Deck", menuName = "CardLogic/Deck")]
public class Deck : ScriptableObject
{
    public Creature Owner;
    [SerializeField] List<Card> CardPresets = new List<Card>();
    public List<Card> cards = new List<Card>();
    public SerializableStack<Card> BuyingPile = new SerializableStack<Card>(), DiscardPile = new SerializableStack<Card>();
    public void Setup()
    {
        foreach (Card c in CardPresets)
        {
            AddCard(c);
        }
    }

    public void AddCard(Card preset)
    {
        Card card = Instantiate(preset);
        card.Setup();
        card.deck = this;
        cards.Add(card);
        CardUIController.instance.InstantiateCard(card);
    }
    public void ShuffleDeck() // coloca todas as cartas na pilha de descarte na pilha de compras e as embaralha
    {
        while (DiscardPile.Count>0)
        {
            BuyingPile.Add(DiscardPile.GetTop());
        }
        ShufflePile(ref BuyingPile);
    }
    public void ShufflePile(ref SerializableStack<Card> pile) // embaralha apenas uma pilha (descarte ou compra)
    {
        pile = ListUT.Shuffle(pile);
    }
    public void StartShuffle()// embaralha as cartas do deck e bota na pilha de compra (USAR APENAS NO INICIO DO COMBATE)
    {
        ResetPiles();
        BuyingPile = ListUT.ToStack(ListUT.Shuffle(cards));
    }
    public void ResetPiles()// limpa pilha de compra e descarte
    {
        BuyingPile.Clear();
        DiscardPile.Clear();
    }
    
}
