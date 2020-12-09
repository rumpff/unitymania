using SimpleEasing;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ModifierObject : MonoBehaviour
{
    private int m_integerValue;

    [SerializeField] private string m_title;
    [SerializeField] [TextArea] private string m_description;
    [Space(5)]

    [SerializeField] private int m_defaultValue;
    [SerializeField] private int m_minValue;
    [SerializeField] private int m_maxValue;
    [SerializeField] private bool m_hasDangerZone;
    [SerializeField] private int m_dangerZone;
    [Space(5)]

    [Tooltip("note: always loops back to 0")]
    [SerializeField] private bool m_canLoop;
    [Space(5)]

    [SerializeField] private bool m_valuesToString;
    [SerializeField] private List<string> m_stringList;
    [Space(5)]

    [SerializeField] private string m_backString;
    [Space(5)]

    [SerializeField] private bool m_valueIsKey;
    [SerializeField] private bool m_getValueFromResolutions;
    [Space(5)]

    [SerializeField] private bool m_isNoteSkin;
    [SerializeField] private bool m_isHitSound;
    public bool UpdateResolution;


    private RectTransform m_rectTransform;

    private TextMeshProUGUI m_titleText;
    private TextMeshProUGUI m_valueText;

    // Extra
    private float m_valueScale;
    private NumberAnimationValues[] m_vOffsets;

    private string m_prevString;
    private string m_stringSize;

    private int m_prevInt;
    private float m_stringAnimationStart;
    private float m_stringAnimationTime;

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();

        var childTexts = transform.GetComponentsInChildren<TextMeshProUGUI>();
        m_titleText = childTexts[0];
        m_valueText = childTexts[1];

        m_prevString = m_maxValue.ToString();

        m_valueScale = 1;

        // Set the texts
        m_titleText.text = m_title + " =";

        // Set stringsize

        m_stringSize = "";
        for (int i = 0; i < m_maxValue.ToString().Length; i++)
        {
            m_stringSize += "0";
        }
    }

    private void Start()
    {
        // Initalize offset array
        m_vOffsets = new NumberAnimationValues[m_maxValue.ToString().Length];

        for (int i = 0; i < m_vOffsets.Length; i++)
        {
            m_vOffsets[i] = new NumberAnimationValues();
        }

        m_prevInt = m_integerValue;
    }

    private void Update()
    {
        string text = "";
        bool isSelected = (IsHighlighted && ModWindow.State == ModifierWindowStates.optionSelected);

        if(m_isNoteSkin)
        {
            m_maxValue = SongSelect.SpritePacks.Count-1;
        }

        // Set the texts
        bool valueIsString = true;

        if(m_isNoteSkin)
        {
            text = SongSelect.SpritePacks[m_integerValue].packName;
        }
        else if (m_valuesToString)
        {
            int value = m_integerValue + m_minValue;

            if (value < m_stringList.Count)
            {
                text = m_stringList[value];
            }
            else
            {
                text = "overflow +" + (value - (m_stringList.Count - 1)).ToString();
            }
        }
        else if (m_valueIsKey)
        {
            if(isSelected)
            {
                text = "Press a key";
            }
            else
            {
                text = ((KeyCode)m_integerValue).ToString();
            }          
        }
        else if (m_getValueFromResolutions)
        {
            Resolution r = ModificationApplier.Instance.Resolutions[m_integerValue];
            text = r.width + "x" + r.height;
        }
        else // Display actual value
        {
            valueIsString = false;

            string val = m_integerValue.ToString(m_stringSize);
            string finaltext = "";            

            // Check for value changes
            for (int i = 0; i < val.Length; i++)
            {
                if(val[i] != m_prevString[i])
                {
                    m_vOffsets[i].Start = Mathf.Sign(int.Parse(m_prevString) - int.Parse(val)) * 0.2f;
                    m_vOffsets[i].Time = 0;
                }
            }

            // Create the string
            for (int i = 0; i < val.Length; i++)
            {
                string offset = m_vOffsets[i].Value.ToString("0.000");
                finaltext += $"<voffset={offset}em>";
                finaltext += val[i];
            }

            // aminate vOffstes
            for (int i = 0; i < m_vOffsets.Length; i++)
            {
                m_vOffsets[i].Time += Time.deltaTime * 3.0f;
            }

            finaltext += "</voffset>";


            text = finaltext;
            m_prevString = m_integerValue.ToString(m_stringSize);
        }

        if(valueIsString)
        {
            // Animate string texts too

            if(m_prevInt != m_integerValue)
            {
                m_stringAnimationStart = Mathf.Sign(m_prevInt - m_integerValue) * 0.2f;
                m_stringAnimationTime = 0;
            }
            string offset = Easing.easeOutBack(Mathf.Clamp01(m_stringAnimationTime), m_stringAnimationStart, -m_stringAnimationStart, 1).ToString("0.000");
            text = $"<space={offset}em>" + text;

            m_stringAnimationTime += Time.deltaTime * 5;
            m_prevInt = m_integerValue;
        }

        text += m_backString;

        float scaleDest = 1.0f;
        if (isSelected)
            scaleDest = 1.2f;

        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(scaleDest, scaleDest, 1.0f), 32 * Time.deltaTime);

        Color textColor = Color.white;
        if(m_integerValue == m_defaultValue)
        {
            textColor = new Color(0.6f, 0.6f, 0.6f);
        }
        else if (m_integerValue > m_dangerZone && m_hasDangerZone)
        {
            textColor = Color.red;
        }

        m_valueText.color = textColor;

        m_valueText.text = text;
        m_valueText.rectTransform.localScale = new Vector3(m_valueScale, m_valueScale, 1.0f);

        m_valueScale = Mathf.Lerp(m_valueScale, 1.0f, 15 * Time.deltaTime);
    }

    public RectTransform RectTransform
    {
        get { return m_rectTransform; }
    }

    public string Description
    {
        get { return m_description; }
    }
    public bool IsHighlighted { get; set; }
    public bool ValueIsKey
    {
        get { return m_valueIsKey; }
    }
    public bool MoveValue(int amount)
    {
        int newVal = m_integerValue + amount;

        if(m_canLoop)
        {
            newVal = mod(newVal, m_maxValue + 1);
        }

        if ((newVal) <= m_maxValue && (newVal) >= m_minValue)
        {
            m_integerValue = newVal;
            return true;
        }

        return false;
    }
    public object GetValue()
    {
        return m_integerValue;
    }
    public void SetValue(int value)
    {
        m_integerValue = value;
    }
    public void SetDefault()
    {
        m_integerValue = m_defaultValue;
    }
    public ModifierWindow ModWindow { get; set; }

    /// <summary>
    /// mod function which allows negative numbers
    /// </summary>
    /// <param name="x"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    int mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    string reverse(string s)
    {
        if (s == null)
            return null;

        char[] arr = s.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }

    public struct NumberAnimationValues
    {
        public float Start;
        public float Time;

        public float Value
        {
            get
            {
                return Easing.easeOutElastic(Mathf.Clamp01(Time), Start, -Start, 1);
            }
        }
    }

    public bool IsHitSound
    {
        get { return m_isHitSound; }
    }

    public string TitleText
    {
        get { return m_titleText.text; }
    }

    public string ValueText
    {
        get { return m_valueText.text; }
    }

    public float ValueScale
    {
        get { return m_valueScale; }
    }
}
