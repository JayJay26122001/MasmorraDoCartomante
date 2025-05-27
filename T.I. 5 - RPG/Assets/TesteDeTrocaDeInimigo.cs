using System.Collections.Generic;
using UnityEngine;

public class TesteDeTrocaDeInimigo : MonoBehaviour
{
    public List<GameObject> enemyAssets;
    GameObject activeEnemy;
    int activeIndex;
    private void Awake()
    {
        SetActiveEnemy(0);
    }
    /*void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && GameplayManager.instance.InputActive)
        {
            SetActiveEnemy((activeIndex + 1) % enemyAssets.Count);
        }
    }*/
    public void SetActiveEnemy(int index)
    {
        activeEnemy?.SetActive(false);
        activeEnemy = enemyAssets[index];
        activeEnemy.SetActive(true);
        activeIndex = index;
        GetComponent<Enemy>().anim = activeEnemy.GetComponentInChildren<Animator>();
    }
}
