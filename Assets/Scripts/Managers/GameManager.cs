using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static Scene SceneInstance;
    
    private Dictionary<Directions, Receptor> m_receptors;
    private SpritePack m_spritePack;

    private Dictionary<Directions, Note[]> m_notes;
    private List<Note> m_allNotes;

    private GameEndManager m_gameEndManager;
    private GameplayManager m_gameplayManager;
    private ScoreManager m_scoreManager;
    private ChartManager m_chartManager;
    private UIManager m_uiManager;
    private GameCamera m_gameCameraComp;

    private Camera m_backdropCamera;
    private Camera m_gameCamera;

    private SpriteRenderer m_backdrop;

    private void Awake()
    {
        Instance = this;
        SceneInstance = SceneManager.GetActiveScene();

        m_allNotes = new List<Note>();
        m_notes = new Dictionary<Directions, Note[]>();

        // Fill the spritepacks, debug
        /*
        m_spritePack.receptor = Resources.Load<Sprite>("Sprites/Notes/Receptor");
        m_spritePack.note = Resources.Load<Sprite>("Sprites/Notes/TapNote");

        m_spritePack.holdTop = new Sprite[] { Resources.Load<Sprite>("Sprites/Notes/HoldTopInActive"),
            Resources.Load<Sprite>("Sprites/Notes/HoldTopActive") };
        m_spritePack.holdCenter = new Sprite[] { Resources.Load<Sprite>("Sprites/Notes/HoldCenterInActive"),
            Resources.Load<Sprite>("Sprites/Notes/HoldCenterActive") };
        m_spritePack.holdBottom = new Sprite[] { Resources.Load<Sprite>("Sprites/Notes/HoldBottomInActive"),
            Resources.Load<Sprite>("Sprites/Notes/HoldBottomActive") };
            */

        //m_spritePack = (SpritePack)Resources.Load("Sprite Packs/" + Modifications.Instance.NoteSkin);
        try
        {
            m_spritePack = SongSelect.SpritePacks[Modifications.Instance.NoteSkinIndex];
        }
        catch
        {
            m_spritePack = SongSelect.SpritePacks[0];
        }

        m_gameEndManager = GetComponent<GameEndManager>();
        m_gameplayManager = GetComponent<GameplayManager>();
        m_scoreManager = GetComponent<ScoreManager>();
        m_chartManager = GetComponent<ChartManager>();
        m_uiManager = GetComponent<UIManager>();
    }

    private void Start()
    {
        m_gameCamera = GameObject.Find("GameCamera").GetComponent<Camera>();
        m_backdropCamera = GameObject.Find("BackdropCamera").GetComponent<Camera>();

        m_gameCameraComp = m_gameCamera.GetComponent<GameCamera>();

        // Put the receptors in a dictionary
        var receptors = GameObject.Find("Receptors").GetComponentsInChildren<Receptor>();
        m_receptors = new Dictionary<Directions, Receptor>();

        foreach(Receptor r in receptors)
        {
            m_receptors.Add(r.Direction, r);
        }

        // Make the backdrop fit perfectly with any screensize / ratio

        float screenAspect = (float)Screen.width / (float)Screen.height;
        float camHalfHeight = m_backdropCamera.orthographicSize;
        float camHalfWidth = screenAspect * camHalfHeight;
        float camWidth = 2.0f * camHalfWidth;

        m_backdrop = GameObject.Find("Backdrop").GetComponent<SpriteRenderer>();
        m_backdrop.sprite = SongData.Backdrop;
        m_backdrop.transform.localScale = new Vector2(camWidth, camWidth);
        m_backdrop.color = Color.HSVToRGB(0.0f, 0.0f, Modifications.Instance.BackdropBrightness / 100.0f);

        if (Modifications.Instance.BackgroundType == BackgroundTypes.nothing)
            m_backdrop.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);

        // Reset the audio effects from the song selection
        var mixer = ChartManager.AudioSource.outputAudioMixerGroup.audioMixer;
        mixer.SetFloat("musicDistortion", 0f);
        mixer.SetFloat("musicLowPass", 22000f);

        // Start the song initalization
        ChartManager.StartInitalization();
    }

    private void Update()
    {
        // Let only a set amount of notes update to improve framerate
        int maxUpdatingNotes = Modifications.Instance.MaxNoteUpdates;
        for (int i = 0; i < Mathf.Min(maxUpdatingNotes, m_allNotes.Count); i++)
        {
            // Very lame way to not check the lengh of the note arrays
            try
            {
                var note = m_allNotes[i];
                if (note.IsHit)
                {
                    maxUpdatingNotes++;
                    continue;
                }

                note.gameObject.SetActive(true);
                note.CanUpdate = true;
                
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    public void RestartGame()
    {
        // Reset every note

        foreach(Note note in m_allNotes)
        {
            note.ResetNote();
        }

        m_scoreManager.ResetScore();
        m_gameCameraComp.ResetCamera();
        m_chartManager.ResetSong();

    }

    public void LoadSongSelect()
    {
        SceneManager.LoadScene("songSelectScene", LoadSceneMode.Additive);
    }
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public GameplayManager GameplayManager
    {
        get { return m_gameplayManager; }
    }

    public ScoreManager ScoreManager
    {
        get { return m_scoreManager; }
    }

    public ChartManager ChartManager
    {
        get { return m_chartManager; }
    }
    public UIManager UIManager
    {
        get { return m_uiManager; }
    }
    public GameEndManager GameEndManager
    {
        get { return m_gameEndManager; }
    }
    public GameCamera GameCameraComp
    {
        get { return m_gameCameraComp; }
    }

    public Dictionary<Directions, Note[]> Notes
    {
        get { return m_notes; }
        set { m_notes = value; }
    }
    public List<Note> AllNotes
    {
        get { return m_allNotes; }
        set { m_allNotes = value; }    
    }

    public Dictionary<Directions, Receptor> Receptors
    {
        get { return m_receptors; }
    }

    public SpriteRenderer BackDrop
    {
        get { return m_backdrop; }
    }

    public SpritePack SpritePack
    {
        get { return m_spritePack; }
    }
}
