using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    private GameManager m_gameManager;
    private InputHandler m_inputHandler;
    private Dictionary<Judgements, float> m_judgementTimes;
    private Dictionary<Judgements, int> m_judgementWeights;

    private bool m_inputDisabled = false;

    #region Events
    public delegate void NoteHit(NoteTypes t, Directions d, Judgements j);
    public delegate void Judgement(Judgements j);

    public event NoteHit NoteHitEvent;
    public event Judgement JudgementEvent;
    #endregion

    private void Awake()
    {
        // Times are based on osu!mania's OD 8
        // https://new.ppy.sh/forum/t/442816 for more info
        m_judgementTimes = JudgementValues.GetJugementTimes(Modifications.Instance.JudgeDiff);
    }

    void Start ()
    {
        m_gameManager = GameManager.Instance;
        m_inputHandler = InputHandler.Instance;

        // Subscribe to the keyinput events
        m_inputHandler.KeyLeftEvent += PressLeft;
        m_inputHandler.KeyDownEvent += PressDown;
        m_inputHandler.KeyUpEvent += PressUp;
        m_inputHandler.KeyRightEvent += PressRight;

        m_inputHandler.KeyLeftReleaseEvent += ReleaseLeft;
        m_inputHandler.KeyDownReleaseEvent += ReleaseDown;
        m_inputHandler.KeyUpReleaseEvent += ReleaseUp;
        m_inputHandler.KeyRightReleaseEvent += ReleaseRight;
    }

    private void OnDisable()
    {
        // UnSubscribe to the keyinput events
        m_inputHandler.KeyLeftEvent -= PressLeft;
        m_inputHandler.KeyDownEvent -= PressDown;
        m_inputHandler.KeyUpEvent -= PressUp;
        m_inputHandler.KeyRightEvent -= PressRight;

        m_inputHandler.KeyLeftReleaseEvent -= ReleaseLeft;
        m_inputHandler.KeyDownReleaseEvent -= ReleaseDown;
        m_inputHandler.KeyUpReleaseEvent -= ReleaseUp;
        m_inputHandler.KeyRightReleaseEvent -= ReleaseRight;
    }

    private void Update()
    {
        // Check if notes are below the late-miss threshold
        for (int l = 0; l < m_gameManager.Notes.Count; l++) // L stands for the current list
        {
            var noteList = m_gameManager.Notes[(Directions)l];

            // Loop through all the notes of the current row
            for (int n = 0; n < noteList.Length; n++) // N stands for the current note
            {
                // Skip if note isn't active
                if (!noteList[n].IsActive)
                    continue;

                // Check if the note is too far away
                if ((noteList[n].DistanceInSec) < -m_judgementTimes[Judgements.miss])
                {
                    switch (noteList[n].Type)
                    {
                        case NoteTypes.Tap:
                            // Trigger the judgement event
                            OnJudgement(Judgements.miss);

                            // Disable the note
                            noteList[n].OnHit();
                            break;

                        case NoteTypes.HoldHeader:
                            break;

                        case NoteTypes.HoldTail:
                            // Trigger the judgement event
                            OnJudgement(Judgements.miss);

                            // Disable the note
                            noteList[n].OnHit();
                            break;
                    }             
                }
            }
        }
    }

    #region Key events

    private void PressLeft()
    {
        bool noteIsHit = false;
        if (m_inputDisabled)
            return;

        noteIsHit = CheckTiming(Directions.left, NoteTypes.Tap);

        if (!noteIsHit)
            noteIsHit = CheckTiming(Directions.left, NoteTypes.HoldHeader);

        m_gameManager.Receptors[Directions.left].Activate(noteIsHit);
    }
    private void PressDown()
    {
        bool noteIsHit = false;
        if (m_inputDisabled)
            return;

        noteIsHit = CheckTiming(Directions.down, NoteTypes.Tap);

        if (!noteIsHit)
            noteIsHit = CheckTiming(Directions.down, NoteTypes.HoldHeader);

        m_gameManager.Receptors[Directions.down].Activate(noteIsHit);
    }
    private void PressUp()
    {
        bool noteIsHit = false;
        if (m_inputDisabled)
            return;

        noteIsHit = CheckTiming(Directions.up, NoteTypes.Tap);

        if (!noteIsHit)
            noteIsHit = CheckTiming(Directions.up, NoteTypes.HoldHeader);

        m_gameManager.Receptors[Directions.up].Activate(noteIsHit);
    }
    private void PressRight()
    {
        bool noteIsHit = false;
        if (m_inputDisabled)
            return;

        noteIsHit = CheckTiming(Directions.right, NoteTypes.Tap);

        if (!noteIsHit)
            noteIsHit = CheckTiming(Directions.right, NoteTypes.HoldHeader);

        m_gameManager.Receptors[Directions.right].Activate(noteIsHit);
    }

    private void ReleaseLeft()
    {
        if (m_inputDisabled)
            return;

        CheckTiming(Directions.left, NoteTypes.HoldTail);
    }
    private void ReleaseDown()
    {
        if (m_inputDisabled)
            return;

        CheckTiming(Directions.down, NoteTypes.HoldTail);
    }
    private void ReleaseUp()
    {
        if (m_inputDisabled)
            return;

        CheckTiming(Directions.up, NoteTypes.HoldTail);
    }
    private void ReleaseRight()
    {
        if (m_inputDisabled)
            return;

        CheckTiming(Directions.right, NoteTypes.HoldTail);
    }

    #endregion

    // Checks timing of the closest note, returns false if it cound't find one of it's type
    private bool CheckTiming(Directions dir, NoteTypes noteType)
    {
        // the note that is the closest to the receptor
        int noteID = -1;

        // distance for the closest note
        float distance = float.MaxValue;

        // The judgement for the closest note
        Judgements judgement = Judgements.miss;

        // Get the list of notes that are in the direction
        var noteList = m_gameManager.Notes[dir];

        // Loop through all the notes of the current row
        // to check which note is the closest to the receptor
        for (int i = 0; i < noteList.Length; i++)
        {
            // Skip if the note isn't the desired type
            if (noteList[i].Type != noteType)
                continue;

            // Skip if note isn't active
            if (!noteList[i].IsActive)
                continue;

            float noteDistance = noteList[i].Distance;

            // Check if current note is closer than the previous closest note
            if (Mathf.Abs(noteDistance) < Mathf.Abs(distance))
            {
                // Update the Id and distance
                noteID = i;
                distance = noteList[i].DistanceInSec;
            }
        }

        if (noteID == -1)
            return false;

        // Check if the note is too far away
        // This doesn't count for the hold tails, but only when they're active
        if (Mathf.Abs(distance) > m_judgementTimes[Judgements.miss])
        {
            if (!(noteType == NoteTypes.HoldTail && (noteList[noteID] as HoldTail).HoldActive))
                return false;
        }

        // Check which judgement this hit should get
        for (int i = 0; i < Enum.GetNames(typeof(Judgements)).Length; i++)
        {
            if (distance <= (m_judgementTimes[(Judgements)i] * (1 / (float)Modifications.Instance.MusicPitch * 100)))
            {
                judgement = (Judgements)i;
                break;
            }
        }

        // Let the note know it was hit
        noteList[noteID].OnHit();

        // Trigger the note hit event
        OnNoteHit(noteType, dir, judgement);

        // Trigger the judgement event
        OnJudgement(judgement);

        return true;
    }

    // Event Handlers
    private void OnNoteHit(NoteTypes t, Directions d, Judgements j)
    {
        if (NoteHitEvent != null)
            NoteHitEvent(t, d, j);

        switch (t)
        {
            case NoteTypes.Tap:
                AudioManager.Instance.PlaySound(Utility.GetHitsound(Modifications.Instance.HitSound));
                break;

            case NoteTypes.HoldHeader:
                AudioManager.Instance.PlaySound(Utility.GetHitsound(Modifications.Instance.HitSound));
                break;

            case NoteTypes.HoldTail:
                break;
        }
    }
    private void OnJudgement(Judgements j)
    {
        if (JudgementEvent != null)
            JudgementEvent(j);
    }

    public Dictionary<Judgements, float> JudgementTimes
    {
        get { return m_judgementTimes; }
    }

    public bool InputDisabled
    {
        get { return m_inputDisabled; }
        set { m_inputDisabled = value; }
    }
}
