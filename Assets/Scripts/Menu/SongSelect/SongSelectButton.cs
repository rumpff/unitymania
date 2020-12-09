using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleEasing;

public class SongSelectButton : MonoBehaviour
{

    [SerializeField] private Image m_thumbnail;
    [SerializeField] private TextMeshProUGUI m_titleText, m_artistText;
    [SerializeField] private RectTransform m_thumbRect;

    private RectTransform m_rectTransform;

    private MenuItemContent m_menuItemContent;


    private SongSelect m_songSelect;

    private int m_contentKey = 0;

    private float m_selectAnimationTimer = float.MaxValue;
    private float m_selectAnimationDuration = 0.2f;

    private float m_scale;

    private Color m_color;
    private Color m_colorDest;

    private float m_hue = 0;
    private float m_sat = 63 / 100.0f;
    private float m_val = 82 / 100.0f;

    private float m_thumbHeight;
    private float m_thumbWidth;

    private Vector2 m_pos;
    private Vector2 m_posOffset;
    private Vector2 m_posDest;

    private Quaternion m_rotDest;
    private float m_scaleDest;

    private bool m_initalized = false;

    private bool m_loaded = false;

    private float m_hitTimer = 0;
    private float m_hitDistance = 1200;

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
        IsSelected = false;

        m_thumbWidth = 1100; // BUTTON WIDHT DINGES
        m_scale = 1;
        m_scaleDest = 1;
        m_pos = m_rectTransform.anchoredPosition;
    } 

    public void Initalize(SongSelect songSelect, int contentId)
    {
        m_songSelect = songSelect;
        UpdateKey(contentId);

        m_initalized = true;
    }

    private void Update()
    {
        if (!m_initalized)
        {
            m_rectTransform.localScale = Vector2.zero;
            return;
        }

        // Check if the files are loaded
        if (AudioClip != null && Backdrop != null)
            m_loaded = true;
        else
            // Update the content
            ReloadStructData();

        m_posOffset = Vector2.zero;

        if (IsSelected)
        {
            float timer = Mathf.Clamp(m_selectAnimationTimer, 0.0f, m_selectAnimationDuration);
            m_scale = Easing.easeOutBack(timer, 0.6f, 0.4f, m_selectAnimationDuration);

            //m_colorDest = Color.HSVToRGB(m_hue, m_sat, m_val);
            m_colorDest = m_menuItemContent.averageColor;

            if(m_songSelect.ToFocus)
            {
                // Animation when hit
                m_posOffset.x = Easing.easeInOutQuart(m_hitTimer, 0, m_hitDistance, m_songSelect.ToFocusLength);

                m_hitTimer += Time.deltaTime;
                m_hitTimer = Mathf.Clamp(m_hitTimer, 0, m_songSelect.ToFocusLength);
            }
            else
            {
                m_hitTimer = 0;
            }
        }
        else
        {
            m_scale = Mathf.Lerp(m_scale, 0.6f, 16 * Time.deltaTime);

            m_colorDest = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        }

        m_color = Color.Lerp(m_color, m_colorDest, 8 * Time.deltaTime);


        m_pos = Vector2.Lerp(m_pos, m_posDest, 20 * Time.deltaTime);

        m_rectTransform.anchoredPosition = m_pos + m_posOffset;
        m_rectTransform.localScale = new Vector3(m_scale, m_scale, 1);

        /*
        for (int i = 0; i < m_backgroundParts.Length; i++)
        {
            m_backgroundParts[i].color = m_color;
        }
        */

        m_selectAnimationTimer += Time.deltaTime;
    }

    public void SetDestination(Vector2 pos)
    {
        m_posDest = pos;
    }

    public void RotateDest(Quaternion rotate)
    {
        m_rotDest = rotate;
    }

    public void SetPosition(Vector2 pos)
    {
        m_posDest = pos;
        m_rectTransform.anchoredPosition = pos;
    }

    public RectTransform RectTransform
    {
        get { return m_rectTransform; }
    }

    public bool IsSelected { get; set; }
    public float ScaleDest
    {
        set { m_scaleDest = value; }
    }

    public AudioClip AudioClip
    {
        get { return m_menuItemContent.musicClip; }
        set { m_menuItemContent.musicClip = value; }
    }

    public Metadata SongData
    {
        get { return m_menuItemContent.metaData; }
    }

    public Sprite Backdrop
    {
        get { return m_menuItemContent.backdrop; }
        set { m_menuItemContent.backdrop = value; }
    }

    public bool Loaded
    {
        get { return m_loaded; }
    }

    public void UpdateKey(int id)
    {
        m_contentKey = id;
        m_hue = (float)id / (float)m_songSelect.MenuItemContents.Count;

        ReloadStructData();
    }
    public int ContentKey
    {
        get { return m_contentKey; }
    }

    private void ReloadStructData()
    {
        // Load the menuitem content here
        m_menuItemContent = m_songSelect.GetMenuItemContent(m_contentKey);

        // Set the (possibly) new text's
        m_titleText.text = m_menuItemContent.metaData.title;
        m_artistText.text = m_menuItemContent.metaData.artist;

        if (m_menuItemContent.backdrop != null)
        {
            // Calculate the thumbnail width;
            float bgAspect = m_menuItemContent.backdrop.rect.height /  m_menuItemContent.backdrop.rect.width;
            m_thumbHeight = m_thumbWidth * bgAspect;

            m_thumbnail.sprite = m_menuItemContent.backdrop;
            m_thumbRect.sizeDelta = new Vector3(m_thumbWidth, m_thumbHeight, 1);
        }
    }

    public void OnSelect()
    {
        if (Modifications.Instance.MenuAnimations == 1)
        {
            m_selectAnimationTimer = 0;
            m_color = Color.white;
        }
        else // Skip animations
        {
            m_selectAnimationTimer = 99;
            //m_color = Color.white;
        }
    }
    public void OnHit()
    {
        // Currently nothing...
    }
}
