using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongData
{
    public static SongData Instance;
    public static Metadata Song;
    public static Difficulties Difficulty;
    public static Sprite Backdrop;
    public static AudioClip AudioClip;
    
    public SongData()
    {
        if(Instance != null)
        {
            Instance = this;
        }
    }
}
