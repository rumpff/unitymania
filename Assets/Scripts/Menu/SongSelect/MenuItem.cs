using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class MenuItem : MonoBehaviour
{
    [SerializeField] private Color m_activeColor, m_inactiveColor, m_loadingColor;

    private SongSelect m_songSelect;

    private MenuItemContent m_menuItemContent;
    private int m_contentKey = 0;

    private Vector2 m_dest;
    private Quaternion m_rotDest;
    private RectTransform m_rectTransform;

    private bool m_selected;
    private bool m_initalized = false;

    private List<TextMeshProUGUI> m_texts;

    private bool m_loaded = false;

    private int m_noteAmount;
    private int m_holdAmount;

    private float m_imageWidth = 12;
    private float m_textWidth;

    private void Awake()
    {
        m_texts = new List<TextMeshProUGUI>();

        for (int i = 0; i < transform.childCount; i++)
        {
            var component = transform.GetChild(i).GetComponent<TextMeshProUGUI>();

            if (component != null)
            {
                m_texts.Add(component);
            }
        }

        m_rectTransform = transform as RectTransform;
    }

    public void Initalize(SongSelect songSelect, int contentId)
    {
        m_songSelect = songSelect;
        UpdateKey(contentId);

        m_rectTransform.sizeDelta = new Vector2(20, 90);
        m_initalized = true;
    }

    private void LateUpdate()
    {
        if (!m_initalized)
            return;

        // Check if the files are loaded
        if (AudioClip != null && Backdrop != null)
            m_loaded = true;
        else
            // Update the content
            ReloadStructData();

        m_textWidth = Mathf.Max(m_texts[0].textBounds.max.x, m_texts[1].textBounds.max.x) + 24;

        m_rectTransform.anchoredPosition = Vector2.Lerp(m_rectTransform.anchoredPosition, m_dest, 10 * Time.deltaTime);
        m_rectTransform.rotation = Quaternion.Slerp(m_rectTransform.rotation, m_rotDest, 10 * Time.deltaTime);

        for (int i = 0; i < m_texts.Count; i++)
        {
            if (m_selected)
                m_texts[i].color = m_activeColor;
            else if(!Loaded)
                m_texts[i].color = m_loadingColor;
            else

                m_texts[i].color = m_inactiveColor;
        }

        float widthDest;

        if(m_selected)
        {
            widthDest = m_textWidth;
        }
        else
        {
            widthDest = 12;
        }

        m_imageWidth = Mathf.Lerp(m_imageWidth, widthDest, 16 * Time.deltaTime);


        m_rectTransform.sizeDelta = new Vector2(m_imageWidth, 90);
    }

    public void SetDestination(Vector2 pos)
    {
        m_dest = pos;
    }

    public void RotateDest(Quaternion rotate)
    {
        m_rotDest = rotate;
    }

    public void SetPosition(Vector2 pos)
    {
        m_dest = pos;
        m_rectTransform.anchoredPosition = pos;
    }

    public RectTransform RectTransform
    {
        get { return m_rectTransform; }
    }

    public bool Selected
    {
        get { return m_selected; }
        set { m_selected = value; }
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
        m_texts[0].text = m_menuItemContent.metaData.title;
        m_texts[1].text = m_menuItemContent.metaData.artist;
    }
}
