using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// I started working on a state based note,
/// but it got in the way of the other game systems
/// so this is on hold for now
/// </summary>
public interface INote
{
    void Create(NoteNEW note);
    void Initalize();
    void PostSpawn();
    void NoteUpdate();
    void OnHit();
    void Deactivate();

}

public class NoteNEW : MonoBehaviour
{
    protected NoteIndex m_noteIndex;

    private ChartManager m_noteGen;
    private SpriteRenderer m_spriteRenderer;

    protected GameObject m_receptor;
    protected Sprite m_sprite;
    protected Color m_color;

    private float m_songPosition;
    private float m_barPosition;
    protected float m_speed;
    private float m_offset;
    private float m_bpm;

    private bool m_canUpdate = false;
    private bool m_isHit = false;
    private bool m_isAcitve = true;
    protected bool m_rotate = true; // If the note rotates based on it's lane

    protected NoteTypes m_type;
    private Directions m_dir;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Initalization for when the note is spawned
    /// </summary>
    /// <param name="barPosition"></param>
    /// <param name="direction"></param>
    /// <param name="noteGen"></param>
    /// <param name="offset"></param>
    /// <param name="bpm"></param>
    public virtual void Initalize(NoteIndex noteIndex, Directions direction, ChartManager noteGen, float songPosition, float bpm, float barPosition)
    {
        m_noteIndex = noteIndex;
        m_noteGen = noteGen;
        m_barPosition = barPosition;
        m_songPosition = songPosition;
        m_dir = direction;
        //m_offset = offset;
        m_bpm = bpm;
        m_speed = Modifications.Instance.ScrollSpeed * (1 / (float)Modifications.Instance.MusicPitch * 100);

        m_receptor = GameManager.Instance.Receptors[m_dir].gameObject;

        int angle = 0;

        if (m_rotate)
        {
            switch (direction)
            {
                case Directions.left:
                    angle = -90;
                    break;
                case Directions.down:
                    angle = 0;
                    break;
                case Directions.up:
                    angle = 180;
                    break;
                case Directions.right:
                    angle = 90;
                    break;
            }
        }

        // Apply all modifications
        m_spriteRenderer.sprite = m_sprite;
        m_spriteRenderer.color = m_color;

        transform.eulerAngles = new Vector3(0, 0, angle);
    }

    /// <summary>
    /// Method that gets called when all the notes are spawned
    /// </summary>
    public virtual void PostSpawn()
    {
        // Start the update
        //StartCoroutine(UpdateCoroutine());
        //m_canUpdate = true;
        gameObject.SetActive(false);
    }
    protected IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            NoteUpdate();
            yield return new WaitForEndOfFrame();
        }
    }

    private void Update()
    {
        if (!m_canUpdate)
            return;

        NoteUpdate();
    }

    protected virtual void NoteUpdate()
    {
        // update code here
    }

    /// <summary>
    /// When the note gets hit
    /// </summary>
    public virtual void OnHit()
    {
        m_isHit = true;
    }

    /// <summary>
    /// Deactivates the note
    /// </summary>
    public virtual void Deactivate()
    {
        Debug.Log("Deacc");
        m_isAcitve = false;
        transform.localScale = Vector3.zero;
    }

    /// <summary>
    /// The note's corresponding receptor
    /// </summary>
    public GameObject Receptor
    {
        get { return m_receptor; }
    }

    public bool CanUpdate
    {
        get { return m_canUpdate; }
        set { m_canUpdate = value; }
    }

    /// <summary>
    /// The time in seconds of one bar
    /// </summary>
    public float BarTime
    {
        get { return (60.0f / m_bpm * 4.0f); }
    }
    
    public float BarPosition
    {
        get { return m_barPosition; }
    } 
    /// <summary>
    /// The speed of the note
    /// </summary>
    public float Speed
    {
        get { return m_speed; }
    }

    /// <summary>
    /// The amount of bars the note is away of the receptor
    /// </summary>
    public float Distance
    {
        //get { return ((m_barPosition * BarTime) - m_noteGen.SongTime - m_offset); }
        get { return (m_songPosition - m_noteGen.SongTime - m_offset); }
    }

    /// <summary>
    /// The amount of seconds the note is away of the receptor
    /// </summary>
    public float DistanceInSec
    {
        get { return Distance * BarTime; }
    }

    /// <summary>
    /// If the note is active or not
    /// </summary>
    public bool IsActive
    {
        get { return m_isAcitve; }
    }

    /// <summary>
    /// If the note is hit or not
    /// </summary>
    public bool IsHit
    {
        get { return m_isHit; }
    }

    /// <summary>
    /// The type of the note
    /// </summary>
    public NoteTypes Type
    {
        get { return m_type; }
    }

    public void SetSprite(Sprite sprite, int spriteIndex)
    {
        Debug.LogError("Set sprite function is empty!");
    }
    public void SetColor(Color color)
    {
        Debug.LogError("Set color function is empty!");
    }
    /// <summary>
    /// Calulates a index for the sprite based on the position in it's bar
    /// </summary>
    /// <param name="barpos"></param>
    /// <returns></returns>
    public int SpriteIndex(float barpos)
    {
        int index = 7; // set tis to seven for now, in stepmania the note is made gray if its higher than a 64th
        float pos = barpos % 1;

        #region Lots of if

        if (pos % (1.0f / 4.0f) == 0)
        {
            index = 0;
        }
        else if (pos % (1f / 8f) == 0)
        {
            index = 1;
        }
        else if (pos % (1f / 12f) == 0)
        {
            index = 2;
        }
        else if (pos % (1f / 16f) == 0)
        {
            index = 3;
        }
        else if (pos % (1f / 24) == 0)
        {
            index = 4;
        }
        else if (pos % (1f / 32) == 0)
        {
            index = 5;
        }
        else if (pos % (1f / 48) == 0)
        {
            index = 6;
        }
        else if (pos % (1f / 64) == 0)
        {
            index = 7;
        }

        #endregion

        return index;
    }
    public Color NoteColor(float barpos)
    {
        Color color = Color.gray;
        float pos = barpos % 1;

        #region Lots of if

        if (pos % (1.0f / 4.0f) == 0)
        {
            // Red
            color = rgbColor(247, 44, 44);
        }
        else if (pos % (1f / 8f) == 0)
        {
            // Blue
            color = rgbColor(23, 51, 193);
        }
        else if (pos % (1f / 12f) == 0)
        {
            // Purple
            color = rgbColor(139, 52, 216);
        }
        else if (pos % (1f / 16f) == 0)
        {
            // Yellow
            color = rgbColor(237, 233, 21);
        }
        else if (pos % (1f / 24) == 0)
        {
            // Pink
            color = rgbColor(247, 42, 165);
        }
        else if (pos % (1f / 32) == 0)
        {
            // Orange
            color = rgbColor(247, 148, 19);
        }
        else if (pos % (1f / 48) == 0)
        {
            // Turqoise
            color = rgbColor(25, 234, 192);
        }
        else if (pos % (1f / 64) == 0)
        {
            // Green
            color = rgbColor(135, 232, 18);
        }

        #endregion

        return color;
    }
    private Color rgbColor(float red, float green, float blue)
    {
        return new Color((float)red / 255f, (float)green / 255f, (float)blue / 255f);
    }
}

