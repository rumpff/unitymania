using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowDancer : MonoBehaviour
{
    [SerializeField] private int m_rowLength, m_rowAmount;
    [SerializeField] private float m_xSpacing, m_zSpacing;
    [SerializeField] private float m_jumpDelay, m_jumpHeight, m_jumpAngle;
    [SerializeField] private GameObject m_dancerParent, m_dancerPrefab;

    private SongSelect m_songSelect;
    private DancingArrow[,] m_dancingArrows;

    private float m_songTime;
    private float m_songOffset;
    private float m_songBpm;

    private void Awake()
    {
        m_songSelect = GetComponent<SongSelect>();
    }

    void Start ()
    {
        // Initalize Arrows
        m_dancingArrows = new DancingArrow[m_rowLength, m_rowAmount];
        for(int r = 0; r < m_rowAmount; r++)
        {
            for (int i = 0; i < m_rowLength; i++)
            {
                GameObject dancingArrow = Instantiate(m_dancerPrefab, m_dancerParent.transform);
                dancingArrow.name = "DancingArrow [" + i.ToString() + "," + r.ToString() + "]";

                DancingArrow danceComponent = dancingArrow.GetComponent<DancingArrow>();

                danceComponent.Initalize();

                m_dancingArrows[i, r] = danceComponent;
            }
        }
    }

	void Update ()
    {
        if (m_songSelect.State == SongSelectState.loadingSongs || m_songSelect.State == SongSelectState.intro || Modifications.Instance.SongselectBackground == 0)
            return;

        // Update values
        m_songTime = m_songSelect.AudioSource.time;
        m_songOffset = m_songSelect.CurrentSelected.SongData.offset;
        m_songBpm = m_songSelect.CurrentSelected.SongData.bpms[0].bpm; // Update this later to work with multiple bpms


        // Dance the arrows
        float currentPos = (m_songTime - m_songOffset) * (m_songBpm / 60) * Modifications.Instance.MenuAnimations;

        for (int r = 0; r < m_rowAmount; r++)
        {
            for (int i = 0; i < m_rowLength; i++)
            {
                float distFromCenter = i - (m_rowLength / 2);
                float y = Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * (currentPos - (m_jumpDelay * r) - Mathf.Abs(distFromCenter * 0.03f)) * 0.5f * 360));
                float arrowAngle = Mathf.Sin(Mathf.Deg2Rad * (currentPos - (m_jumpDelay * r) - Mathf.Abs(distFromCenter * 0.03f)) * 0.5f * 360) * m_jumpAngle;

                float angle =  ((r + i) % 2 == 0) ? arrowAngle : arrowAngle * -1;

                m_dancingArrows[i, r].Position = new Vector3((i - (m_rowLength/2)) * m_xSpacing, y * m_jumpHeight, r * m_zSpacing);
                m_dancingArrows[i, r].Rotation = new Vector3(0, 0, angle);
            }
        }
    }
}
