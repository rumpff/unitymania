using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ModifierUITab : MonoBehaviour
{
    private Image m_image;
    private TextMeshPro m_titleText;
    private RectTransform m_textRect;
    private float m_textScale = 1;
    private float m_textScaleDest = 1;

    private float m_angleDest = 0;
    private float m_angle = 0;

    private void Awake()
    {
        m_titleText = transform.GetComponentInChildren<TextMeshPro>();

        m_textRect = m_titleText.GetComponent<RectTransform>();

        m_image = GetComponentInChildren<Image>();
    }

    private void Update()
    {
        // Set position
        m_angle = Mathf.LerpAngle(m_angle, m_angleDest, 30 * Time.deltaTime);
        float radius = 500;

        Vector3 tabPos = new Vector3()
        {
            x = Mathf.Cos(m_angle * Mathf.Deg2Rad) * radius,
            z = Mathf.Sin(m_angle * Mathf.Deg2Rad) * radius
        };

        Vector3 tabEuler = new Vector3()
        {
            x = 0,
            y = 0,
            z = 0
        };

        float darkness = 1 - (Mathf.Sin(m_angle * Mathf.Deg2Rad) * 0.4f + 0.2f);
        m_titleText.color = new Color(darkness, darkness, darkness);


        transform.localPosition = tabPos;
        transform.eulerAngles = tabEuler;
    }

    public void SetTitle(string title)
    {
        m_titleText.text = title;
    }

    public void SetAngle(float angle)
    {
        m_angleDest = angle;
    }
}
