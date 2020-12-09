
using System.Collections;
using UnityEngine;

public class Note : MonoBehaviour
{
    protected NoteIndex m_noteIndex;

    private ChartManager m_noteGen;
    private SpriteRenderer m_spriteRenderer;

    protected GameObject m_receptor;
    protected Receptor m_receptorComp;
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
    public virtual void Initalize(NoteIndex noteIndex, float barPosition, Directions direction, ChartManager noteGen, float offset, float bpm, float songPosition)
    {
        m_noteIndex = noteIndex;
        m_noteGen = noteGen;
        m_barPosition = barPosition;
        m_songPosition = songPosition;
        m_dir = direction;
        m_offset = offset;
        m_bpm = bpm;
        m_speed = Modifications.Instance.ScrollSpeed * (1 / (float)Modifications.Instance.MusicPitch * 100);       

        m_receptor = GameManager.Instance.Receptors[m_dir].gameObject;
        m_receptorComp = m_receptor.GetComponent<Receptor>();

        if (m_rotate)
        {
            transform.eulerAngles = new Vector3(0, 0, m_receptorComp.CurrentRotation);
        }

        // Apply all modifications
        m_spriteRenderer.sprite = m_sprite;
        m_spriteRenderer.color = m_color;
    }

    public virtual void ResetNote()
    {
        m_canUpdate = false;
        m_isHit = false;
        m_isAcitve = true;

        PostSpawn();
    }

    /// <summary>
    /// Method that gets called when all the notes are spawned
    /// </summary>
    public virtual void PostSpawn()
    {
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!m_canUpdate || !m_isAcitve)
            return;

        NoteUpdate();
    }

    protected virtual void NoteUpdate()
    {
        // update code here
        if(m_rotate)
        {
            float angle = DistanceInUnits * (10 * Modifications.Instance.NoteTwist);
            float receptorAngle = (m_receptorComp.BaseRotation + m_receptorComp.GetRotationAtTime(m_songPosition));
            transform.eulerAngles = new Vector3(0, 0, receptorAngle + (angle * System.Convert.ToInt32(Modifications.Instance.NoteTwist != 0)));

            float scale = 1;

            switch (Modifications.Instance.NoteScaleMode)
            {
                case NoteScaleMode.none:
                    scale = 1;
                    break;

                case NoteScaleMode.appear:
                    scale = 1 - Mathf.Clamp01((DistanceInUnits - 3) / 3f);
                    break;

                case NoteScaleMode.disappear:
                    scale = Mathf.Clamp01((DistanceInUnits - 2) / 3f);
                    break;

                case NoteScaleMode.bigboy:
                    scale = 3;
                    break;

                case NoteScaleMode.jumping:
                    scale = 1 + Mathf.Abs(Mathf.Sin(DistanceInUnits / 2.0f));
                    break;
            }


            transform.localScale = new Vector3(scale, scale, 1);
        }
        else
        {
            transform.localScale = Vector3.one;
        }

    }

    protected void SetPositionToBar(bool clampToReceptor)
    {
        float scrollDir = 0;
        switch (Modifications.Instance.ScrollDirection)
        {
            case ScrollDirction.down:
                scrollDir = 1.0f;
                break;

            case ScrollDirction.up:
                scrollDir = -1.0f;
                break;
        }

        // Set the position
        float y = (m_receptor.transform.position.y) + ((Distance) * m_speed) * scrollDir;

        if (Modifications.Instance.SlowdownMod == 1)
            y = (m_receptor.transform.position.y) + ((Distance * (Distance)) * m_speed) * scrollDir;

        Vector2 pos = new Vector2
        {
            x = m_receptor.transform.position.x,
            y = y
        };

        if (clampToReceptor)
        {
            switch (Modifications.Instance.ScrollDirection)
            {
                case ScrollDirction.down:
                    pos.y = Mathf.Clamp(pos.y, m_receptor.transform.position.y, pos.y + 1);
                    break;

                case ScrollDirction.up:
                    pos.y = Mathf.Clamp(pos.y, pos.y - 1, m_receptor.transform.position.y);
                    break;
            }
        }


        transform.position = pos;
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
        m_isAcitve = false;
        transform.localScale = Vector3.zero;
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

    public float DistanceInUnits
    {
        get { return transform.position.y - m_receptor.transform.position.y; }
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

    public Color NoteColor(float barpos)
    {
        Color color = new Color();

        switch (Modifications.Instance.NoteColors)
        {
            case NoteColors.beat:
                float pos = barpos % 1;
                float hue = 0;

                // Check which color index we want
                // I don't use a for here because numbers like 28, 36, 40, 44 etc are very uncommon 
                // and don't need their own color as it would make recognizing it more confusing

                if (pos % (1.0f / 4.0f) == 0)
                {
                    // Red
                    color = rgbColor(247, 44, 44);

                    hue = 4 / 360.0f;
                }
                else if (pos % (1f / 8.0f) == 0)
                {
                    // Blue
                    color = rgbColor(23, 51, 193);

                    hue = 235 / 360.0f;
                }
                else if (pos % (1f / 12.0f) == 0)
                {
                    // Purple
                    color = rgbColor(139, 52, 216);

                    hue = 266 / 360.0f;
                }
                else if (pos % (1f / 16.0f) == 0)
                {
                    // Yellow
                    color = rgbColor(237, 233, 21);

                    hue = 51 / 360.0f;
                }
                else if (pos % (1f / 24.0f) == 0)
                {
                    // Pink
                    color = rgbColor(247, 42, 165);

                    hue = 329 / 360.0f;
                }
                else if (pos % (1f / 32.0f) == 0)
                {
                    // Orange
                    color = rgbColor(247, 148, 19);

                    hue = 25 / 360.0f;
                }
                else if (pos % (1f / 48.0f) == 0)
                {
                    // Turqoise
                    color = rgbColor(25, 234, 192);

                    hue = 62 / 360.0f;
                }
                else if (pos % (1f / 64.0f) == 0)
                {
                    // Green
                    color = rgbColor(135, 232, 18);

                    hue = 129 / 360.0f;
                }
                else
                {
                    rgbColor(255, 214, 250);

                    hue = 88 / 360.0f;
                }

                color = Color.HSVToRGB(hue, 0.71f, 0.85f);
                break;

            case NoteColors.random:
                color = Color.HSVToRGB(
                    Random.Range(0.0f, 1.0f),
                    Random.Range(0.5f, 1.0f),
                    Random.Range(0.5f, 1.0f));
                break;

            case NoteColors.white:
                color = Color.white;
                break;

            case NoteColors.adaptive:
                color = SongSelect.AverageColor;
                break;
        }

        return color;
    }
    private Color rgbColor(float red, float green, float blue)
    {
        return new Color((float)red / 255f, (float)green / 255f, (float)blue / 255f);
    }
}
