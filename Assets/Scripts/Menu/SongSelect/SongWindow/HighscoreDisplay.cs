using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SimpleEasing;

public class HighscoreDisplay : MonoBehaviour
{
    [Tooltip("prefab that'll be used for creating more text objects")]
    [SerializeField] private TextMeshProUGUI m_highscorePrefab;
    private RectTransform m_rectTransform;

    private List<TextMeshProUGUI> m_highscoreTexts;
    private List<TextMeshProUGUI> m_disabledTexts;
    private List<ScoreData> m_highScores;

    private List<float> m_oldPositions;
    private List<float> m_newPositions;

    [Space(5)]
    [SerializeField] private float m_textSpacing;
    [SerializeField] private float m_xOffset;
    [SerializeField] private float m_yOffset;
    [SerializeField] private int m_maxVisibleScores;
    
    private ActiveState m_state;

    private int m_selectedIndex;

    private float m_entranceTimer;

    private bool m_highscoreEmpty = true;

    [SerializeField] private float m_scrollLength;
    private float m_scrollTimer;
    private int m_scrollDistance = 1;

    private void Start()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_highscoreTexts = new List<TextMeshProUGUI>();
        m_disabledTexts = new List<TextMeshProUGUI>();
    }

    private void Update()
    {
        // Handle state specifics
        switch (m_state)
        {
            case ActiveState.inactive:
                {
                    m_entranceTimer = 0;
                }
                break;

            case ActiveState.active:
                {
                    m_entranceTimer += Time.deltaTime;
                    m_scrollTimer += Time.deltaTime;
                }
                break;
        }


        // Set the size for the masking
        m_rectTransform.sizeDelta = new Vector2()
        {
            x = 600 + m_xOffset,
            y = m_maxVisibleScores * m_textSpacing
        };

        // Position the texts
        int textAmount = m_highscoreTexts.Count;

        for (int i = 0; i < m_highscoreTexts.Count; i++)
        {
            RectTransform obj = m_highscoreTexts[i].rectTransform;

            float xTimer = Mathf.Clamp01(m_entranceTimer - (i * 0.05f));
            float yTimer = Mathf.Clamp(m_scrollTimer, 0, m_scrollLength);

            Vector2 position = new Vector2()
            {
                x = Easing.easeInOutExpo(xTimer, -SizeDelta.x, SizeDelta.x + m_xOffset, 1),
                //y = -(i - centerIndex) * m_textSpacing
                y = Easing.easeOutBack(yTimer, m_oldPositions[i], m_newPositions[i] - m_oldPositions[i] + m_yOffset, m_scrollLength)
            };

            obj.anchoredPosition = position;

            // Set the selection highlight
            bool isSelected = (m_selectedIndex == i);

            Color c = isSelected ? Color.white : Color.gray;
            m_highscoreTexts[i].color = Color.Lerp(m_highscoreTexts[i].color, c, 25 * Time.deltaTime);
            obj.localScale = Vector3.Lerp(obj.localScale, Vector3.one * (isSelected ? 1.2f : 1.0f), 16 * Time.deltaTime);
        }
    }

    public bool MoveSelection(int amount)
    {
        int newIndex = m_selectedIndex + amount;

        if (newIndex >= 0 && newIndex < m_highScores.Count)
        {
            m_selectedIndex = newIndex;
            m_scrollDistance = amount;
            m_scrollTimer = 0;

            // Calculate the button's new positions
            int textAmount = m_highscoreTexts.Count;
            float centerIndex;

            if (textAmount >= m_maxVisibleScores)
            {
                centerIndex = Mathf.Clamp(m_selectedIndex, (int)(m_maxVisibleScores / 2), (textAmount - 1) - ((int)(m_maxVisibleScores / 2)));
            }
            else
            {
                centerIndex = (textAmount - 1) / 2.0f;
            }

            for (int i = 0; i < m_highscoreTexts.Count; i++)
            {
                RectTransform obj = m_highscoreTexts[i].rectTransform;

                m_oldPositions[i] = obj.anchoredPosition.y;
                m_newPositions[i] = -(i - centerIndex) * m_textSpacing;
            }
			
			AudioManager.Instance.PlaySound(Sounds.HighscoreSelect);
            return true;
        }
        else
        {
            // Can't move selection
			AudioManager.Instance.PlaySound(Sounds.HighscoreSelectError);
            return false;
        }
    }

    public void SetNewHighscores(List<ScoreData> highScores)
    {
        m_highScores = highScores;
        m_highscoreEmpty = false;

        int amountDifference = m_highScores.Count - m_highscoreTexts.Count;

        // Make sure that there exactly enough text objects active
        int signDifference = Utility.GamemakerSign(amountDifference);

        while (amountDifference != 0)
        {
            switch ((int)Mathf.Sign(amountDifference))
            {
                // There are too much text objects active
                case -1:
                    {
                        TextMeshProUGUI txt = m_highscoreTexts.Last();
                        DeactivateText(txt);
                    }
                    break;

                // There are not enough text objects
                case 1:
                    {
                        // Check if there are any stored objects left

                        if (m_disabledTexts.Count > 0)
                        {
                            TextMeshProUGUI txt = m_disabledTexts.Last();

                            txt.gameObject.SetActive(true);

                            m_highscoreTexts.Add(txt);
                            m_disabledTexts.Remove(txt);
                        }
                        else // We need to create new objects
                        {
                            TextMeshProUGUI txt = Instantiate(m_highscorePrefab, m_rectTransform);
                            m_highscoreTexts.Add(txt);
                        }
                    }
                    break;
            }

            // Update the difference
            amountDifference = m_highScores.Count - m_highscoreTexts.Count;
        }


        // Sort the list
        m_highScores.Sort((s1, s2) => s1.Score.CompareTo(s2.Score));

        // Sort() sorts ascending, but we want the highest score to be on the top
        m_highScores.Reverse();

        // Set the texts to their highscores
        for (int i = 0; i < m_highScores.Count; i++)
        {
            m_highscoreTexts[i].text = ParseHighscore(m_highScores[i]);
        }

        // Reset the selection
        m_selectedIndex = 0;

        // Set the buttons's positions
        
        m_oldPositions = new List<float>();
        m_newPositions = new List<float>();

        int textAmount = m_highScores.Count;
        float centerIndex;

        if (textAmount >= m_maxVisibleScores)
        {
            centerIndex = Mathf.Clamp(m_selectedIndex, (int)(m_maxVisibleScores / 2), (textAmount - 1) - ((int)(m_maxVisibleScores / 2)));
        }
        else
        {
            centerIndex = (textAmount - 1) / 2.0f;
        }

        for (int i = 0; i < m_highScores.Count; i++)
        {
            float y = -(i - centerIndex) * m_textSpacing;

            m_oldPositions.Add(y);
            m_newPositions.Add(y);
        }
    }
    
    public void SetHighscoresEmpty()
    {
        m_highScores = new List<ScoreData>();
        m_highscoreEmpty = true;

        int amountDifference = 1 - m_highscoreTexts.Count;

        // Make sure that there exactly enough text objects active
        int signDifference = Utility.GamemakerSign(amountDifference);

        while (amountDifference != 0)
        {
            switch ((int)Mathf.Sign(amountDifference))
            {
                // There are too much text objects active
                case -1:
                    {
                        TextMeshProUGUI txt = m_highscoreTexts.Last();
                        DeactivateText(txt);
                    }
                    break;

                // There are not enough text objects
                case 1:
                    {
                        // Check if there are any stored objects left

                        if (m_disabledTexts.Count > 0)
                        {
                            TextMeshProUGUI txt = m_disabledTexts.Last();

                            txt.gameObject.SetActive(true);

                            m_highscoreTexts.Add(txt);
                            m_disabledTexts.Remove(txt);
                        }
                        else // We need to create new objects
                        {
                            TextMeshProUGUI txt = Instantiate(m_highscorePrefab, m_rectTransform);
                            m_highscoreTexts.Add(txt);
                        }
                    }
                    break;
            }

            // Update the difference
            amountDifference = 1 - m_highscoreTexts.Count;
        }

        // Set the texts to their highscores
        for (int i = 0; i < 1; i++)
        {
            m_highscoreTexts[i].text = "no highscores\nfound";
        }

        // Reset the selection
        m_selectedIndex = 0;

        // Set the buttons's positions
        m_oldPositions = new List<float>();
        m_newPositions = new List<float>();

        int textAmount = 1;
        float centerIndex;

        centerIndex = (textAmount - 1) / 2.0f;

        for (int i = 0; i < 1; i++)
        {
            float y = -(i - centerIndex) * m_textSpacing;

            m_oldPositions.Add(y);
            m_newPositions.Add(y);
        }
    }

    private string ParseHighscore(ScoreData hs)
    {
        string output = string.Empty;

        output += hs.Rating.ToString();
        output += " - ";
        output += hs.Score;

        return output;
    }

    private void DeactivateText(TextMeshProUGUI txt)
    {
        txt.gameObject.SetActive(false);

        m_disabledTexts.Add(txt);
        m_highscoreTexts.Remove(txt);
    }

    public bool ContainsScores
    {
        get { return !m_highscoreEmpty; }
    }

    public ScoreData SelectedHighscore
    {
        get { return m_highScores[m_selectedIndex]; }
    }

    public List<ScoreData> HighScores
    {
        get { return m_highScores; }
    }
    public TextMeshProUGUI[] HighscoreTexts
    {
        get { return HighscoreTexts; }
    }

    public Vector2 SizeDelta
    {
        get { return m_rectTransform.sizeDelta; }
    }

    public Vector2 Position
    {
        get { return m_rectTransform.anchoredPosition; }
        set { m_rectTransform.anchoredPosition = value; }
    }
    public ActiveState State
    {
        get { return m_state; }
        set { m_state = value; }
    }
    public int SelectedIndex
    {
        get { return m_selectedIndex; }
    }

    private int RoundUp(float n)
    {
        int o = (int)n;
        if (n > o) o++;
        return o;
    }
}
