using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static Combat currentCombat;
    public static GameplayManager instance;


    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
