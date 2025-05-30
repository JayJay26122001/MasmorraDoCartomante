using UnityEngine;
using System;
public class RoomObject : MonoBehaviour
{
    public BoardRoom roomRef;

    public void OnMouseDown()
    {
        if(GameplayManager.instance.currentRoom.nextRooms.Contains(roomRef) && GameplayManager.instance.InputActive)
        {
            GameplayManager.instance.PauseInput(2);
            Action act = new Action(() => { return; });
            switch(roomRef.type.roomName)
            {
                case "Boss":
                    GameplayManager.instance.figtingBoss = true;
                    act = new Action(() => { SwitchToBattle(); });
                    break;
                case "Battle":
                case "Mimic":
                    GameplayManager.instance.figtingBoss = false;
                    act = new Action(() => { SwitchToBattle(); });
                    break;
                case "Shop":
                    act = new Action(() => { SwitchToShop(); });
                    break;
            }
            GameplayManager.instance.MoveBoard(act);
            GameplayManager.instance.currentRoom = roomRef;
        }
    }

    public void SwitchToBattle()
    {
        GameplayManager.instance.PlayCutscene(1);
    }
    public void SwitchToShop()
    {
        GameplayManager.instance.PlayCutscene(2);
    }
}
