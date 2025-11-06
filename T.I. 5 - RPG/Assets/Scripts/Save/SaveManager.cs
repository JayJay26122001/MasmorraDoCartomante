using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using NUnit.Framework;
using System.Linq.Expressions;

public class SaveManager : MonoBehaviour
{
    public static PlayerData GetPlayerData()
    {
        PlayerData data = new PlayerData();

        //Player Deck Infos
        data.playerDeckCards = new List<string>();
        foreach (Deck deck in GameplayManager.instance.player.decks)
        {
            foreach (Card card in deck.cards)
            {
                /*for(int i = 0; i < GameManager.instance.GameCards.Count; i++)
                {

                }*/
                string cardName = card.Name;
                data.playerDeckCards.Add(cardName);
                //int cardIndex = GameManager.instance.GameCards.IndexOf(card);
            }
        }

        //Map and Board Infos
        data.piecePos = GameplayManager.instance.bg.playerPiece.transform.localPosition;
        data.boardPos = GameplayManager.instance.bg.transform.localPosition;
        for(int i = 0; i < GameplayManager.instance.bg.board.Count; i++)
        {
            for(int j = 0; j < GameplayManager.instance.bg.board[i].Count; j++)
            {
                if (GameplayManager.instance.currentRoom == GameplayManager.instance.bg.board[i][j])
                {
                    data.currentRoomLevel = i;
                    data.currentRoomIndex = j;
                    break;
                    //j = GameplayManager.instance.bg.board[i].Count;
                    //i = GameplayManager.instance.bg.board.Count;
                }
            }
        }

        //Player Stats Infos
        data.money = GameplayManager.instance.player.Money;
        data.hp = GameplayManager.instance.player.Health;

        return data;
    }

    public static void DataToPlayer(Player player, PlayerData data)
    {
        //Player Deck
        player.decks.Clear();
        Deck newDeck = ScriptableObject.CreateInstance<Deck>();
        newDeck.CardPresets = new List<Card>();
        /*foreach (int index in data.playerDeckCards)
        {
            newDeck.cards.Add(GameManager.instance.GameCards[index]);
        }*/
        foreach (Card card in GameManager.instance.UnlockedCards)
        {
            foreach(string s in data.playerDeckCards)
            {
                if (string.Compare(s, card.Name) == 0)
                {
                    newDeck.CardPresets.Add(card);
                }
            }
        }
        player.AddDeck(newDeck);

        //Map and Board
        GameplayManager.instance.bg.playerPiece.transform.localPosition = data.piecePos;
        GameplayManager.instance.bg.transform.localPosition = data.boardPos;
        GameplayManager.instance.currentRoom = GameplayManager.instance.bg.board[data.currentRoomLevel][data.currentRoomIndex];

        //Player Stats
        player.Money = data.money;
        GameManager.instance.uiController.UpdateMoney(player.Money);
        player.Health = data.hp;
        GameplayManager.instance.UpdateCreatureUI(player);
    }

    public static UnlockedCardsData GetUnlockedCardsData()
    {
        UnlockedCardsData data = new UnlockedCardsData();

        data.unlockedCards = new List<string>();
        foreach (Card card in GameManager.instance.UnlockedCards)
        {
            string cardName = card.Name;
            data.unlockedCards.Add(cardName);
            //int cardIndex = GameManager.instance.GameCards.IndexOf(card);
        }
        return data;
    }

    public static void DataToUnlockedCards(UnlockedCardsData data)
    {
        GameManager.instance.UnlockedCards.Clear();
        foreach (Card card in GameManager.instance.GameCards)
        {
            if (data.unlockedCards.Contains(card.Name))
            {
                GameManager.instance.UnlockedCards.Add(card);
            }
        }
    }

    public static BoardData GetBoardData()
    {
        BoardData data = new BoardData();

        data.roomLevel = new List<int>();
        data.nextRoomIndex = new List<int>();
        data.nextRoomCount = new List<int>();
        data.roomTypeIndex = new List<int>();
        data.area = GameplayManager.instance.areaIndex;
        //data.roomPos = new List<Vector3>();
        for (int i = 0; i < GameplayManager.instance.bg.board.Count; i++)
        {
            foreach (BoardRoom board in GameplayManager.instance.bg.board[i])
            {
                data.roomLevel.Add(i);
                data.nextRoomCount.Add(board.nextRoomsCount);
                foreach (BoardRoom room in board.nextRooms)
                {
                    data.nextRoomIndex.Add(GameplayManager.instance.bg.board[i+1].IndexOf(room));
                }
                if(board.type == GameplayManager.instance.bg.boards[GameplayManager.instance.areaIndex].battleRoom)
                {
                    data.roomTypeIndex.Add(-2);
                }
                else if(board.type == GameplayManager.instance.bg.boards[GameplayManager.instance.areaIndex].shopRoom)
                {
                    data.roomTypeIndex.Add(-3);
                }
                else
                {
                    data.roomTypeIndex.Add(GameplayManager.instance.bg.boards[GameplayManager.instance.areaIndex].roomList.IndexOf(board.type));
                }
                //data.roomPos.Add(board.roomObject.gameObject.transform.localPosition);
            }
        }

        /*data.LineStartPos = new List<Vector3>();
        data.LineEndPos = new List<Vector3>();
        foreach (GameObject go in GameplayManager.instance.bg.lineObjects)
        {
            LineRenderer lineRenderer = go.GetComponent<LineRenderer>();
            data.LineStartPos.Add(lineRenderer.GetPosition(0));
            data.LineEndPos.Add(lineRenderer.GetPosition(1));
        }*/

        return data;
    }

