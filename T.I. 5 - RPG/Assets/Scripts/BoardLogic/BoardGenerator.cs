using UnityEngine;
using System.Collections.Generic;
public class BoardGenerator : MonoBehaviour
{
    public List<List<BoardRoom>> board = new List<List<BoardRoom>>();
    public int levelsCount, maxBranches;
    public BoardRoomSO startRoom, bossRoom;
    int battleProbability, mimicProbability, shopProbability, branchProbability, mergeProbability, proceedProbability, nextRoomsCount, branchLevel = 1;
    float battlePModifier = 1, mimicPModifier = 1, shopPModifier = 1, branchPModifier = 1, mergePModifier = 1, proceedPModifier = 1;
    bool willMerge;
    List<int> nrProbabilities = new List<int>();
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
        //InstantiateBoard();
    }

    public void GenerateBoard()
    {
        board.Clear();
        branchLevel = 1;
        ChangeProbabilities(startRoom.roomName);
        newRoom = new BoardRoom(startRoom, nrProbabilities, 1, 1, false);
        board.Insert(0, new List<BoardRoom>());
        board[0].Add(newRoom);
        for(int i = 1; i < levelsCount - 1; i++)
        {
            board.Insert(i, new List<BoardRoom>());
            for (int j = 0; j < board[i - 1].Count; j++)
            {
                if(!VerifyCanMerge(board[i - 1][j], i, j))
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
                                        if(board[i - 1][j].nextRoomsCount > 1)
                                        {
                                            branchLevel = board[i - 1][j].branchLevel + 1;
                                        }
                                        else
                                        {
                                            branchLevel = board[i - 1][j].branchLevel;
                                        }
                                        ChangeProbabilities(board[i - 1][j].type.possibleNextRooms[k].roomName);
                                        newRoom = new BoardRoom(board[i - 1][j].type.possibleNextRooms[k], nrProbabilities, nextRoomsCount, branchLevel, willMerge);
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
                else
                {
                    nrProbabilities = new List<int> { board[i - 1][j].nextRoomsProbabilities[0] + board[i - 1][j + 1].nextRoomsProbabilities[0], board[i - 1][j].nextRoomsProbabilities[1] + board[i - 1][j + 1].nextRoomsProbabilities[1] , board[i - 1][j].nextRoomsProbabilities[2] + board[i - 1][j + 1].nextRoomsProbabilities[2] };
                    int sum = 0;
                    foreach (int s in nrProbabilities)
                    {
                        sum += s;
                    }
                    int rand = Random.Range(0, sum);
                    int aux = 0;
                    for (int k = 0; k < nrProbabilities.Count; k++)
                    {
                        aux += nrProbabilities[k];
                        if (rand < aux)
                        {
                            branchLevel = board[i - 1][j].branchLevel - 1;
                            ChangeProbabilities(board[i - 1][j].type.possibleNextRooms[k].roomName);
                            newRoom = new BoardRoom(board[i - 1][j].type.possibleNextRooms[k], nrProbabilities, nextRoomsCount, branchLevel, willMerge);
                            board[i - 1][j].nextRooms.Add(newRoom);
                            board[i - 1][j + 1].nextRooms.Add(newRoom);
                            board[i].Add(newRoom);
                            j++;
                            break;
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

    public bool VerifyCanMerge(BoardRoom room, int i, int j)
    {
        if(!room.wantsToMerge)
        {
            if(board[i - 1].Count > j + 1)
            {
                if(board[i - 1][j + 1].wantsToMerge && board[i - 1][j + 1].branchLevel == room.branchLevel)
                {
                    room.wantsToMerge = true;
                    room.nextRoomsCount = 1;
                    return true;
                }
                else
                {
                    board[i - 1][j + 1].wantsToMerge = false;
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            if(board[i - 1].Count > j + 1 && board[i - 1][j + 1].branchLevel == room.branchLevel)
            {
                board[i - 1][j + 1].nextRoomsCount = 1;
                board[i - 1][j + 1].wantsToMerge = true;
                return true;
            }
            else
            {
                room.wantsToMerge = false;
                return false;
            }
        }
    }

    public void ChangeProbabilities(string s)
    {
        if(string.Compare(s, "Start") == 0)
        {
            battleProbability = 60;
            mimicProbability = 40;
            shopProbability = 0;
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

            if(branchLevel == 1)
            {
                mergeProbability = 0;
                proceedProbability = (int)(70 * proceedPModifier);
                branchProbability = (int)(30 * branchPModifier);
            }
            else if(branchLevel == maxBranches)
            {
                branchProbability = 0;
                mergeProbability = (int)(40 * proceedPModifier);
                proceedProbability = (int)(60 * proceedPModifier);
            }
            else
            {
                mergeProbability = (int)(30 * proceedPModifier);
                proceedProbability = (int)(40 * proceedPModifier);
                branchProbability = (int)(30 * branchPModifier);
            }
            int sum = mergeProbability + proceedProbability + branchProbability;
            int rand = Random.Range(0, sum);
            if(rand <= mergeProbability)
            {
                nextRoomsCount = 1;
                willMerge = true;
                mergePModifier = Mathf.Clamp(mergePModifier - 0.2f, 0, 1.5f);
                branchPModifier += Mathf.Clamp(branchPModifier + 0.1f, 0, 1.5f);
                proceedPModifier += Mathf.Clamp(proceedPModifier + 0.1f, 0.1f, 2f);
            }
            else if(rand <= mergeProbability + proceedProbability)
            {
                nextRoomsCount = 1;
                willMerge = false;
                mergePModifier = Mathf.Clamp(mergePModifier + 0.1f, 0, 1.5f);
                branchPModifier += Mathf.Clamp(branchPModifier + 0.1f, 0, 1.5f);
                proceedPModifier += Mathf.Clamp(proceedPModifier - 0.1f, 0.1f, 2f);
            }
            else
            {
                nextRoomsCount = 2;
                willMerge = false;
                mergePModifier = Mathf.Clamp(mergePModifier + 0.2f, 0, 1.5f);
                branchPModifier += Mathf.Clamp(branchPModifier - 0.3f, 0, 1.5f);
                proceedPModifier += Mathf.Clamp(proceedPModifier + 0.1f, 0.1f, 2f);
            }
        }
        nrProbabilities = new List<int> { battleProbability, mimicProbability, shopProbability };
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
