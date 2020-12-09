using SimpleEasing;
using UnityEngine;
using UnityEngine.UI;

public class GameExit : MonoBehaviour
{
    [SerializeField] private Image m_overlayImage;
    private float m_timer;
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKey(KeyCode.Escape))
        {
            m_timer += Time.deltaTime;
        }
        else
        {
            m_timer -= Time.deltaTime;
        }

        m_timer = Mathf.Clamp01(m_timer);

        if(m_timer == 1)
        {
            Application.Quit();
        }

        m_overlayImage.color = new Color(0, 0, 0, Easing.easeInQuint(m_timer, 0.0f, 1.0f, 1.0f));
    }
}
