using UnityEngine;
using System.Collections.Generic;
using System;
public class Interactable : MonoBehaviour
{
    public List<ControlUI> interactions = new List<ControlUI>();
    bool shown = false;
    private void OnMouseOver()
    {
        ShowInteractions();
    }

    private void OnMouseExit()
    {
        HideInteractions();
    }

    public void ShowInteractions()
    {
        if(!shown)
        {
            shown = true;
            foreach (ControlUI c in interactions)
            {
                GameManager.instance.uiController.ShowCommandPopup(c);
            }
        }
    }

    public void HideInteractions()
    {
        shown = false;
        foreach (ControlUI c in interactions)
        {
            GameManager.instance.uiController.HideCommandPopup(c);
        }
    }

    private void OnDisable()
    {
        HideInteractions();
    }
}

[Serializable]
public class ControlUI
{
    public Sprite image;
    public string command;
}
