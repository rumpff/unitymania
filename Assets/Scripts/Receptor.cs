using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleEasing;

public class Receptor : MonoBehaviour
{
    [SerializeField] private Directions m_direction;
    private ParticleSystem m_particleSystem;

    private float m_scale = 0;
    private float m_scaleTimer = 0;

    private Vector3 m_defaultPos;
    private float m_rotation = 0;
    private float m_baseRotation = 0;

    private float m_emmisionScale = 0;
    
	void Start ()
    {
        m_particleSystem = transform.Find("ParticleSystem").GetComponent<ParticleSystem>();

        ParticleSystem.MainModule ps = m_particleSystem.main;
        ps.startSpeed = new ParticleSystem.MinMaxCurve(0, Modifications.Instance.ParticleSpeed);

        switch (m_direction)
        {
            case Directions.left:
                m_baseRotation = -90;
                InputHandler.Instance.KeyLeftHeldEvent += KeyPressed;
                break;

            case Directions.down:
                m_baseRotation = 0;
                InputHandler.Instance.KeyDownHeldEvent += KeyPressed;
                break;

            case Directions.up:
                m_baseRotation = 180;
                InputHandler.Instance.KeyUpHeldEvent += KeyPressed;
                break;

            case Directions.right:
                m_baseRotation = 90;
                InputHandler.Instance.KeyRightHeldEvent += KeyPressed;
                break;
        }

        m_defaultPos = transform.position;
        transform.eulerAngles = new Vector3(0, 0, m_baseRotation);

        // Get and apply the sprite
        GetComponent<SpriteRenderer>().sprite = GameManager.Instance.SpritePack.receptor;

        m_emmisionScale = Modifications.Instance.ParticleEmmisionScale;
    }
    private void OnDestroy()
    {
        switch (m_direction)
        {
            case Directions.left:
                InputHandler.Instance.KeyLeftHeldEvent -= KeyPressed;
                break;

            case Directions.down:
                InputHandler.Instance.KeyDownHeldEvent -= KeyPressed;
                break;

            case Directions.up:
                InputHandler.Instance.KeyUpHeldEvent -= KeyPressed;
                break;

            case Directions.right:
                InputHandler.Instance.KeyRightHeldEvent -= KeyPressed;
                break;
        }
    }

    void Update ()
    {
        // Update scale
        m_scaleTimer += Time.deltaTime;
        if (m_scaleTimer > 0.1f) { m_scaleTimer = 0.1f; }
        m_scale = Easing.easeOutQuad(m_scaleTimer, 0.9f, 0.1f, 0.1f);
        transform.localScale = new Vector2(m_scale, m_scale);

        Vector3 pos = m_defaultPos;

        if(Modifications.Instance.ReceptorWaveHor != 0)
        {
            float songTime = GameManager.Instance.ChartManager.SongTime * (Modifications.Instance.ReceptorWaveSpeed / 100.0f);
            pos.x += (Mathf.Sin(songTime) * Modifications.Instance.ReceptorWaveHor);
        }

        if (Modifications.Instance.ReceptorWaveVer != 0)
        {
            float songTime = GameManager.Instance.ChartManager.SongTime * (Modifications.Instance.ReceptorWaveSpeed / 100.0f);
            pos.y -= (Mathf.Sin(songTime + ((int)m_direction / 4.0f * Mathf.PI)) * (Modifications.Instance.ReceptorWaveVer / 3.0f));
        }

        if(Modifications.Instance.ReceptorTwist != 0)
        {
            float songTime = GameManager.Instance.ChartManager.SongTime;
            m_rotation = GetRotationAtTime(songTime);
        }

        transform.position = pos;
        transform.eulerAngles = new Vector3(0, 0, m_baseRotation + m_rotation);

        // Make sure the particle system doesn't rotate with the receptor
        m_particleSystem.transform.eulerAngles = Vector3.zero;
    }

    public float GetRotationAtTime(float time)
    {
        return time * (90 * Modifications.Instance.ReceptorTwist);
    }
    public void Activate(bool noteHit)
    {
        if (noteHit)
        {
            float emitAmount = Random.Range(20, 40) * m_emmisionScale;
            m_particleSystem.Emit((int)emitAmount);
        }
    }
    public void KeyPressed()
    {
        m_scaleTimer = 0;
    }  

    public Directions Direction
    {
        get { return m_direction; }
    }

    public float BaseRotation
    {
        get { return m_baseRotation; }
    }
    public float CurrentRotation
    {
        get { return m_baseRotation + m_rotation; }
    }
}
