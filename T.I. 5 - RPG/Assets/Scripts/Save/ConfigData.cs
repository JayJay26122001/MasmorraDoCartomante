using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ConfigData
{
    public int selectedRes, screenMode;
    public float master, music, sfx;
    public ConfigData(int selectedRes, int screenMode, float master, float music, float sfx)
    {
        this.selectedRes = selectedRes;
        this.screenMode = screenMode;
        this.master = master;
        this.music = music;
        this.sfx = sfx;
    }
}
