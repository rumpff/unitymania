using UnityEngine;
using UnityEngine.UI;

public class VisualizerBar : MonoBehaviour
{

    public RectTransform RectTransform { get; private set; }
    public Image Image { get; private set; }
    public Shadow[] Shadows { get; private set; }

    void Start ()
    {
        RectTransform = GetComponent<RectTransform>();
        Image = GetComponent<Image>();
        Shadows = GetComponents<Shadow>();
    }

    public void SetShadowColor(Color color)
    {
        for (int i = 0; i < Shadows.Length; i++)
        {
            Shadows[i].effectColor = color;
        }
    }
}
