using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldHead : Note
{
    private HoldTail m_tail;

    private bool m_holdActive = false;

    public override void Initalize(NoteIndex noteIndex, float barPosition, Directions direction, ChartManager noteGen, float offset, float bpm, float songPosition)
    {
        m_sprite = GameManager.Instance.SpritePack.note;
        m_color = NoteColor(barPosition);
        m_type = NoteTypes.HoldHeader;

        m_rotate = true;

        // Correct the rotation when the scroll direction is reversed
        if (Modifications.Instance.ScrollDirection == ScrollDirction.up)
        {
            transform.eulerAngles += new Vector3(0, 0, 180);
        }

        base.Initalize(noteIndex, barPosition, direction, noteGen, offset, bpm, songPosition);
    }

    public override void PostSpawn()
    {
        // Look for it's tail
        // We start looking from its own position and we stop at the first tail we see
        for (int i = m_noteIndex.note; i < GameManager.Instance.Notes[m_noteIndex.dir].Length; i++)
        {
            var note = GameManager.Instance.Notes[m_noteIndex.dir][i];

            if (note.Type == NoteTypes.HoldTail)
            {
                m_tail = note as HoldTail;
                break;
            }
        }

        base.PostSpawn();
    }

    public override void ResetNote()
    {

        m_holdActive = false;
        base.ResetNote();
    }

    protected override void NoteUpdate()
    {
        if (m_holdActive)
        {
            // Set the position
            Vector2 pos = new Vector2
            {
                x = m_receptor.transform.position.x,
                y = m_receptor.transform.position.y
            };

            transform.position = pos;
        }
        else
        {
            SetPositionToBar(false);
        }

        base.NoteUpdate();
    }
   
    public bool HoldActive
    {
        get { return m_holdActive; }
    }

    public override void OnHit()
    {
        base.OnHit();
        m_holdActive = true;
    }

    public override void Deactivate()
    {
        base.OnHit();
        base.Deactivate();
    }
}
