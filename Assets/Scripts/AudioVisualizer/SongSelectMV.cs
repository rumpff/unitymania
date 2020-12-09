using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongSelectMV : MonoBehaviour
{
    [SerializeField] private bool m_UI;
    [Tooltip("for non ui")]
    [SerializeField] private Vector3 m_spacing;
    [SerializeField] private SongSelect m_songSelect;
    [SerializeField] private GameObject m_visualizerBar;
    [SerializeField] private float m_magnitude;

    private readonly int m_barAmount = 64;

    private AudioPeer m_audioPeer;
    private Color m_barColor;

    private List<VisualizerBar> m_visualizerBars;

	void Start ()
    {
        m_audioPeer = GetComponent<AudioPeer>();
        m_barColor = Color.white;

        m_visualizerBars = new List<VisualizerBar>();
        for (int i = 0; i < m_barAmount; i++)
        {
            var obj = Instantiate(m_visualizerBar, this.transform);
            //obj.transform.position = new Vector3(i - 32, 0, 12);
            m_visualizerBars.Add(obj.AddComponent<VisualizerBar>());

            //obj.GetComponent<Image>().color = Color.HSVToRGB((i / 64.0f), 1, 1);
            //obj.GetComponent<Shadow>().effectColor = Color.HSVToRGB((i / 64.0f), 1f, 0.4f);

            if(!m_UI)
            {
                obj.transform.localPosition = m_spacing * i;
            }
        }
    }
	
	void Update ()
    {
        if (m_songSelect.State != SongSelectState.focus && m_songSelect.State != SongSelectState.preview)
            return;

        Color colorDest = m_songSelect.GetMenuItemContent(m_songSelect.CurrentSelected.ContentKey).averageColor;
        m_barColor = Color.Lerp(m_barColor, colorDest, 7 * Time.deltaTime);

        for (int i = 0; i < m_barAmount; i++)
        {
            var y = m_audioPeer._audioBandBuffer[i] * m_magnitude;
            if(y < 0.01f) { y = 0.01f; }

            if (m_UI)
            {
                m_visualizerBars[i].RectTransform.sizeDelta = new Vector2(1, y);
                //m_visualizerBars[i].transform.localScale = new Vector3(1, y, 1);

                float h, s, v;
                Color.RGBToHSV(m_barColor, out h, out s, out v);

                m_visualizerBars[i].Image.color = m_barColor;
                m_visualizerBars[i].SetShadowColor(Color.HSVToRGB(h, s, v - 0.15f));
                //m_visualizerBars[i].Shadow.effectColor = Color.HSVToRGB((i / 64.0f), 1f, 0.4f);
            }
            else
            {
                m_visualizerBars[i].transform.localScale = new Vector3(m_visualizerBars[i].transform.localScale.x, y, m_visualizerBars[i].transform.localScale.z);
            }
        }
    }
}
