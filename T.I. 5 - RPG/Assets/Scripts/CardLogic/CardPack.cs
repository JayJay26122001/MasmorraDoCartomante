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
    int selectedQuantity;
    private void Start()
    {
        bought = false;
        mat = this.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        if(this.gameObject.GetComponent<ShopObject>().type == ShopObject.ObjectType.Shop)
        {
            mat.color = new Color32(150, 50, 0, 255);
        }
        else if (this.gameObject.GetComponent<ShopObject>().type == ShopObject.ObjectType.StarterAttack)
        {
            mat.color = new Color(0.5f, 0, 0, 1);
        }
        else if (this.gameObject.GetComponent<ShopObject>().type == ShopObject.ObjectType.StarterDefense)
        {
            mat.color = new Color(0, 0.15f, 0.5f, 1);
        }
        else if (this.gameObject.GetComponent<ShopObject>().type == ShopObject.ObjectType.StarterMind)
        {
            mat.color = new Color(0, 0.5f, 0, 1);
        }
        //DefineCards();
        /*foreach (Card c in cards)
        {
            Debug.Log(c.Name);
        }*/
    }

    public void DefineCards()
    {
        bought = false;
        mat = this.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        mat.SetFloat("_DisappearTime", 0);
        cardsInstances.Clear();
        cards = data.possibleCards.SelectCards(data.cardQuantity);
        if(priceText != null)
        {
            priceText.text = "$" + data.price;
        }
    }

    public void OnMouseDown()
    {
        if(GameplayManager.instance.canBuy && !bought)
        {
            if (GameplayManager.instance.player.ChangeMoney(-data.price))
            {
                this.gameObject.GetComponent<BoxCollider>().enabled = false;
                selectedQuantity = 0;
                if(data.price > 0)
                {
                    GameplayManager.instance.ExplodeCoins(this.transform.position);
                }
                GameplayManager.instance.canBuy = false;
                if(GameplayManager.instance.atShop)
                {
                    GameplayManager.instance.PlayCutscene(4);
                    discardBell.pack = this;
                }
                else
                {
                    CameraController.instance.Invoke("ActivateZoomedCamera", 0.5f);
                }
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

    public void DestroyBoughtCards(Card selected)
    {
        if(selected != null)
        {
            selectedQuantity++;
        }
        else
        {
            selectedQuantity = data.selectableCardsQuantity;
        }
        if(selectedQuantity == data.selectableCardsQuantity)
        {
            if (GameplayManager.instance.atShop)
            {
                GameplayManager.instance.PlayCutscene(5);
                discardBell.pack = null;
            }
            else
            {
                CameraController.instance.Invoke("DeActivateZoomedCamera", 0.18f);
                GameplayManager.instance.OpenedStarterPack();
            }
            GameplayManager.instance.canBuy = true;
            foreach (Card c in cardsInstances)
            {
                var moveTween = LeanTween.move(c.cardDisplay.gameObject, c.cardDisplay.gameObject.transform.position + Vector3.up * 25, 0.15f);
                moveTween.setOnComplete(() =>
                {
                    Destroy(c.cardDisplay.gameObject);
                });
            }
            cardsInstances.Clear();
        }
        else
        {
            var moveTween = LeanTween.move(selected.cardDisplay.gameObject, selected.cardDisplay.gameObject.transform.position + Vector3.up * 25, 0.15f);
            moveTween.setOnComplete(() =>
            {
                cardsInstances.Remove(selected);
                Destroy(selected.cardDisplay.gameObject);
            });
        }
    }

    float animTimeStart;
    bool inAnimation, disappearing;
    float t;
    public void AnimatePack(bool disappear)
    {
        disappearing = disappear;
        if(priceText != null)
        {
            priceText.text = "";
        }
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
                    if(priceText != null)
                    {
                        priceText.text = "$" + data.price;
                    }
                    bought = false;
                }
            }
        }
    }
}
