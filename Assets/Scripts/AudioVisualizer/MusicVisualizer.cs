using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicVisualizer : MonoBehaviour
{
    public float m_minHeight = 15.0f;
    public float m_maxHeight = 425.0f;
    public float m_lerpInterPolation = 18f;

    private float[] m_samples;
    private float[] m_freqBand;

    private VisualizerObject[] m_visualizerObjects;
    public AudioSource m_audioSource;

	void Start ()
    {
        m_visualizerObjects = GetComponentsInChildren<VisualizerObject>();

        m_samples = new float[512];
        m_freqBand = new float[8];
    }
	
	// Update is called once per frame
	void Update ()
    {
        GetSpectrumData();
        MakeFrequencyBands();
        UpdateVisualizerObjects();
	}

    void GetSpectrumData()
    {
        m_audioSource.GetSpectrumData(m_samples, 0, FFTWindow.Blackman);
    }

    void MakeFrequencyBands()
    {
        var count = 0;
        for (int i = 0; i < 8; i++)
        {
            var average = 0f;
            var sampleCount = (int)Mathf.Pow(2, i) * 2;

            for (int j = 0; j < sampleCount; j++)
            {
                average += m_samples[count] * (count + 1);
                count++;
            }

            average /= count;
            m_freqBand[i] = average * 10;
        }
    }

    void UpdateVisualizerObjects()
    {
        for (int i = 0; i < m_visualizerObjects.Length; i++)
        {
            Vector2 size = m_visualizerObjects[i].transform.localScale;

            size.y = Mathf.Lerp(size.y, m_minHeight + m_samples[i] *
                (m_maxHeight - m_minHeight) * 5, m_lerpInterPolation * Time.deltaTime);

            m_visualizerObjects[i].transform.localScale = size;
        }
    }
}