    public static void DataToBoard(BoardGenerator bg, BoardData data)
    {
        //GameplayManager.instance.bg.board.Clear();
        bg.board.Clear();
        GameplayManager.instance.areaIndex = data.area;
        GameplayManager.instance.SwitchArea();
        for (int i = 0; i < bg.boards[GameplayManager.instance.areaIndex].levelsCount; i++)
        {
            bg.board.Add(new List<BoardRoom>());
        }
        BoardRoom newRoom;
        for (int i = 0; i < data.roomLevel.Count; i++)
        {
            if (data.roomLevel[i] == 0)
            {
                newRoom = new BoardRoom(bg.boards[GameplayManager.instance.areaIndex].startRoom, null, data.nextRoomCount[i], 0, false);
            }
            else if(data.roomLevel[i] == bg.boards[GameplayManager.instance.areaIndex].levelsCount - 1)
            {
                newRoom = new BoardRoom(bg.boards[GameplayManager.instance.areaIndex].bossRoom);
            }
            else
            {
                if (data.roomTypeIndex[i] == -2)
                {
                    newRoom = new BoardRoom(bg.boards[GameplayManager.instance.areaIndex].battleRoom, null, data.nextRoomCount[i], 0, false);
                }
                else if (data.roomTypeIndex[i] == -3)
                {
                    newRoom = new BoardRoom(bg.boards[GameplayManager.instance.areaIndex].shopRoom, null, data.nextRoomCount[i], 0, false);
                }
                else
                {
                    newRoom = new BoardRoom(bg.boards[GameplayManager.instance.areaIndex].roomList[data.roomTypeIndex[i]], null, data.nextRoomCount[i], 0, false);
                }
            }
            bg.board[data.roomLevel[i]].Add(newRoom);
        }
        int aux = 0;
        for(int i = 0; i < bg.board.Count; i++)
        {
            for(int j = 0; j < bg.board[i].Count; j++)
            {
                for(int k = 0; k < bg.board[i][j].nextRoomsCount; k++)
                {
                    bg.board[i][j].nextRooms.Add(bg.board[i + 1][data.nextRoomIndex[aux]]);
                    aux++;
                }
            }
        }
        bg.InstantiateBoard(true);
    }

    public static void SaveBoard()
    {
        BoardData boardData = GetBoardData();
        string bSave = JsonUtility.ToJson(boardData);
        File.WriteAllText(Application.dataPath + "/boardSave.json", bSave);
    }

    public static void SavePlayer()
    {
        PlayerData pData = GetPlayerData();
        string pSave = JsonUtility.ToJson(pData);
        File.WriteAllText(Application.dataPath + "/playerSave.json", pSave);
    }

    public static void SaveUnlockedCards()
    {
        UnlockedCardsData ucData = GetUnlockedCardsData();
        string ucSave = JsonUtility.ToJson(ucData);
        File.WriteAllText(Application.dataPath + "/unlockedCardsSave.json", ucSave);
    }

    public static void SaveConfig()
    {
        string s = JsonUtility.ToJson(GameManager.instance.uiController.data);
        File.WriteAllText(Application.dataPath + "/configSave.json", s);
    }

    public static void LoadBoard(BoardGenerator board)
    {
        string path = Application.dataPath + "/boardSave.json";
        if (!File.Exists(path)) return;
        string bSave = File.ReadAllText(path);
        BoardData data = JsonUtility.FromJson<BoardData>(bSave);
        DataToBoard(board, data);
    }
    public static void LoadPlayer(Player player)
    {
        string path = Application.dataPath + "/playerSave.json";
        if (!File.Exists(path)) return;
        string pSave = File.ReadAllText(path);
        PlayerData data = JsonUtility.FromJson<PlayerData>(pSave);
        DataToPlayer(player, data);
    }

    public static void LoadUnlockedCards()
    {
        string path = Application.dataPath + "/unlockedCardsSave.json";
        if (!File.Exists(path)) return;
        string ucSave = File.ReadAllText(path);
        UnlockedCardsData data = JsonUtility.FromJson<UnlockedCardsData>(ucSave);
        DataToUnlockedCards(data);
    }

    public static void LoadConfig()
    {
        string path = Application.dataPath + "/configSave.json";
        if (!File.Exists(path)) return;
        string s = File.ReadAllText(path);
        GameManager.instance.uiController.data = JsonUtility.FromJson<ConfigData>(s);
    }

    public static void DeleteGameSaves()
    {
        string boardPath = Application.dataPath + "/boardSave.json";
        string playerPath = Application.dataPath + "/playerSave.json";
        if(File.Exists(boardPath))
        {
            File.Delete(Application.dataPath + "/boardSave.json");
        }
        if(File.Exists(playerPath))
        {
            File.Delete(Application.dataPath + "/playerSave.json");
        }
    }
}
