using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

// Should be called: chartmanager
public class ChartManager : MonoBehaviour
{
    private Difficulties m_difficulty;

    private Metadata m_songData;
    private AudioSource m_song;
    private NoteData m_noteData;
    private GameManager m_gameManager;
    private SongParser m_songParser;

    [Tooltip("Object where all the notes will spawn in")]
    [SerializeField]
    private Transform m_noteParent;

    public GameObject tapNotePrefab;
    public GameObject holdHeadPrefab;
    public GameObject holdTailPrefab;

    private float m_songTime = 0.0f;
    private float m_songCountdown;
    private float m_songCountdownTime = 2.0f;
    private bool m_songCountdownStarted = false;

    private List<float> m_bpmDeltas;

    private void Awake()
    {
        m_song = GetComponent<AudioSource>();

        m_songCountdown = m_songCountdownTime;
    }

    private void Start()
    {
        m_gameManager = GameManager.Instance;
    }

    // Update is called once per frame
    private void Update()
    {
        // Make that the song doesn't start the exact moment the scene loads
        if (m_songCountdown >= 0)
        {
            m_songTime = -m_songCountdown;
            m_songCountdown -= Time.deltaTime;
            m_songCountdownStarted = true;

            // Start the song once the countdown is over
            if (m_songCountdown < 0)
            {
                m_song.Play();
            }
        }
        else
        {
            m_songTime = m_song.time;
        }
    }

    public void ResetSong()
    {
        m_song.Stop();

        m_songTime = 0.0f;
        m_songCountdown = 0.0f;
        m_songCountdownStarted = true;
    }

    public void InitSteps(Metadata newSongData, Difficulties newDifficulty)
    {
        m_songData = newSongData;
        m_difficulty = newDifficulty;

        m_noteData = m_songData.notedata[m_difficulty];

        /*
        switch (m_difficulty)
        {
            case Difficulties.beginner:
                m_noteData = m_songData.beginner;
                break;

            case Difficulties.easy:
                m_noteData = m_songData.easy;
                break;

            case Difficulties.medium:
                m_noteData = m_songData.medium;
                break;

            case Difficulties.hard:
                m_noteData = m_songData.hard;
                break;

            case Difficulties.challenge:
                m_noteData = m_songData.challenge;
                break;

            case Difficulties.insane:
                m_noteData = m_songData.insane;
                break;
        }
        */

        m_song.clip = SongData.AudioClip;

        SpawnNotes();
    }

