using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class CardPack : MonoBehaviour
{
    public CardPackSO data;
    public List<Card> cards;
    public List<Card> cardsInstances;
    public TextMeshPro priceText;
    public GameObject discardBell;
    private void Start()
    {
        //DefineCards();
        /*foreach (Card c in cards)
        {
            Debug.Log(c.Name);
        }*/
    }

    public void DefineCards()
    {
        cardsInstances.Clear();
        cards = data.possibleCards.SelectCards(data.cardQuantity);
        priceText.text = data.price + "$";
    }

    public void OnMouseDown()
    {
        if(GameplayManager.instance.canBuy)
        {
            if (GameplayManager.instance.player.ChangeMoney(-data.price))
            {
                GameplayManager.instance.canBuy = false;
                discardBell.transform.SetParent(this.transform);
                foreach(Card c in cards)
                {
                    CardDisplay aux = CardUIController.instance.InstantiateCard(c);
                    aux.pack = this;
                    cardsInstances.Add(aux.cardData);
                    CardUIController.OrganizeBoughtPackCards(this);
                }
            }
            else
            {
                Debug.Log("You don't have enough money.");
            }
        }
    }

    public void DestroyBoughtCards()
    {
        discardBell.transform.SetParent(null);
        GameplayManager.instance.canBuy = true;
        foreach (Card c in cardsInstances)
        {
            Destroy(c.cardDisplay.gameObject);
        }
        cardsInstances.Clear();
    }
}
