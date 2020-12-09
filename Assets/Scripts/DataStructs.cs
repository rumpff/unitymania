using UnityEngine;
using System.Collections.Generic;

// This structure contains all the information for a track
public struct Metadata
{
    public bool valid;

    public string title;
    public string subtitle;
    public string artist;
    public string bannerPath;
    public string backgroundPath;
    public string musicPath;
    public float offset;
    public float sampleStart;
    public float sampleLength;
    public BPMS[] bpms;

    public Dictionary<Difficulties, bool> difficultyExists;
    public Dictionary<Difficulties, NoteData> notedata;

    public AudioClip songClip;
}

// This structure contains all the bars for a song at a single difficulty
public struct NoteData
{
    public List<Bar> bars;
}

// This contains all the notes within one bar
public struct Bar
{
    public int[,] notes;
}

/// <summary>
/// Informaion about the position of a note in the song
/// </summary>
public struct NoteIndex
{
    /// <summary>
    /// The bar the note is in
    /// </summary>
    public int bar;
    /// <summary>
    /// The row the note is in
    /// </summary>
    public int row;
    /// <summary>
    /// The position of the note
    /// </summary>
    public Directions dir;
    /// <summary>
    /// The index of the note of an array of all the notes of the same position
    /// </summary>
    public int note;

    /// <summary>
    /// Contains all info about a note
    /// </summary>
    /// <param name="bar"></param>
    /// <param name="row"></param>
    /// <param name="pos"></param>
    /// <param name="noteType"></param>
    public NoteIndex(int b, int r, Directions d, int n)
    {
        bar = b;
        row = r;
        dir = d;
        note = n;
    }
}

public struct BPMS
{
    public float bar;
    public float bpm;
}

public struct SpritePackInfo
{
    public string name;
    public string dir;

    public string receptorName;
    public string tapnoteName;

    public string holdBottomInactiveName;
    public string holdBottomActiveName;

    public string holdCenterInactiveName;
    public string holdCenterActiveName;

    public string holdTopInactiveName;
    public string holdTopActiveName;

    public Vector2 receptorPivot;
    public Vector2 tapPivot;

    public Vector2 holdTopPivot;
    public Vector2 holdCenterPivot;
    public Vector2 holdBottomPivot;
}

// Text for the song select preview
public struct SSPText
{
    public List<string> text;
    public string[] dest;
}

public struct RectSize
{
    public Vector2 position;
    public Vector3 size;

    /// <summary>
    /// Struckt that holds position and size for rect transforms
    /// </summary>
    /// <param name="p">the position</param>
    /// <param name="s">the size</param>
    public RectSize(Vector2 p, Vector3 s)
    {
        position = p;
        size = s;
    }

    public static RectSize zero
    {
        get { return new RectSize(Vector2.zero, Vector3.zero); }
    }
}

public struct ScoreData
{
    public Ratings Rating;
    public int Score;
    public float Accuracy;
    public float Combo;
    public System.DateTime Time;
}

public class SongScoreData
{
    public Dictionary<Difficulties, List<ScoreData>> Scores;

    public SongScoreData()
    {
        Scores = new Dictionary<Difficulties, List<ScoreData>>();
    }
}

public struct PlayerHighscores
{
    public Dictionary<string, SongScoreData> Highscores;
}

// Content for the buttons in the song selection
public struct MenuItemContent
{
    public Metadata metaData;
    public Sprite backdrop;
    public AudioClip musicClip;
    public Color averageColor;
}

// In game
public enum Difficulties { beginner, easy, medium, hard, challenge, insane };
public enum Directions { left, down, up, right };
public enum Judgements { marvelous, perfect, great, good, bad, miss };
public enum Ratings { ss, s, a, b, c, d };
public enum NoteTypes { Tap = 1, HoldHeader = 2, HoldTail = 3 };
public enum ScrollDirction { down, up }
public enum NoteScaleMode { none, appear, disappear, bigboy, jumping }
// Menus
public enum SongSelectState { intro, preview, focus, loadingSongs, loadingGame, inModifiers };
public enum MainMenuStates { intro, main, options, help, toSelect };
public enum ModifierWindowStates { normal, optionSelected };
public enum ActiveState { inactive, active };
// Options
public enum NoteSkins { ExactV2, MidiNote, SMDefault };
public enum NoteColors { beat, random, adaptive, white };
public enum BackgroundTypes { backdrop, nothing };
// Other
public enum Sounds
{
    DifficultySelect,
    DifficultyEnter,
    MenuTickNew,
    NoteHitDefault,
    NoteHitOsu,
    NoteHitTrumpet,
    NoteHitEat,
    NoteHitDistortion,
    NoteHitSans,
    NoteHitBruh,
    SelectSong,
    SongStart,
    ModValueChange,
    SwitchTab,
    ModApply,
    ModRevert,
    ModSelect,
    ModMoveSelection,
    Error,
    PausedContinue,
    PausedRestart,
    PausedGiveUp,
    PausedSelect,
    SsFocusEnter,
	HighscoreSelect,
	HighscoreSelectError
}
public enum HitSounds
{
    Default,
    Kick,
    Osu,
    Trumpet,
    Eat,
    Distortion,
    Sans,
    Bruh
}
public enum KeyTypes { StartKey, ExitKey, RestartKey, LeftKey, RightKey, UpKey, DownKey }