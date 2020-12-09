using UnityEngine;

public class TapNote : Note
{
    public override void Initalize(NoteIndex noteIndex, float barPosition, Directions direction, ChartManager noteGen, float offset, float bpm, float songPosition)
    {
        m_type = NoteTypes.Tap;

        m_sprite = GameManager.Instance.SpritePack.note;
        m_color = NoteColor(barPosition);

        base.Initalize(noteIndex, barPosition, direction, noteGen, offset, bpm, songPosition);
    }

    protected override void NoteUpdate()
    {
        SetPositionToBar(false);

        base.NoteUpdate();
    }

    public override void OnHit()
    {
        base.OnHit();

        Deactivate();
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }
}