    public void SpawnNotes()
    {
        int bars = m_noteData.bars.Count;

        List<Note> notesLeft = new List<Note>();
        List<Note> notesDown = new List<Note>();
        List<Note> notesUp = new List<Note>();
        List<Note> notesRight = new List<Note>();

        for (int b = 0; b < bars; b++) // b stands for all the bars in the song
        {
            int rows = m_noteData.bars[b].notes.GetLength(0);

            for (int r = 0; r < rows; r++) // r stands for the row in the curent bar
            {
                float barPosition;

                barPosition = ((1.0f / rows) * r) + b;

                for (int n = 0; n < 4; n++) // n stands for the note in the current row
                {
                    int note = m_noteData.bars[b].notes[r, n]; // The type of the current note

                    if (note != 0) // Check if the current note isn't nothing
                    {
                        GameObject obj = null;
                        NoteIndex nIndex;
                        int index = -1;

                        switch ((NoteTypes)note) // Check which type note is and instantiate a prefab
                        {
                            case NoteTypes.Tap:
                                obj = Instantiate(tapNotePrefab, m_noteParent);
                                break;

                            case NoteTypes.HoldHeader:
                                obj = Instantiate(holdHeadPrefab, m_noteParent);
                                break;

                            case NoteTypes.HoldTail:
                                obj = Instantiate(holdTailPrefab, m_noteParent);
                                break;
                        }


                        var noteComp = obj.GetComponent<Note>();

                        float bpm = 0.0f;
                        var bpmsLength = m_songData.bpms.Length;

                        // Check what the bpm for the note is
                        for (int i = 0; i < bpmsLength; i++)
                        {
                            try
                            {
                                if (barPosition >= m_songData.bpms[i].bar && barPosition < m_songData.bpms[i + 1].bar)
                                {
                                    bpm = m_songData.bpms[i].bpm;
                                }
                            }
                            catch
                            {
                                bpm = m_songData.bpms[bpmsLength - 1].bpm;
                            }
                        }

                        // Check the direction (horizontal position) of the note
                        // + what the index is for the overal direction for that note
                        switch ((Directions)n)
                        {
                            case Directions.left:
                                index = notesLeft.Count;
                                notesLeft.Add(noteComp);
                                break;

                            case Directions.down:
                                index = notesDown.Count;
                                notesDown.Add(noteComp);
                                break;

                            case Directions.up:
                                index = notesUp.Count;
                                notesUp.Add(noteComp);
                                break;

                            case Directions.right:
                                index = notesRight.Count;
                                notesRight.Add(noteComp);
                                break;
                        }

                        // This might be a temporary solution unitill the notepool works
                        m_gameManager.AllNotes.Add(noteComp);

                        // Initalize the note
                        nIndex = new NoteIndex(b, r, (Directions)n, index);
                        noteComp.Initalize(nIndex, barPosition, (Directions)n, this, m_songData.offset, bpm, GetDurationUntilMeasure(barPosition));
                    }
                }
            }
        }

        // Give the list of all the notes to the gamemanager
        m_gameManager.Notes[Directions.left] = notesLeft.ToArray();
        m_gameManager.Notes[Directions.down] = notesDown.ToArray();
        m_gameManager.Notes[Directions.up] = notesUp.ToArray(); ;
        m_gameManager.Notes[Directions.right] = notesRight.ToArray(); ;

        // Run post spawn for every note spawned
        for (int direction = 0; direction < 4; direction++)
        {
            for (int note = 0; note < m_gameManager.Notes[(Directions)direction].Length; note++)
            {
                m_gameManager.Notes[(Directions)direction][note].PostSpawn();
            }
        }
    }

    float GetMeasureOffset(int index)
    {
        if (index >= m_songData.bpms.Length) return int.MaxValue;
        return m_songData.bpms[index].bar;
    }

    float GetBPM(int index)
    {
        return m_songData.bpms[index].bpm;
    }

    float GetDurationFromBPM(float BPM)
    {
        if (BPM == 0.0f) return 0.0f;
        return 60.0f / BPM * 4.0f;
    }

    float GetDurationUntilMeasure(float measure)
    {
        float elapsedTime = 0.0f;
        float previousOffset = 0.0f;

        for (int index = 0; index < m_songData.bpms.Length; index++)
        {
            float offset = GetMeasureOffset(index + 1);
            if (offset >= measure) offset = measure;

            float deltaTime = offset - previousOffset;
            previousOffset = offset;

            float currentBPM = GetBPM(index);

            float currentDuration = GetDurationFromBPM(currentBPM) * deltaTime;

            elapsedTime += currentDuration;
            if (offset >= measure) break;

        }

        return elapsedTime;
    }

    public void StartInitalization()
    {
        InitSteps(SongData.Song, SongData.Difficulty);
    }

    public void PauseMusic()
    {
        m_song.Pause();
    }

    public void ResumeMusic()
    {
        m_song.UnPause();
    }

    public float SongTime
    {
        get { return m_songTime; }
    }
    public AudioSource AudioSource
    {
        get { return m_song; }
    }
    public Metadata Metadata
    {
        get { return m_songData; }
    }
    public NoteData NoteData
    {
        get { return m_noteData; }
    }
    public float Countdown
    {
        get { return m_songCountdown; }
    }
    public float CountdownTime
    {
        get { return m_songCountdownTime; }
    }
    public bool CountdownStarted
    {
        get { return m_songCountdownStarted; }
    }

}