public class TapNoteNEW : INote
{
    NoteNEW m_note;
    public void Create(NoteNEW note)
    {
        m_note = note;
    }

    public void Initalize()
    {
        m_note.SetSprite(GameManager.Instance.SpritePack.note, 0);
        m_note.SetColor(m_note.NoteColor(m_note.BarPosition));
    }

    public void NoteUpdate()
    {
        // Set the position
        Vector2 pos = new Vector2
        {
            x = m_note.Receptor.transform.position.x,
            y = m_note.Distance * m_note.Speed
        };

        m_note.transform.position = pos;
    }

    public void OnHit()
    {
        m_note.Deactivate();
    }

    public void Deactivate()
    {
        // Nothing
    }

    public void PostSpawn()
    {
        // Nothing
    }
}

public class HoldHeadNoteNEW : INote
{
    NoteNEW m_note;
    public void Create(NoteNEW note)
    {
        m_note = note;
    }

    public void Initalize()
    {
        m_note.SetSprite(GameManager.Instance.SpritePack.note, 0);
        m_note.SetColor(m_note.NoteColor(m_note.BarPosition));
    }

    public void NoteUpdate()
    {
        // Set the position
        Vector2 pos = new Vector2
        {
            x = m_note.Receptor.transform.position.x,
            y = m_note.Distance * m_note.Speed
        };

        m_note.transform.position = pos;
    }

    public void OnHit()
    {
        m_note.Deactivate();
    }

    public void Deactivate()
    {
        // Nothing
    }

    public void PostSpawn()
    {
        throw new System.NotImplementedException();
    }
}

public class HoldTailNoteNEW : INote
{
    NoteNEW m_note;
    public void Create(NoteNEW note)
    {
        m_note = note;
    }

    public void Initalize()
    {
        m_note.SetSprite(GameManager.Instance.SpritePack.note, 0);
        m_note.SetColor(m_note.NoteColor(m_note.BarPosition));
    }

    public void NoteUpdate()
    {
        // Set the position
        Vector2 pos = new Vector2
        {
            x = m_note.Receptor.transform.position.x,
            y = m_note.Distance * m_note.Speed
        };

        m_note.transform.position = pos;
    }

    public void OnHit()
    {
        m_note.Deactivate();
    }

    public void Deactivate()
    {
        // Nothing
    }

    public void PostSpawn()
    {
        throw new System.NotImplementedException();
    }
}