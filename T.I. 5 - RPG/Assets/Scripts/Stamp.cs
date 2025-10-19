using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class Stamp : MonoBehaviour
{
    public int price;
    public TextMeshPro priceTag;
    public GameObject emptyCard;
    Vector3 startPos;
    Card stampedCard;
    CardDisplay copy;
    public bool stamping;

    private void OnEnable()
    {
        stamping = false;
        priceTag.text = "";
        emptyCard.SetActive(true);
        LeanTween.move(emptyCard, emptyCard.transform.position - Vector3.up * 25, 0.25f);
        this.GetComponent<BoxCollider>().enabled = true;
    }

    public void Activate()
    {
        if(!GameplayManager.instance.duplicatingCards)
        {
            this.GetComponent<BoxCollider>().enabled = false;
            GameplayManager.instance.DuplicatingCards();
        }
    }

    public void SetPrice(Card c)
    {
        switch(c.Rarity)
        {
            case Card.CardRarity.Common:
                price = 1;
                break;
            case Card.CardRarity.Uncommon:
                price = 3;
                break;
            case Card.CardRarity.Rare:
                price = 10;
                break;
            case Card.CardRarity.Epic:
                price = 15;
                break;
            case Card.CardRarity.Legendary:
                price = 20;
                break;
        }
        priceTag.text = "$" + price;
    }

    public void StartStampCards(Card c, GameObject go)
    {
        stamping = true;
        stampedCard = c;
        startPos = this.transform.position;
        LeanTween.move(this.gameObject, go.transform.position + Vector3.up * 5 + Vector3.forward * (c.cardDisplay.cardBase.gameObject.transform.localScale.y / 2), 0.5f).setOnComplete(() =>
        {
            LeanTween.move(this.gameObject, go.transform.position + Vector3.up * (this.GetComponent<BoxCollider>().size.y * 0.75f) + Vector3.forward * (c.cardDisplay.cardBase.gameObject.transform.localScale.y / 2), 0.2f).setOnComplete(() =>
            {
                Invoke("StampCardsPt2", 1);
            });
        });
    }

    void StampCardsPt2()
    {
        LeanTween.move(this.gameObject, emptyCard.transform.position + Vector3.up * 5, 0.5f).setOnComplete(() =>
        {
            LeanTween.move(this.gameObject, emptyCard.transform.position + Vector3.up * (this.GetComponent<BoxCollider>().size.y * 0.75f), 0.2f).setOnComplete(() =>
            {
                copy = CardUIController.instance.InstantiateCard(stampedCard);
                copy.UpdateCard();
                copy.gameObject.transform.position = emptyCard.transform.position - Vector3.forward * (copy.cardBase.gameObject.transform.localScale.y / 2);
                copy.gameObject.transform.rotation = emptyCard.transform.rotation;
                emptyCard.SetActive(false);
                Invoke("EndStampCards", 1);
            });
        });
    }

    void EndStampCards()
    {
        LeanTween.move(this.gameObject, startPos + Vector3.up * 2, 0.5f).setOnComplete(() =>
        {
            LeanTween.move(this.gameObject, startPos, 0.5f).setEaseOutBack().setOnComplete(() =>
            {
                LeanTween.move(copy.gameObject, copy.gameObject.transform.position + Vector3.up * 25, 0.05f).setOnComplete(() =>
                {
                    Destroy(copy.gameObject);
                });
                GameplayManager.instance.DestroyDuplicatingCards();
            });
        });
    }
}
