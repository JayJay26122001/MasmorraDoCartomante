using UnityEngine;
using System.Collections.Generic;
public class BoardGenerator : MonoBehaviour
{
    public List<BoardRoomSO> board = new List<BoardRoomSO>();
    public int roomCount;
    public BoardRoomSO startRoom, bossRoom;
    private void Start()
    {
        GenerateBoard();
        foreach(BoardRoomSO room in board)
        {
            Debug.Log(room.roomName);
        }
    }

    public void GenerateBoard()
    {
        board.Clear();
        BoardRoomSO start = Instantiate(startRoom);
        board.Insert(0, start);
        for(int i = 1; i < roomCount - 1; i++)
        {
            while(board[i - 1].nextRooms.Count < board[i - 1].nextRoomsCount)
            {
                int sum = 0;
                foreach(int s in board[i - 1].nextRoomsProbabilities)
                {
                    sum += s;
                }
                bool success = false;
                while(!success)
                {
                    int rand = Random.Range(0, sum);
                    int aux = 0;
                    for(int j = 0; j < board[i-1].nextRoomsProbabilities.Count; j++)
                    {
                        aux += board[i - 1].nextRoomsProbabilities[j];
                        if(rand < aux)
                        {
                            if(!board[i - 1].nextRooms.Contains(board[i - 1].possibleNextRooms[j]))
                            {
                                BoardRoomSO auxRoom = Instantiate(board[i - 1].possibleNextRooms[j]);
                                board[i - 1].nextRooms.Add(auxRoom);
                                board.Insert(i,auxRoom);
                                success = true;
                            }
                            break;
                        }
                    }
                }
            }
        }
        BoardRoomSO boss = Instantiate(bossRoom);
        board[roomCount - 2].nextRoomsCount = 1;
        board[roomCount - 2].nextRooms.Add(boss);
        board.Insert(roomCount - 1, boss);
    }
}
