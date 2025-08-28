using UnityEngine;
using System;
public class RoomObject : MonoBehaviour
{
    public BoardRoom roomRef;
    public MeshFilter icon;

    private void Start()
    {
        icon = this.transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
        if(roomRef.type.iconMesh != null)
        {
            icon.mesh = roomRef.type.iconMesh;
            icon.gameObject.GetComponent<MeshRenderer>().material.color = Color.black;
        }
        else
        {
            icon.gameObject.SetActive(false);
        }
    }

    public void OnMouseDown()
    {
        if(GameplayManager.instance.currentRoom.nextRooms.Contains(roomRef) && GameplayManager.instance.InputActive && !GameManager.instance.uiController.gamePaused && !GameplayManager.instance.bg.inMovement)
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
            //GameplayManager.instance.MoveBoard(act);
            GameplayManager.instance.MovePiece(act, this.transform.position + Vector3.up * 1.2f);
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
