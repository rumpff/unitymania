using UnityEngine;
using SimpleEasing;

public class GameCamera : MonoBehaviour
{
    private Vector3 m_originalPos;
    private GameObject m_backdropCamera;

    private float m_screenshakeAmount;
    private float m_screenshakeTime = 0;

    private readonly float m_screenshakeMaxPosition = 2;
    private readonly float m_screenshakeMaxRotation = 20;

    private bool m_camIdle = true;
    private float m_fromIdleTimer = 0;
    private float m_fromIdleLength = 1;

	void Start ()
    {
        var yBase = 0.0f;

        switch (Modifications.Instance.ScrollDirection)
        {
            case ScrollDirction.down:
                yBase = 3.5f;
                break;

            case ScrollDirction.up:
                yBase = -3.5f;
                break;
        }

        transform.position = new Vector3
        {
            x = -(Modifications.Instance.ReceptorPosition * 5),
            y = yBase + (Modifications.Instance.ReceptorHeight * 0.5f),
            z = -5
        };

        // Make sure the backdrop camera is centered
        m_backdropCamera = transform.GetChild(0).gameObject;
        m_backdropCamera.transform.position = Vector3.zero;

        m_originalPos = transform.position;
        GameManager.Instance.GameplayManager.NoteHitEvent += OnNoteHit;
    }
    private void OnDestroy()
    {
        GameManager.Instance.GameplayManager.NoteHitEvent -= OnNoteHit;
    }

    void Update ()
    {
        Vector3 pos = m_originalPos;

        // Intro animation
        if(!m_camIdle)
        {
            pos.z = Easing.easeOutElastic(m_fromIdleTimer, -10, 5, m_fromIdleLength);

            m_fromIdleTimer += Time.deltaTime;
            m_fromIdleTimer = Mathf.Clamp(m_fromIdleTimer, 0, m_fromIdleLength);
        }
        else
        {
            pos.z = -999;
        }

        // Screenshake
        pos.x += (Mathf.PerlinNoise((Time.time + m_screenshakeTime) * 4.5f, 3) * m_screenshakeMaxPosition) * m_screenshakeAmount;
        pos.y += (Mathf.PerlinNoise((Time.time + m_screenshakeTime) * 4.5f, 4) * m_screenshakeMaxPosition) * m_screenshakeAmount;

        m_screenshakeAmount = Mathf.Lerp(m_screenshakeAmount, 0, 6 * Time.deltaTime);

        transform.position = pos;
    }

    public void ResetCamera()
    {
        m_camIdle = true;
        m_fromIdleTimer = 0;
    }

    public void AddScreenShake(float amount)
    {
        m_screenshakeAmount += amount;
        m_screenshakeTime += Random.Range(0.5f, 200.0f);
    }
    public void OnNoteHit(NoteTypes t, Directions d, Judgements j)
    {
        // Note is hit
        if(j != Judgements.miss)
        {
            AddScreenShake(1.0f * (Modifications.Instance.ScreenshakeIntensity / 100.0f));
        }
    }

    public bool Idle
    {
        get { return m_camIdle; }
        set { m_camIdle = value; }
    }
}
