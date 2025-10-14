using UnityEngine;
using System;
public class RoomObject : MonoBehaviour
{
    public BoardRoom roomRef;
    public MeshFilter icon;
    public GameObject outline;
    private void Start()
    {
        outline = this.transform.GetChild(1).gameObject;
        outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.5f);
        outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.5f);
        outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0f);
        outline.SetActive(false);
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
            outline.SetActive(false);
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
                case "Heal":
                    act = new Action(() => { SwitchToHeal(); });
                    break;
                case "Trash":
                    act = new Action(() => { SwitchToTrash(); });
                    break;
                case "Stamp":
                    act = new Action(() => { SwitchToStamp(); });
                    break;
            }
            //GameplayManager.instance.MoveBoard(act);
            GameplayManager.instance.MovePiece(act, this.transform.position + Vector3.up * 0.2f);
            GameplayManager.instance.currentRoom = roomRef;
        }
    }
    public void OnMouseOver()
    {
        if (GameplayManager.instance.currentRoom.nextRooms.Contains(roomRef) && GameplayManager.instance.InputActive && !GameManager.instance.uiController.gamePaused && !GameplayManager.instance.bg.inMovement)
        {
            outline.SetActive(true);
        }
    }

    public void OnMouseExit()
    {
        outline.SetActive(false);
    }

    public void SwitchToBattle()
    {
        GameplayManager.instance.PlayCutscene(1);
    }
    public void SwitchToShop()
    {
        GameplayManager.instance.PlayCutscene(2);
    }
    public void SwitchToHeal()
    {
        GameplayManager.instance.PlayCutscene(8);
    }
    public void SwitchToTrash()
    {
        GameplayManager.instance.PlayCutscene(10);
    }
    public void SwitchToStamp()
    {
        GameplayManager.instance.PlayCutscene(12);
    }
}
