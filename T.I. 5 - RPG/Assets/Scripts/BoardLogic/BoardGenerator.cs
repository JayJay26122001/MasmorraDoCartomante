using UnityEngine;
using System.Collections.Generic;
public class BoardGenerator : MonoBehaviour
{
    public List<List<BoardRoom>> board = new List<List<BoardRoom>>();
    //public int levelsCount, maxBranches;
    //public BoardRoomSO startRoom, bossRoom;
    //public List<BoardRoomSO> roomList = new List<BoardRoomSO>();
    public List<BoardSO> boards = new List<BoardSO>();
    public List<ControlledProbability> typeProbabilities = new List<ControlledProbability>();
    public List<ControlledProbability> levelProbabilities = new List<ControlledProbability>();
    List<ControlledProbability> auxProb = new List<ControlledProbability>();
    //int battleProbability, mimicProbability, shopProbability, branchProbability, mergeProbability, proceedProbability;
    int nextRoomsCount, branchLevel = 1;
    //float battlePModifier = 1, mimicPModifier = 1, shopPModifier = 1, branchPModifier = 1, mergePModifier = 1, proceedPModifier = 1;
    bool willMerge;
    BoardRoom newRoom;
    public GameObject roomTest, boardBase, playerPiece, pieceAux;
    public LineRenderer lineRenderer;
    public Material shaderMat;
    float zOffset, xOffset;
    List<GameObject> lineObjects = new List<GameObject>();
    float animTimeStart;
    bool inAnimation, disappearing;
    public float animSpeed;
    public bool inMovement;
    Vector3 startPos;
    private void Start()
    {
        boardBase.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
        playerPiece.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
        startPos = transform.localPosition;
        inMovement = false;
        inAnimation = false;
        //GenerateBoard();
        /*for(int i = 0; i < board.Count; i++)
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
        }*/
        //InstantiateBoard();
    }

    public void ResetBoard()
    {
        //levelsCount = 4;
        board.Clear();
        lineObjects.Clear();
        foreach(Transform c in this.transform)
        {
            Destroy(c.gameObject);
        }
        /*foreach(ControlledProbability p in typeProbabilities)
        {
            p.ModifyMultiplier(1 - p.multiplier);
        }*/
        foreach(ControlledProbability p in levelProbabilities)
        {
            p.ModifyMultiplier(1 - p.multiplier);
        }
        transform.localPosition = startPos;
        boardBase.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
        playerPiece.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
        GenerateBoard();
    }

