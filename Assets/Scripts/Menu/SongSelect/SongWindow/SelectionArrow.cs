using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionArrow : MonoBehaviour
{
    [SerializeField] private float m_angle, m_distance;

    private RectTransform[] m_objects;

    private void Start()
    {
        m_objects = transform.GetComponentsInChildren<RectTransform>();
    }

    private void Update()
    {
        for (int i = 0; i < m_objects.Length; i++)
        {
            float angle = (m_angle + (i * 90)) * Mathf.Deg2Rad;

            float x = Mathf.Cos(angle) * m_distance;
            float y = Mathf.Sin(angle) * m_distance;

            m_objects[i].anchoredPosition = new Vector2(x, y);
        }
    }
}
