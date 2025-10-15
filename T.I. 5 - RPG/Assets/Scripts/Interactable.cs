using UnityEngine;
using System.Collections.Generic;
using System;
public class Interactable : MonoBehaviour
{
    public List<ControlUI> interactions = new List<ControlUI>();

    private void OnMouseEnter()
    {
        foreach(ControlUI c in interactions)
        {
            GameManager.instance.uiController.ShowCommandPopup(c);
        }
    }

    private void OnMouseExit()
    {
        foreach(ControlUI c in interactions)
        {
            GameManager.instance.uiController.HideCommandPopup(c);
        }
    }
}

[Serializable]
public class ControlUI
{
    public Sprite image;
    public string command;
}