    public void GenerateBoard()
    {
        startPos = transform.localPosition;
        board.Clear();
        branchLevel = 1;
        //ChangeProbabilities(startRoom.roomName);
        typeProbabilities.Clear();
        int area = GameplayManager.instance.areaIndex;
        for (int i = 0; i < boards[area].startRoom.baseProbabilities.Count; i++)
        {
            typeProbabilities.Add(new ControlledProbability(boards[area].startRoom.baseProbabilities[i].type, boards[area].startRoom.baseProbabilities[i].probability, boards[area].startRoom.baseProbabilities[i].multiplier, boards[area].startRoom.baseProbabilities[i].minMult, boards[area].startRoom.baseProbabilities[i].maxMult, true));
        }
        newRoom = new BoardRoom(boards[area].startRoom, typeProbabilities, 1, 1, false);
        board.Insert(0, new List<BoardRoom>());
        board[0].Add(newRoom);
        GameplayManager.instance.currentRoom = newRoom;
        for(int i = 1; i < boards[area].levelsCount - 1; i++)
        {
            board.Insert(i, new List<BoardRoom>());
            for (int j = 0; j < board[i - 1].Count; j++)
            {
                if(!VerifyCanMerge(board[i - 1][j], i, j))
                {
                    int sum = 0;
                    foreach(ControlledProbability s in board[i - 1][j].nextRoomsProbabilities)
                    {
                        sum += s.probability;
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
                                aux += board[i - 1][j].nextRoomsProbabilities[k].probability;
                                if (rand < aux)
                                {
                                    if (!board[i - 1][j].CheckNextRooms(boards[area].roomList[k]) /*board[i - 1][j].type.possibleNextRooms[k]*/ )
                                    {
                                        if(board[i - 1][j].nextRoomsCount > 1)
                                        {
                                            branchLevel = board[i - 1][j].branchLevel + 1;
                                        }
                                        else
                                        {
                                            branchLevel = board[i - 1][j].branchLevel;
                                        }
                                        for(int l = 0; l < typeProbabilities.Count; l++)
                                        {
                                            typeProbabilities[l].ModifyProbability(board[i - 1][j].nextRoomsProbabilities[l].probability);
                                            typeProbabilities[l].ModifyMultiplier(board[i - 1][j].nextRoomsProbabilities[l].multiplier - typeProbabilities[l].multiplier);
                                        }
                                        ChangeProbabilities(boards[area].roomList[k] /*board[i - 1][j].type.possibleNextRooms[k]*/ );
                                        newRoom = new BoardRoom(/*board[i - 1][j].type.possibleNextRooms[k]*/ boards[area].roomList[k], typeProbabilities, nextRoomsCount, branchLevel, willMerge);
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
                    auxProb.Clear();
                    //nrProbabilities = new List<int> { board[i - 1][j].nextRoomsProbabilities[0].probability + board[i - 1][j + 1].nextRoomsProbabilities[0].probability, board[i - 1][j].nextRoomsProbabilities[1].probability + board[i - 1][j + 1].nextRoomsProbabilities[1].probability, board[i - 1][j].nextRoomsProbabilities[2] + board[i - 1][j + 1].nextRoomsProbabilities[2] };
                    for(int k = 0; k < typeProbabilities.Count; k++)
                    {
                        auxProb.Add(new ControlledProbability(typeProbabilities[k].type, board[i - 1][j].nextRoomsProbabilities[k].probability + board[i - 1][j + 1].nextRoomsProbabilities[k].probability, (board[i - 1][j].nextRoomsProbabilities[k].multiplier + board[i - 1][j + 1].nextRoomsProbabilities[k].multiplier) / 2, typeProbabilities[k].minMult, typeProbabilities[k].maxMult, true));
                    }
                    int sum = 0;
                    foreach (ControlledProbability s in auxProb)
                    {
                        sum += s.probability;
                    }
                    int rand = Random.Range(0, sum);
                    int aux = 0;
                    for (int k = 0; k < auxProb.Count; k++)
                    {
                        aux += auxProb[k].probability;
                        if (rand < aux)
                        {
                            branchLevel = board[i - 1][j].branchLevel - 1;
                            for (int l = 0; l < typeProbabilities.Count; l++)
                            {
                                typeProbabilities[l].ModifyProbability(board[i - 1][j].nextRoomsProbabilities[l].probability);
                                typeProbabilities[l].ModifyMultiplier(board[i - 1][j].nextRoomsProbabilities[l].multiplier - typeProbabilities[l].multiplier);
                            }
                            ChangeProbabilities(boards[area].roomList[k] /*board[i - 1][j].type.possibleNextRooms[k]*/);
                            newRoom = new BoardRoom(boards[area].roomList[k] /*board[i - 1][j].type.possibleNextRooms[k]*/, typeProbabilities, nextRoomsCount, branchLevel, willMerge);
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
        board.Insert(boards[area].levelsCount - 1, new List<BoardRoom>());
        newRoom = new BoardRoom(boards[area].bossRoom);
        foreach(BoardRoom r in board[boards[area].levelsCount - 2])
        {
            r.nextRoomsCount = 1;
            r.nextRooms.Add(newRoom);
        }
        board[boards[area].levelsCount - 1].Add(newRoom);
        InstantiateBoard();
    }

    public bool VerifyCanMerge(BoardRoom room, int i, int j)
    {
        if(!room.wantsToMerge)
        {
            if(board[i - 1].Count > j + 1)
            {
                if(board[i - 1][j + 1].wantsToMerge && board[i - 1][j + 1].branchLevel == room.branchLevel && room.nextRoomsCount == 1)
                {
                    room.wantsToMerge = true;
                    //room.nextRoomsCount = 1;
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
            if(board[i - 1].Count > j + 1 && board[i - 1][j + 1].branchLevel == room.branchLevel && board[i - 1][j + 1].nextRoomsCount == 1)
            {
                //board[i - 1][j + 1].nextRoomsCount = 1;
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

    /*public void ChangeProbabilities(string s)
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
                proceedProbability = (int)(25 * proceedPModifier);
                branchProbability = (int)(75 * branchPModifier);
            }
            else if(branchLevel == maxBranches)
            {
                branchProbability = 0;
                mergeProbability = (int)(40 * proceedPModifier);
                proceedProbability = (int)(60 * proceedPModifier);
            }
            else
            {
                mergeProbability = (int)(20 * proceedPModifier);
                proceedProbability = (int)(40 * proceedPModifier);
                branchProbability = (int)(40 * branchPModifier);
            }
            int sum = mergeProbability + proceedProbability + branchProbability;
            int rand = Random.Range(0, sum);
            if(rand <= mergeProbability)
            {
                nextRoomsCount = 1;
                willMerge = true;
                mergePModifier = Mathf.Clamp(mergePModifier - 0.2f, 0, 1.5f);
                branchPModifier += Mathf.Clamp(branchPModifier + 0.3f, 0, 1.5f);
                proceedPModifier += Mathf.Clamp(proceedPModifier + 0.1f, 0.1f, 2f);
            }
            else if(rand <= mergeProbability + proceedProbability)
            {
                nextRoomsCount = 1;
                willMerge = false;
                mergePModifier = Mathf.Clamp(mergePModifier + 0.1f, 0, 1.5f);
                branchPModifier += Mathf.Clamp(branchPModifier + 0.2f, 0, 1.5f);
                proceedPModifier += Mathf.Clamp(proceedPModifier - 0.1f, 0.1f, 2f);
            }
            else
            {
                nextRoomsCount = 2;
                willMerge = false;
                mergePModifier = Mathf.Clamp(mergePModifier + 0.2f, 0, 1.5f);
                branchPModifier += Mathf.Clamp(branchPModifier - 0.2f, 0, 1.5f);
                proceedPModifier += Mathf.Clamp(proceedPModifier + 0.1f, 0.1f, 2f);
            }
        }
        nrProbabilities = new List<int> { battleProbability, mimicProbability, shopProbability };
    }*/
    public void ChangeProbabilities(BoardRoomSO r)
    {
        for(int i = 0; i < typeProbabilities.Count; i++)
        {
            typeProbabilities[i].ModifyProbability(r.baseProbabilities[i].probability);
            if(string.Compare(r.roomName, typeProbabilities[i].type) == 0)
            {
                if(string.Compare(r.roomName, "Battle") == 0)
                {
                    typeProbabilities[i].ModifyMultiplier(-0.1f);
                }
                else
                {
                    typeProbabilities[i].ModifyMultiplier(-typeProbabilities[i].multiplier);
                }
            }
            else
            {
                typeProbabilities[i].ModifyMultiplier(0.1f);
            }
        }
        if(branchLevel == 1)
        {
            levelProbabilities[0].ModifyProbability(0);
            levelProbabilities[1].ModifyProbability(25);
            levelProbabilities[2].ModifyProbability(75);
        }
        else if(branchLevel == boards[GameplayManager.instance.areaIndex].maxBranches)
        {
            levelProbabilities[0].ModifyProbability(35);
            levelProbabilities[1].ModifyProbability(65);
            levelProbabilities[2].ModifyProbability(0);
        }
        else
        {
            levelProbabilities[0].ModifyProbability(20);
            levelProbabilities[1].ModifyProbability(35);
            levelProbabilities[2].ModifyProbability(45);
        }
        int sum = 0;
        foreach(ControlledProbability p in levelProbabilities)
        {
            sum += p.probability;
        }
        int rand = Random.Range(0, sum);
        if(rand <= levelProbabilities[0].probability)
        {
            nextRoomsCount = 1;
            willMerge = true;
            levelProbabilities[0].ModifyMultiplier(-0.2f);
            levelProbabilities[2].ModifyMultiplier(0.3f);
        }
        else if(rand <= levelProbabilities[0].probability + levelProbabilities[1].probability)
        {
            nextRoomsCount = 1;
            willMerge = false;
            levelProbabilities[0].ModifyMultiplier(0.1f);
            levelProbabilities[2].ModifyMultiplier(0.2f);
        }
        else
        {
            nextRoomsCount = 2;
            willMerge = false;
            levelProbabilities[0].ModifyMultiplier(0.2f);
            levelProbabilities[2].ModifyMultiplier(-0.1f);
        }
    }

    public void InstantiateBoard()
    {
        boardBase.GetComponent<MeshRenderer>().material.color = new Color32(50, 100, 150, 255);
        playerPiece.GetComponent<MeshRenderer>().material.color = new Color32(0, 100, 0, 255);
        int roomCount;
        zOffset = 0;
        xOffset = 0;
        GameObject room = Instantiate(roomTest, transform.position, roomTest.transform.rotation, this.transform);
        room.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
        room.GetComponent<MeshRenderer>().material.color = board[0][0].type.testColor;
        room.GetComponent<RoomObject>().outline.GetComponent<MeshRenderer>().material.SetColor("_Color", board[0][0].type.testColor);
        board[0][0].roomObject = room;
        room.GetComponent<RoomObject>().roomRef = board[0][0];
        playerPiece.transform.position = room.transform.position + Vector3.up * 0.2f;
        for (int i = 1; i < board.Count; i++)
        {
            zOffset += 20;
            roomCount = 0;
            if(board[i].Count % 2 == 1)
            {
                xOffset = 0;
                room = Instantiate(roomTest, new Vector3(0, 0, zOffset * transform.localScale.z) + transform.position, roomTest.transform.rotation, this.transform);
                room.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
                room.GetComponent<RoomObject>().icon.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
                room.GetComponent<MeshRenderer>().material.color = board[i][Mathf.FloorToInt(board[i].Count/2)].type.testColor;
                room.GetComponent<RoomObject>().outline.GetComponent<MeshRenderer>().material.SetColor("_Color", board[i][Mathf.FloorToInt(board[i].Count / 2)].type.testColor);
                board[i][Mathf.FloorToInt(board[i].Count / 2)].roomObject = room;
                room.GetComponent<RoomObject>().roomRef = board[i][Mathf.FloorToInt(board[i].Count / 2)];
                roomCount++;
                while(roomCount < board[i].Count)
                {
                    xOffset += 20;
                    room = Instantiate(roomTest, new Vector3(xOffset * transform.localScale.x, 0, zOffset * transform.localScale.z) + transform.position, roomTest.transform.rotation, this.transform);
                    room.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
                    room.GetComponent<RoomObject>().icon.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
                    room.GetComponent<MeshRenderer>().material.color = board[i][Mathf.FloorToInt(board[i].Count / 2) + (int)(xOffset * transform.localScale.x / (20 * transform.localScale.x))].type.testColor;
                    room.GetComponent<RoomObject>().outline.GetComponent<MeshRenderer>().material.SetColor("_Color", board[i][Mathf.FloorToInt(board[i].Count / 2) + (int)(xOffset * transform.localScale.x / (20 * transform.localScale.x))].type.testColor);
                    board[i][Mathf.FloorToInt(board[i].Count / 2) + (int)(xOffset * transform.localScale.x / (20 * transform.localScale.x))].roomObject = room;
                    room.GetComponent<RoomObject>().roomRef = board[i][Mathf.FloorToInt(board[i].Count / 2) + (int)(xOffset * transform.localScale.x / (20 * transform.localScale.x))];
                    room = Instantiate(roomTest, new Vector3(-xOffset * transform.localScale.x, 0, zOffset * transform.localScale.z) + transform.position, roomTest.transform.rotation, this.transform);
                    room.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
                    room.GetComponent<RoomObject>().icon.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
                    room.GetComponent<MeshRenderer>().material.color = board[i][Mathf.FloorToInt(board[i].Count / 2) - (int)(xOffset * transform.localScale.x / (20 * transform.localScale.x))].type.testColor;
                    room.GetComponent<RoomObject>().outline.GetComponent<MeshRenderer>().material.SetColor("_Color", board[i][Mathf.FloorToInt(board[i].Count / 2) - (int)(xOffset * transform.localScale.x / (20 * transform.localScale.x))].type.testColor);
                    board[i][Mathf.FloorToInt(board[i].Count / 2) - (int)(xOffset * transform.localScale.x / (20 * transform.localScale.x))].roomObject = room;
                    room.GetComponent<RoomObject>().roomRef = board[i][Mathf.FloorToInt(board[i].Count / 2) - (int)(xOffset * transform.localScale.x / (20 * transform.localScale.x))];
                    roomCount += 2;
                }
            }
            else
            {
                xOffset = -10;
                while (roomCount < board[i].Count)
                {
                    xOffset += 20;
                    room = Instantiate(roomTest, new Vector3(xOffset * transform.localScale.x, 0, zOffset * transform.localScale.z) + transform.position, roomTest.transform.rotation, this.transform);
                    room.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
                    room.GetComponent<RoomObject>().icon.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
                    room.GetComponent<MeshRenderer>().material.color = board[i][Mathf.FloorToInt(board[i].Count / 2) + (int)((xOffset - 10) * transform.localScale.x / (20 * transform.localScale.x))].type.testColor;
                    room.GetComponent<RoomObject>().outline.GetComponent<MeshRenderer>().material.SetColor("_Color", board[i][Mathf.FloorToInt(board[i].Count / 2) + (int)((xOffset - 10) * transform.localScale.x / (20 * transform.localScale.x))].type.testColor);
                    board[i][Mathf.FloorToInt(board[i].Count / 2) + (int)((xOffset - 10) * transform.localScale.x / (20 * transform.localScale.x))].roomObject = room;
                    room.GetComponent<RoomObject>().roomRef = board[i][Mathf.FloorToInt(board[i].Count / 2) + (int)((xOffset - 10) * transform.localScale.x / (20 * transform.localScale.x))];
                    room = Instantiate(roomTest, new Vector3(-xOffset * transform.localScale.x, 0, zOffset * transform.localScale.z) + transform.position, roomTest.transform.rotation, this.transform);
                    room.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
                    room.GetComponent<RoomObject>().icon.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", 1);
                    room.GetComponent<MeshRenderer>().material.color = board[i][Mathf.FloorToInt(board[i].Count / 2) - 1 - (int)((xOffset - 10) * transform.localScale.x / (20 * transform.localScale.x))].type.testColor;
                    room.GetComponent<RoomObject>().outline.GetComponent<MeshRenderer>().material.SetColor("_Color", board[i][Mathf.FloorToInt(board[i].Count / 2) - 1 - (int)((xOffset - 10) * transform.localScale.x / (20 * transform.localScale.x))].type.testColor);
                    board[i][Mathf.FloorToInt(board[i].Count / 2) - 1 - (int)((xOffset - 10) * transform.localScale.x / (20 * transform.localScale.x))].roomObject = room;
                    room.GetComponent<RoomObject>().roomRef = board[i][Mathf.FloorToInt(board[i].Count / 2) - 1 - (int)((xOffset - 10) * transform.localScale.x / (20 * transform.localScale.x))];
                    roomCount += 2;
                }
            }

            foreach(BoardRoom r1 in board[i - 1])
            {
                foreach(BoardRoom r2 in r1.nextRooms)
                {
                    lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                    lineRenderer.transform.SetParent(this.transform);
                    lineRenderer.startColor = Color.black;
                    lineRenderer.endColor = Color.black;
                    lineRenderer.startWidth = 0.9f * transform.localScale.x;
                    lineRenderer.endWidth = 0.9f * transform.localScale.x;
                    lineRenderer.positionCount = 2;
                    lineRenderer.useWorldSpace = true;
                    lineRenderer.material = shaderMat;
                    lineRenderer.material.SetFloat("_DisappearTime", 1);
                    lineRenderer.SetPositions(new Vector3[] { r1.roomObject.transform.position - Vector3.up * transform.localScale.x, r2.roomObject.transform.position - Vector3.up * transform.localScale.x });
                    lineRenderer.useWorldSpace = false;
                    lineRenderer.sortingOrder = -1;
                    lineObjects.Add(lineRenderer.gameObject);
                }
            }
        }
    }

    public void AnimateBoard(bool disappear)
    {
        disappearing = disappear;
        animTimeStart = Time.time;
        inAnimation = true;
    }

    float t;
    private void Update()
    {
        if(inAnimation)
        {
            if(disappearing)
            {
                t = Mathf.Clamp((Time.time - animTimeStart) * animSpeed, 0, 1);
            }
            else
            {
                t = Mathf.Clamp(1 - ((Time.time - animTimeStart) * animSpeed), 0, 1);
            }
            boardBase.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", t);
            playerPiece.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", t);
            for (int i = 0; i < boards[GameplayManager.instance.areaIndex].levelsCount; i++)
            {
                foreach (BoardRoom r in board[i])
                {
                    r.roomObject.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", t);
                    r.roomObject.GetComponent<RoomObject>().icon.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_DisappearTime", t);
                }
            }
            foreach (GameObject go in lineObjects)
            {
                go.GetComponent<LineRenderer>().material.SetFloat("_DisappearTime", t);
            }
            if((t >= 1 && disappearing) || (t <= 0 && !disappearing))
            {
                inAnimation = false;
            }
        }
    }

    public void MovementChange(bool isMoving)
    {
        inMovement = isMoving;
    }
}
