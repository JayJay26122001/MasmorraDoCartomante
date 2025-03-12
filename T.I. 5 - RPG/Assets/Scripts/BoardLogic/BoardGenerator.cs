using UnityEngine;
using System.Collections.Generic;
public class BoardGenerator : MonoBehaviour
{
    public List<List<BoardRoom>> board = new List<List<BoardRoom>>();
    public int levelsCount;
    public BoardRoomSO startRoom, bossRoom;
    int battleProbability, mimicProbability, shopProbability, choiceProbability, nextRoomsCount;
    float battlePModifier = 1, mimicPModifier = 1, shopPModifier = 1, choicePModifier = 1;
    List<int> probabilities = new List<int>();
    BoardRoom newRoom;
    public GameObject roomTest;
    float zOffset, xOffset;
    private void Start()
    {
        GenerateBoard();
        for(int i = 0; i < board.Count; i++)
        {
            foreach(BoardRoom r in board[i])
            {
                string s = "Level: " + i + " /// Name: " + r.type.roomName + " /// Nexts: ";
                foreach(BoardRoom r2 in r.nextRooms)
                {
                    s += r2.type.roomName + " / ";
                }
                Debug.Log(s);
            }
        }
        InstantiateBoard();
    }

    public void GenerateBoard()
    {
        board.Clear();
        ChangeProbabilities(startRoom.roomName);
        newRoom = new BoardRoom(startRoom, probabilities, 1);
        board.Insert(0, new List<BoardRoom>());
        board[0].Add(newRoom);
        for(int i = 1; i < levelsCount - 1; i++)
        {
            board.Insert(i, new List<BoardRoom>());
            for (int j = 0; j < board[i - 1].Count; j++)
            {
                int sum = 0;
                foreach(int s in board[i - 1][j].nextRoomsProbabilities)
                {
                    sum += s;
                }
                while (board[i - 1][j].nextRooms.Count < board[i - 1][j].nextRoomsCount)
                {
                    bool success = false;
                    while (!success)
                    {
                        int rand = Random.Range(0, sum);
                        int aux = 0;
                        for (int k = 0; k < board[i - 1][j].nextRoomsProbabilities.Count; k++)
                        {
                            aux += board[i - 1][j].nextRoomsProbabilities[k];
                            if (rand < aux)
                            {
                                if (!board[i - 1][j].CheckNextRooms(board[i - 1][j].type.possibleNextRooms[k]))
                                {
                                    ChangeProbabilities(board[i - 1][j].type.possibleNextRooms[k].roomName);
                                    newRoom = new BoardRoom(board[i - 1][j].type.possibleNextRooms[k], probabilities, nextRoomsCount);
                                    board[i - 1][j].nextRooms.Add(newRoom);
                                    board[i].Add(newRoom);
                                    success = true;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        board.Insert(levelsCount - 1, new List<BoardRoom>());
        newRoom = new BoardRoom(bossRoom);
        foreach(BoardRoom r in board[levelsCount - 2])
        {
            r.nextRoomsCount = 1;
            r.nextRooms.Add(newRoom);
        }
        board[levelsCount - 1].Add(newRoom);
    }

    public void ChangeProbabilities(string s)
    {
        if(string.Compare(s, "Start") == 0)
        {
            battleProbability = 60;
            mimicProbability = 40;
            shopProbability = 0;
            choiceProbability = 30;
        }
        else
        {
            if(string.Compare(s, "Battle") == 0)
            {
                battleProbability = (int)(10 * battlePModifier);
                mimicProbability = (int)(40 * mimicPModifier);
                shopProbability = (int)(50 * shopPModifier);
                battlePModifier = Mathf.Clamp(battlePModifier - 0.1f, 0.1f, 2f);
                mimicPModifier = Mathf.Clamp(mimicPModifier + 0.1f, 0.1f, 2f);
                shopPModifier = Mathf.Clamp(shopPModifier + 0.1f, 0, 1.5f);
            }
            else if(string.Compare(s, "Mimic") == 0)
            {
                battleProbability = (int)(45 * battlePModifier);
                mimicProbability = (int)(10 * mimicPModifier);
                shopProbability = (int)(45 * shopPModifier);
                battlePModifier = Mathf.Clamp(battlePModifier + 0.1f, 0.1f, 2f);
                mimicPModifier = Mathf.Clamp(mimicPModifier - 0.1f, 0.1f, 2f);
                shopPModifier = Mathf.Clamp(shopPModifier + 0.1f, 0, 1.5f);
            }
            else if(string.Compare(s, "Shop") == 0)
            {
                battleProbability = (int)(55 * battlePModifier);
                mimicProbability = (int)(45 * mimicPModifier);
                shopProbability = 0;
                battlePModifier = Mathf.Clamp(battlePModifier + 0.1f, 0.1f, 2f);
                mimicPModifier = Mathf.Clamp(mimicPModifier + 0.1f, 0.1f, 2f);
                shopPModifier = 0;
            }
            int rand = Random.Range(0, 100);
            if(rand < choiceProbability * choicePModifier)
            {
                nextRoomsCount = 2;
                choicePModifier -= 0.2f;
            }
            else
            {
                nextRoomsCount = 1;
                choicePModifier += 0.1f;
            }
        }
        probabilities = new List<int> { battleProbability, mimicProbability, shopProbability };
    }

    public void InstantiateBoard()
    {
        zOffset = 0;
        xOffset = 0;
        GameObject room = Instantiate(roomTest, Vector3.zero, Quaternion.identity, this.transform);
        room.GetComponent<MeshRenderer>().material.color = board[0][0].type.testColor;
        board[0][0].roomObject = room;
        for(int i = 0; i < board.Count - 1; i++)
        {
            zOffset += 10;
            foreach (BoardRoom r in board[i])
            {
                if(r.nextRoomsCount > 1)
                {
                    xOffset = -10;
                    Instantiate(roomTest, new Vector3(r.roomObject.transform.position.x + xOffset, 0, r.roomObject.transform.position.z), Quaternion.identity, this.transform);
                    room = Instantiate(roomTest, new Vector3(r.roomObject.transform.position.x + xOffset, 0, zOffset), Quaternion.identity, this.transform);
                    room.GetComponent<MeshRenderer>().material.color = r.nextRooms[0].type.testColor;
                    r.nextRooms[0].roomObject = room;
                    xOffset = 10;
                    Instantiate(roomTest, new Vector3(r.roomObject.transform.position.x + xOffset, 0, r.roomObject.transform.position.z), Quaternion.identity, this.transform);
                    room = Instantiate(roomTest, new Vector3(r.roomObject.transform.position.x + xOffset, 0, zOffset), Quaternion.identity, this.transform);
                    room.GetComponent<MeshRenderer>().material.color = r.nextRooms[1].type.testColor;
                    r.nextRooms[1].roomObject = room;
                    xOffset = 0;
                }
                else
                {
                    room = Instantiate(roomTest, new Vector3(r.roomObject.transform.position.x + xOffset, 0, zOffset), Quaternion.identity, this.transform);
                    room.GetComponent<MeshRenderer>().material.color = r.nextRooms[0].type.testColor;
                    r.nextRooms[0].roomObject = room;
                }
            }
        }
    }
}
