using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DancingArrow : MonoBehaviour
{
    private SpriteRenderer m_sprite;

    public void Initalize()
    {
        m_sprite = GetComponent<SpriteRenderer>();
        m_sprite.color = Color.HSVToRGB(Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
    }

    public Vector3 Position
    {
        get { return transform.localPosition; }
        set { transform.localPosition = value; }
    }

    public Vector3 Rotation
    {
        get { return transform.localEulerAngles; }
        set { transform.localEulerAngles = value; }
    }

    public Vector3 Scale
    {
        get { return transform.localScale; }
        set { transform.localScale = value; }
    }
}
