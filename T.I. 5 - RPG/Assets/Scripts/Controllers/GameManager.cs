using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public UIController uiController;
    public List<Card> GameCards;

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
}
