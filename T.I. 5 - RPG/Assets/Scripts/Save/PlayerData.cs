using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class PlayerData
{
    public List<int> unlockedCards;
    public List<int> playerDeckCards;
    public Vector3 boardPos;
    public Vector3 piecePos;
    public int area;
    public int money;
    public int hp;
}
