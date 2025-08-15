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
    /*public ModularInt var = new ModularInt();
    public ModularFloat varFloat = new ModularFloat();
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(var.GetValue());
        }
    }*/
}
