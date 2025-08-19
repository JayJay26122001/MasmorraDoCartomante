using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    /*public static void Save(Player player)
    {
        PlayerData pData = new PlayerDataAdapter(player);
        string pSave = JsonUtility.ToJson(pData);
        File.WriteAllText(Application.dataPath + "/playerSave.json", pSave);
        string cSave = "";
        foreach (CheckpointData cData in GameManager.instance.checkpoints)
        {
            cSave += JsonUtility.ToJson(cData) + " ";
        }
        File.WriteAllText(Application.dataPath + "/checkpointsSave.json", cSave);
    }*/

    public static void SaveConfig()
    {
        string s = JsonUtility.ToJson(GameManager.instance.uiController.data);
        File.WriteAllText(Application.dataPath + "/configSave.json", s);
    }

    public static void LoadConfig()
    {
        string s = File.ReadAllText(Application.dataPath + "/configSave.json");
        GameManager.instance.uiController.data = JsonUtility.FromJson<ConfigData>(s);
    }
    /*public static void LoadPlayer(Player player)
    {
        string pSave = File.ReadAllText(Application.dataPath + "/playerSave.json");
        PlayerData data = JsonUtility.FromJson<PlayerData>(pSave);
        PlayerDataAdapter.DataToPlayer(player, data);
    }
    public static void LoadCheckpoints()
    {
        string cSave = File.ReadAllText(Application.dataPath + "/checkpointsSave.json");
        string[] aux = cSave.Split(" ");
        for (int i = 0; i < aux.Length - 1; i++)
        {
            GameManager.instance.checkpoints[i] = JsonUtility.FromJson<CheckpointData>(aux[i]);
        }
    }*/
}
