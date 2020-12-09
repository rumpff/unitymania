using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualizerObject : MonoBehaviour
{
    [SerializeField] private MusicVisualizer m_musicVisualizer;

    [SerializeField] [Range(0, 7)] private int m_band;

    [SerializeField] private float m_startScale, m_scaleMultiplier;


}
