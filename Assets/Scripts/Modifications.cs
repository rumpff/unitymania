using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


[Serializable]
public class Modifications
{
    private static Modifications m_instance;
    public static Modifications Instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = ReadData();
            }

            return m_instance;
        }
    }

    public int ResolutionIndex = 14;
    public int Fullscreen = 1;
    public int FPSLimit = 60;

    public int MasterVolume = 100;
    public int MusicVolume = 100;
    public int SFXVolume = 100;

    public int MasterDistortion = 0;
    public int MasterBassBoost = 0;
    public int MusicPitch = 100;

    public int ScrollSpeed = 8;

    public int JudgeDiff = 3;

    public Dictionary<KeyTypes, KeyCode> DefaultKeys = new Dictionary<KeyTypes, KeyCode>()
    {
        { KeyTypes.StartKey, KeyCode.Return },
        { KeyTypes.ExitKey, KeyCode.Escape },
        { KeyTypes.LeftKey, KeyCode.LeftArrow },
        { KeyTypes.RightKey, KeyCode.RightArrow },
        { KeyTypes.UpKey, KeyCode.UpArrow },
        { KeyTypes.DownKey, KeyCode.DownArrow },
    };

    public Dictionary<KeyTypes, KeyCode> CustomKeys = new Dictionary<KeyTypes, KeyCode>()
    {
        { KeyTypes.StartKey, KeyCode.None },
        { KeyTypes.RestartKey, KeyCode.None },
        { KeyTypes.ExitKey, KeyCode.None },
        { KeyTypes.LeftKey, KeyCode.E },
        { KeyTypes.RightKey, KeyCode.I },
        { KeyTypes.UpKey, KeyCode.J },
        { KeyTypes.DownKey, KeyCode.F },
    };

    public NoteSkins NoteSkin = NoteSkins.MidiNote;
    public int NoteSkinIndex = 0;

    public NoteColors NoteColors = NoteColors.beat;

    public BackgroundTypes BackgroundType = BackgroundTypes.backdrop;

    public HitSounds HitSound = HitSounds.Default;

    public NoteScaleMode NoteScaleMode = NoteScaleMode.none;

    public int SlowdownMod = 0;
    public int MenuAnimations = 1;


    public int ScreenshakeIntensity = 0;
    public int BackdropBrightness = 33;
    public int PostProcessing = 1;
    public int MotionBlur = 0;
    public int Bloom = 1;
    public int PostExposure = 1;
    public int ChromaticAberration = 0;
    public int Grain = 1;
    public int Vignette = 1;
    public int SpecialEffect = 0;

    public int UILayer = 0;

    public int VSync = 0;
    public int PitchCompensation = 0;

    public int SongselectBackground = 1;

    public int MaxNoteUpdates = 50;

    public ScrollDirction ScrollDirection = ScrollDirction.down;

    public int ReceptorHeight = 0; // -1 higher, 0 normal, 1 lower
    public int ReceptorPosition = 0; // -1 left, 0 center, 1 right

    public int NoteTwist = 0;
    public int ReceptorTwist = 0;
    public int ReceptorWaveHor = 0;
    public int ReceptorWaveVer = 0;
    public int ReceptorWaveSpeed = 100;

    public int ParticleEmmisionScale = 1;
    public int ParticleSpeed = 15;

    public static void WriteData()
    {
        if (!Directory.Exists(Utility.GetRoot() + "/UserData"))
            Directory.CreateDirectory(Utility.GetRoot() + "/UserData");

        string filePath = Utility.GetRoot() + "/UserData/Config.um";
        string jsonData;

        jsonData = JsonConvert.SerializeObject(Instance, Formatting.Indented);
        
        File.WriteAllText(filePath, jsonData);
    }

    private static Modifications ReadData()
    {
        if (!Directory.Exists(Utility.GetRoot() + "/UserData"))
            Directory.CreateDirectory(Utility.GetRoot() + "/UserData");

        string filePath = Utility.GetRoot() + "/UserData/Config.um";
        string jsonData;

        if (!File.Exists(filePath))
            return new Modifications();

        jsonData = File.ReadAllText(filePath);

        try
        {
            return JsonConvert.DeserializeObject<Modifications>(jsonData);
        }
        catch
        {
            File.Delete(filePath);
            return new Modifications();
        }
    }
}
