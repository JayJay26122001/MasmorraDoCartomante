using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class CardPack : MonoBehaviour
{
    public CardPackSO data;
    public List<Card> cards;
    public List<Card> cardsInstances;
    public TextMeshPro priceText;
    public DiscardBell discardBell;
    public Material mat;
    [HideInInspector]public bool bought;
    private void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        mat.color = new Color32(150, 50, 0, 255);
        //DefineCards();
        /*foreach (Card c in cards)
        {
            Debug.Log(c.Name);
        }*/
    }

    public void DefineCards()
    {
        bought = false;
        mat = GetComponent<MeshRenderer>().material;
        mat.SetFloat("_DisappearTime", 0);
        cardsInstances.Clear();
        cards = data.possibleCards.SelectCards(data.cardQuantity);
        priceText.text = "$" + data.price;
    }

    public void OnMouseDown()
    {
        if(GameplayManager.instance.canBuy && !bought)
        {
            if (GameplayManager.instance.player.ChangeMoney(-data.price))
            {
                GameplayManager.instance.ExplodeCoins(this.transform.position);
                GameplayManager.instance.canBuy = false;
                GameplayManager.instance.PlayCutscene(4);
                discardBell.pack = this;
                AnimatePack(true);
                bought = true;
                foreach(Card c in cards)
                {
                    CardDisplay aux = CardUIController.instance.InstantiateCard(c);
                    aux.pack = this;
                    cardsInstances.Add(aux.cardData);
                    aux.UpdateCard();
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
        GameplayManager.instance.PlayCutscene(5);
        discardBell.pack = null;
        GameplayManager.instance.canBuy = true;
        foreach (Card c in cardsInstances)
        {
            var moveTween = LeanTween.move(c.cardDisplay.gameObject, c.cardDisplay.gameObject.transform.position + Vector3.up * 25, 0.05f);
            moveTween.setOnComplete(() =>
            {
                Destroy(c.cardDisplay.gameObject);
            });
        }
        cardsInstances.Clear();
    }

    float animTimeStart;
    bool inAnimation, disappearing;
    float t;
    public void AnimatePack(bool disappear)
    {
        disappearing = disappear;
        priceText.text = "";
        animTimeStart = Time.time;
        inAnimation = true;
    }

    private void Update()
    {
        if (inAnimation)
        {
            if(disappearing)
            {
                t = Mathf.Clamp((Time.time - animTimeStart) * 3, 0, 1);
            }
            else
            {
                t = Mathf.Clamp(1 - ((Time.time - animTimeStart) * 3), 0, 1);
            }
            mat.SetFloat("_DisappearTime", t);
            if ((t >= 1 && disappearing)|| (t <= 0 && !disappearing))
            {
                inAnimation = false;
                if(!disappearing)
                {
                    priceText.text = "$" + data.price;
                    bought = false;
                }
            }
        }
    }
}
