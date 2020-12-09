using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldTail : Note
{
    private HoldHead m_head;

    private SpriteRenderer[] m_subSprites;

    private Sprite m_centerSprite;
    private Sprite m_bottomSprite;

    private float m_centerHeight;

    private int spriteIndex = -1;

    public override void Initalize(NoteIndex noteIndex, float barPosition, Directions direction, ChartManager noteGen, float offset, float bpm, float songPosition)
    {
        m_type = NoteTypes.HoldTail;
        m_color = Color.white;

        m_sprite = GameManager.Instance.SpritePack.holdTop[0];
        m_centerSprite = GameManager.Instance.SpritePack.holdCenter[0];
        m_bottomSprite = GameManager.Instance.SpritePack.holdBottom[0];

        m_centerHeight = m_centerSprite.bounds.size.y;

        m_subSprites = GetComponentsInChildren<SpriteRenderer>();

        m_subSprites[1].sprite = m_centerSprite;
        m_subSprites[2].sprite = m_bottomSprite;

        m_rotate = false;

        // Correct the rotation when the scroll direction is reversed
        if(Modifications.Instance.ScrollDirection == ScrollDirction.up)
        {
            transform.eulerAngles += new Vector3(0, 0, 180);
        }

        base.Initalize(noteIndex, barPosition, direction, noteGen, offset, bpm, songPosition);
    }

    public override void PostSpawn()
    {
        // Look for it's head
        // We start looking from its own position and we stop at the first head we see
        for (int i = m_noteIndex.note; i >= 0; i--)
        {
            var note = GameManager.Instance.Notes[m_noteIndex.dir][i];

            if (note.Type == NoteTypes.HoldHeader)
            {
                m_head = note as HoldHead;
                break;
            }
        }

        base.PostSpawn();
    }

    protected override void NoteUpdate()
    {
        // Set the right sprite
        int i = m_head.HoldActive ? 1 : 0;

        if (i != spriteIndex)
        {
            spriteIndex = i;

            m_sprite = GameManager.Instance.SpritePack.holdTop[spriteIndex];
            m_centerSprite = GameManager.Instance.SpritePack.holdCenter[spriteIndex];
            m_bottomSprite = GameManager.Instance.SpritePack.holdBottom[spriteIndex];

            m_subSprites[0].sprite = m_sprite;
            m_subSprites[1].sprite = m_centerSprite;
            m_subSprites[2].sprite = m_bottomSprite;
        }

        // Set the position
        SetPositionToBar(true);

        var distance = Vector2.Distance(transform.position, m_head.transform.position); // Calculate distance between head and tail
        var centerScale = (1 / m_centerHeight) * distance; // Calculate scale size for the center sprite

        m_subSprites[1].transform.localScale = new Vector3(1, centerScale, 1);
        m_subSprites[2].transform.localPosition = new Vector3(0, -distance, 0);

        base.NoteUpdate();
    }

    public override void OnHit()
    {
        base.OnHit();

        m_head.Deactivate();
        Deactivate();
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public bool HoldActive
    {
        get { return m_head.HoldActive; }
    }
}