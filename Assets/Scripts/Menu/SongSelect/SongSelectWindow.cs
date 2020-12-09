using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using SimpleEasing;


public class SongSelectWindow : MonoBehaviour
{
    private enum FocusState
    {
        Diffselect,
        ToFinalize,
        Finalize
    };

    public int m_fakeHighscoreAmount;

    [SerializeField] private AudioClip m_diffSelectSound;

    [SerializeField] private SongSelect m_songSelect;

    [SerializeField] private HighscoreDisplay m_highscoreDisplay;

    [SerializeField] private List<RectTransform> m_lines;
    [SerializeField] private float m_lineThickness;
    [SerializeField] private List<RectTransform> m_arrows;
    [SerializeField] private RectTransform m_difficultyParent;
    [SerializeField] private RectTransform m_difficultyBackground;
    [SerializeField] private TextMeshProUGUI m_effectText;
    private Dictionary<Difficulties, TextMeshProUGUI> m_difficultyTexts;

    [SerializeField] private float m_difficultyTextSpacing;
    [Space(5)]

    [SerializeField] private RectTransform m_startTextParent;
    [SerializeField] private TextMeshProUGUI m_accuracyText, m_comboText, m_timeText, m_startText;

    private RectTransform m_rect;

    private SongSelectButton m_currentItem;

    /* 0 = Title
     * 1 = Artist 
     * 2 = Left Bottom
     * 3 = Right Bottom */
    private List<TextMeshProUGUI> m_texts;

    private SSPText m_title;
    private SSPText m_artist;
    private SSPText m_bottomLeft;
    private SSPText m_bottomRight;
    

    private SSPText m_noteAmount;
    private SSPText m_holdAmount;

    private Dictionary<Difficulties, bool> m_songDifficulties;
    private List<Difficulties> m_activeDifficulties;
    private Difficulties m_selectedDifficulty;

    private Dictionary<Difficulties, int> m_noteAmounts;
    private Dictionary<Difficulties, int> m_holdAmounts;

    private RectSize m_previewSize;
    private RectSize m_focusSize;

    private RectSize m_oldSize;
    private RectSize m_currentSize;


    private SongSelectState m_state;
    private FocusState m_focusState;
    private Dictionary<FocusState, float> m_stateTimers;

    private float m_sizeEaseTimer = 0.999f; // Make it right below 1 so that the size starts on it's final position

    private float m_difficultyPointerYStart = 0;
    private float m_difficultyPointerYEnd = 0;
    private float m_difficultyPointerTimer = 0;

    private float m_effectTextTimer = Int32.MaxValue;

    private float m_highscoreScrollbarY = 0;

    float m_infoWidth = 620;
    float m_infoY = 10;
    float m_infoTimer = 0;

    private void Awake()
    {
        m_rect = GetComponent<RectTransform>();
        m_texts = new List<TextMeshProUGUI>();

        m_songDifficulties = new Dictionary<Difficulties, bool>();
        m_difficultyTexts = new Dictionary<Difficulties, TextMeshProUGUI>();

        m_noteAmounts = new Dictionary<Difficulties, int>();
        m_holdAmounts = new Dictionary<Difficulties, int>();

        m_focusState = FocusState.Diffselect;

        foreach (Difficulties diff in (Difficulties[])Enum.GetValues(typeof(Difficulties)))
        {
            m_noteAmounts[diff] = 0;
            m_holdAmounts[diff] = 0;
        }

        for (int i = 0; i < Enum.GetValues(typeof(Difficulties)).Length; i++)
        {
            m_difficultyTexts[(Difficulties)i] = m_difficultyParent.GetChild(i).GetComponent<TextMeshProUGUI>();
        }

        // Reset the timers
        m_stateTimers = new Dictionary<FocusState, float>();
        foreach (FocusState state in (FocusState[])Enum.GetValues(typeof(FocusState)))
        {
            m_stateTimers.Add(state, 0.0f);
        }

        CreateRectSizes();

        for (int i = 0; i < transform.childCount; i++)
        {
            var component = transform.GetChild(i).GetComponent<TextMeshProUGUI>();

            if (component != null)
            {
                m_texts.Add(component);
            }
        }

        InitalizeSSP(ref m_title);
        InitalizeSSP(ref m_artist);
        InitalizeSSP(ref m_bottomLeft);
        InitalizeSSP(ref m_bottomRight);

        InitalizeSSP(ref m_noteAmount);
        InitalizeSSP(ref m_holdAmount);


        var inputHandler = InputHandler.Instance;

        // Subscribe to the keyinput events
        inputHandler.KeyUpEvent += KeyUp;
        inputHandler.KeyDownEvent += KeyDown;
        inputHandler.KeyStartEvent += KeyStart;
    }
    private void OnDisable()
    {
        var inputHandler = InputHandler.Instance;

        // Unsubscribe to the keyinput events
        inputHandler.KeyUpEvent -= KeyUp;
        inputHandler.KeyDownEvent -= KeyDown;
        inputHandler.KeyStartEvent -= KeyStart;
    }

    private void Update()
    {
        if (!m_songSelect.m_started)
            return;

        m_rect.localScale = Vector3.one;

        UpdateWindowSize();
        SetTextSettings();
        UpdateFocusText();
        UpdateTitleTexts();
        FocusedUpdate();
        UpdateEffectText();
    }
    private void FixedUpdate()
    {
        /// The animation for the text transition happens in the fixed update
        /// because it then has the same speed whatever the framerate is

        // Create the new strings
        StringTransition(ref m_title);
        StringTransition(ref m_title);

        StringTransition(ref m_artist);

        StringTransition(ref m_bottomLeft);
        StringTransition(ref m_bottomRight);

        StringTransition(ref m_noteAmount);
        StringTransition(ref m_holdAmount);

        // Apply the strings
        m_texts[0].text = ListToString(m_title.text);
        m_texts[1].text = ListToString(m_artist.text);
        m_texts[2].text = ListToString(m_bottomLeft.text);
        m_texts[3].text = ListToString(m_bottomRight.text);
    }
    // Updates the little effect when the player selects a difficulty
    private void UpdateEffectText()
    {
        float duration = 1.0f;
        float scale = 1;
        float alpha = 0;

        Color color = m_effectText.color;

        if (m_state != SongSelectState.focus)
            m_effectTextTimer = duration;

        alpha = Easing.easeOutExpo(m_effectTextTimer, 1, -1, duration);
        scale = Easing.easeOutExpo(m_effectTextTimer, 1, 2, duration);

        m_effectText.rectTransform.localScale = new Vector3(scale, scale, 1.0f);
        m_effectText.color = new Color(color.r, color.g, color.b, alpha);

        m_effectTextTimer += Time.deltaTime;
        m_effectTextTimer = Mathf.Clamp(m_effectTextTimer, 0, duration);
    }
    /// <summary>
    /// Initalize for the focus state
    /// </summary>
    private void EnterFocusState()
    {
        m_activeDifficulties = new List<Difficulties>();

        foreach (Difficulties diff in (Difficulties[])Enum.GetValues(typeof(Difficulties)))
        {
            if (m_songDifficulties[diff])
            {
                m_activeDifficulties.Add(diff);
            }
        }

        m_selectedDifficulty = m_activeDifficulties[0];

        /// The buttons need to be positioned correctly so that we can give the correct position to the pointers
        // First, set all the diff texts inactive
        foreach (Difficulties diff in (Difficulties[])Enum.GetValues(typeof(Difficulties)))
        {
            m_difficultyTexts[diff].gameObject.SetActive(false);
        }

        // Reset the state timers
        foreach (FocusState state in (FocusState[])Enum.GetValues(typeof(FocusState)))
        {
            m_stateTimers[state] = 0.0f;
        }

        int diffAmount = m_activeDifficulties.Count;

        for (int i = 0; i < diffAmount; i++)
        {
            Difficulties difficulty = m_activeDifficulties[i];
            TextMeshProUGUI textObject = m_difficultyTexts[m_activeDifficulties[i]];

            // Then activate the texts we need
            textObject.gameObject.SetActive(true);

            // And position them correctly
            Vector3 position = new Vector3();
            float normalY = CenterAlignment(i, (diffAmount - 1));

            position.y = normalY * m_difficultyTextSpacing;

            textObject.rectTransform.anchoredPosition3D = position;
            textObject.rectTransform.localScale = new Vector3(1, 1, 1);
            textObject.rectTransform.localEulerAngles = new Vector3(0, 0, 0);
        }

        AudioManager.Instance.PlaySound(Sounds.SsFocusEnter);

        UpdateDiffPointers();

        #region garbage
        /* we no do dis 4 asec
        foreach(Difficulties diff in m_songDifficulties)
        {
            NoteData nData;
            int notes = 0;
            int holds = 0;

            // Obtain the notedata for the current difficulty
            nData = m_currentItem.SongData.notedata[diff];

            int bars = nData.bars.Count;
            
            // Read all the notes from the data
            for (int b = 0; b < bars; b++) // b stands for all the bars in the song
            {
                int rows = nData.bars[b].notes.GetLength(0);

                for (int r = 0; r < rows; r++) // r stands for the row in the curent bar
                {
                    float barPosition;

                    barPosition = ((1.0f / rows) * r) + b;

                    for (int n = 0; n < 4; n++) // n stands for the note in the current row
                    {
                        int note = nData.bars[b].notes[r, n]; // The type of the current note

                        if (note != 0) // Check if the current note isn't nothing
                        {
                            switch ((NoteTypes)note) // Check which type note is and instantiate a prefab
                            {
                                case NoteTypes.Tap:
                                    notes++;
                                    break;

                                case NoteTypes.HoldHeader:
                                    holds++;
                                    break;
                            }
                        }
                    }
                }

                m_noteAmounts[diff] = notes;
                m_holdAmounts[diff] = holds;
            }
        }
        */
        #endregion
    }

    /// <summary>
    /// Handles all the updates for when selecting a diff and such
    /// </summary>
    private void FocusedUpdate()
    {
        if (m_state != SongSelectState.focus)
        {
            NonFocusUpdate();
            return;
        }

        switch (m_focusState)
        {
            case FocusState.Diffselect:
                DiffSelectUpdate();
                break;

            case FocusState.ToFinalize:
                ToFinalizeUpdate();
                break;

            case FocusState.Finalize:
                FinalizeUpdate();
                break;
        }

        m_stateTimers[m_focusState] += Time.deltaTime;
    }
    private void NonFocusUpdate()
    {
        foreach (Difficulties diff in (Difficulties[])Enum.GetValues(typeof(Difficulties)))
        {
            m_difficultyTexts[diff].gameObject.SetActive(false);
        }

        foreach (RectTransform line in m_lines)
        {
            line.sizeDelta = Vector2.zero;
        }

        foreach (RectTransform arrow in m_arrows)
        {
            arrow.localScale = Vector2.zero;
        }

        m_highscoreDisplay.State = ActiveState.inactive;

        m_startTextParent.localScale = Vector3.zero;
        m_accuracyText.rectTransform.localScale = Vector3.zero;
        m_comboText.rectTransform.localScale = Vector3.zero;
        m_timeText.rectTransform.localScale = Vector3.zero;
        m_difficultyBackground.sizeDelta = Vector2.zero;

        m_infoTimer = 0.0f;
    }
    private void DiffSelectUpdate()
    {
        int diffAmount = m_activeDifficulties.Count;

        for (int i = 0; i < m_activeDifficulties.Count; i++)
        {
            Difficulties difficulty = m_activeDifficulties[i];
            TextMeshProUGUI textObject = m_difficultyTexts[m_activeDifficulties[i]];

            // Give them the correct color
            Color c = new Color(0.2f, 0.2f, 0.2f);
            if (difficulty == m_selectedDifficulty)
            {
                c = DifficultyColor(difficulty);

                float scale = Mathf.Clamp(Mathf.Sin(Mathf.Deg2Rad * (m_songSelect.BeatTimer + 1) * 0.5f * 360), -1, 0) * 0.1f * Modifications.Instance.MenuAnimations + 1.0f;
                textObject.rectTransform.localScale = new Vector3(scale, scale, 1);

            }
            else
            {
                textObject.rectTransform.localScale = new Vector3(1, 1, 1);
            }


            textObject.color = Color.Lerp(textObject.color, c, 20 * Time.deltaTime);
        }

        // Position the lines
        //* we add 0.5 to diffamount because then the line is perfectly spaced after the texts
        float borderDistanceDest = (diffAmount + 0.5f) / 2.0f * m_difficultyTextSpacing;
        float lineWidth = Easing.easeInOutExpo(ClampedStateTimer(FocusState.Diffselect, 0.6f), 0, 420, 0.6f);
        float borderDistance = Easing.easeOutBack(ClampedStateTimer(FocusState.Diffselect, 0.5f, 0.3f), 0, borderDistanceDest, 0.5f);

        Vector3 line1Pos = new Vector3(0, -borderDistance);
        Vector3 line2Pos = new Vector3(0, borderDistance);

        m_lines[0].anchoredPosition3D = line1Pos;
        m_lines[1].anchoredPosition3D = line2Pos;

        m_lines[0].sizeDelta = new Vector2(lineWidth, m_lineThickness);
        m_lines[1].sizeDelta = new Vector2(lineWidth, m_lineThickness);

        m_lines[0].localEulerAngles = new Vector3(0, 0, 0);
        m_lines[1].localEulerAngles = new Vector3(0, 0, 0);

        m_lines[2].localScale = Vector3.zero;

        m_difficultyBackground.sizeDelta = new Vector2(m_rect.sizeDelta.x, borderDistance * 2);

        m_difficultyParent.sizeDelta = new Vector2(0, borderDistance*2);

        // Position the pointers
        m_difficultyPointerTimer += (Time.deltaTime * 4.88f);
        m_difficultyPointerTimer = Mathf.Clamp(m_difficultyPointerTimer, 0.0f, 0.4f);

        float yChange = m_difficultyPointerYEnd - m_difficultyPointerYStart;
        float selectorY = Easing.easeOutQuad(m_difficultyPointerTimer, m_difficultyPointerYStart, yChange, 0.4f);

        float selectedWidth = m_difficultyTexts[m_selectedDifficulty].rectTransform.sizeDelta.x / 2;
        float beatOffset = Mathf.Clamp01(Mathf.Sin(Mathf.Deg2Rad * (m_songSelect.BeatTimer + 1) * 0.5f * 360)) * 48 * Modifications.Instance.MenuAnimations;
        float selectorOffset = 4;

        //m_difficultyPointerWidth = Mathf.Lerp(m_difficultyPointerWidth, selectedWidth, 18 * Time.deltaTime) + beatOffset;
        float selectorX = selectedWidth + beatOffset - (m_arrows[0].sizeDelta.x / 2.0f) + selectorOffset;


        Vector3 arrow1Pos = new Vector3(-(selectorX), selectorY);
        Vector3 arrow2Pos = new Vector3(selectorX, selectorY);

        m_arrows[0].anchoredPosition3D = arrow1Pos;
        m_arrows[1].anchoredPosition3D = arrow2Pos;

        m_arrows[0].localScale = Vector3.one;
        m_arrows[1].localScale = Vector3.one;

        m_highscoreDisplay.State = ActiveState.inactive;
    }
    private void ToFinalizeUpdate()
    {
        // The duration of the animation
        float length = 1.90f;
        float timer = m_stateTimers[FocusState.ToFinalize];

        if (m_stateTimers[FocusState.ToFinalize] >= length)
        {
            m_focusState = FocusState.Finalize;
            return;
        }

        // Disable all the diff texts besides the selected one
        foreach (Difficulties diff in (Difficulties[])Enum.GetValues(typeof(Difficulties)))
        {
            m_difficultyTexts[diff].gameObject.SetActive(m_selectedDifficulty == diff);
        }
        // Arrows invisible
        foreach (RectTransform arrow in m_arrows)
        {
            arrow.localScale = Vector2.zero;
        }
        // Un-constrain the mask
        m_difficultyParent.sizeDelta = m_rect.sizeDelta;

        float diffPosOld = CenterAlignment(m_activeDifficulties.IndexOf(m_selectedDifficulty), (m_activeDifficulties.Count - 1)) * m_difficultyTextSpacing;
        Vector2 diffPos = Vector2.zero;
        Vector3 diffAngle = Vector3.zero;
        Vector3 diffScale = Vector3.one;

        // Fase 1, selected dif to center, hide the lines
        diffPos.y = Easing.easeInOutQuint(ClampedStateTimer(FocusState.ToFinalize, 0.5f), diffPosOld, -diffPosOld, 0.5f);

        float lineWidth = Easing.easeInOutQuint(ClampedStateTimer(FocusState.ToFinalize, 0.5f), 420, -420, 0.5f);
        float bgWidth = Easing.easeInOutQuint(ClampedStateTimer(FocusState.ToFinalize, 0.5f), m_rect.sizeDelta.x, -m_rect.sizeDelta.x, 0.5f);

        m_lines[0].sizeDelta = new Vector2(lineWidth, m_lineThickness);
        m_lines[1].sizeDelta = new Vector2(lineWidth, m_lineThickness);

        m_difficultyBackground.sizeDelta = new Vector2(bgWidth, m_difficultyBackground.sizeDelta.y);


        // Fase 2, rotate scale, and move dif to the left
        float diffXLeft = -(m_rect.sizeDelta.x / 4);
        if (timer >= 0.4f)
        {
            diffAngle.z = Easing.easeOutBack(ClampedStateTimer(FocusState.ToFinalize, 0.3f, 0.5f), 0, 90.0f, 0.3f);
            diffScale.x = Easing.easeOutExpo(ClampedStateTimer(FocusState.ToFinalize, 0.3f, 0.5f), 1, 0.2f, 0.3f);
            diffScale.y = diffScale.x;

            diffPos.x = Easing.easeInOutQuart(ClampedStateTimer(FocusState.ToFinalize, 0.7f, 1.0f), 0, diffXLeft - (m_difficultyTextSpacing * diffScale.x), 0.7f);
        }

        // Fase 3, line next to diff appears
        if (timer >= 1.5f)
        {
            float lineHeightDest = m_difficultyTexts[m_selectedDifficulty].rectTransform.sizeDelta.x * diffScale.x;
            float lineSizeNormal = Easing.easeInOutExpo(ClampedStateTimer(FocusState.ToFinalize, 0.4f, 1.5f), 0, 1, 0.4f);

            float infoCenterX = ((m_rect.sizeDelta.x - m_infoWidth) / 2.0f);

            m_lines[0].anchoredPosition = new Vector2(diffXLeft, 0);
            m_lines[0].sizeDelta = new Vector2(lineSizeNormal * lineHeightDest, m_lineThickness);
            m_lines[0].localEulerAngles = new Vector3(0, 0, diffAngle.z);

            m_lines[2].localScale = Vector3.one;
            m_lines[2].sizeDelta = new Vector2(lineSizeNormal * m_infoWidth, m_lineThickness);
            m_lines[2].anchoredPosition = new Vector2(infoCenterX, 0);
        }

        // Apply values
        m_difficultyTexts[m_selectedDifficulty].rectTransform.anchoredPosition = diffPos;
        m_difficultyTexts[m_selectedDifficulty].rectTransform.localEulerAngles = diffAngle;
        m_difficultyTexts[m_selectedDifficulty].rectTransform.localScale = diffScale;
    }
    private void FinalizeUpdate()
    {
        // First, set all the diff texts inactive
        foreach (Difficulties diff in (Difficulties[])Enum.GetValues(typeof(Difficulties)))
        {
            m_difficultyTexts[diff].gameObject.SetActive(false);
        }

        // And the arrows invisible
        foreach (RectTransform arrow in m_arrows)
        {
            arrow.localScale = Vector2.zero;
        }

        // Un-constrain the mask
        m_difficultyParent.sizeDelta = m_rect.sizeDelta;

        // Set the selected difficulty's text
        m_difficultyTexts[m_selectedDifficulty].gameObject.SetActive(true);

        Vector2 dPos = new Vector2(-(m_rect.sizeDelta.x / 4), 0);
        float dScale = 1.2f;
        float dAngle = 90.0f;

        m_difficultyTexts[m_selectedDifficulty].rectTransform.anchoredPosition = new Vector2(dPos.x - (m_difficultyTextSpacing * dScale), dPos.y);
        m_difficultyTexts[m_selectedDifficulty].rectTransform.localScale = new Vector3(dScale, dScale, 1);
        m_difficultyTexts[m_selectedDifficulty].rectTransform.localEulerAngles = new Vector3(0, 0, dAngle);

        // The line besides the difficulty
        float lineHeight = m_difficultyTexts[m_selectedDifficulty].rectTransform.sizeDelta.x * dScale;

        m_lines[0].anchoredPosition = dPos;
        m_lines[0].sizeDelta = new Vector2(lineHeight, m_lineThickness);
        m_lines[0].localEulerAngles = new Vector3(0, 0, dAngle);


        if (m_highscoreDisplay.ContainsScores)
        {
            // The "scrollBar" line
            int highscoreAmount = m_highscoreDisplay.HighScores.Count - 1;
            float normalY = (m_highscoreDisplay.SelectedIndex - (highscoreAmount / 2.0f)) / (highscoreAmount / 2.0f) * -1;
            float selectorHeight = Mathf.Clamp(lineHeight / highscoreAmount, m_lineThickness * 3, lineHeight);
            float scrollbarY = (normalY * ((lineHeight / 2) - (selectorHeight / 2)));

            m_highscoreScrollbarY = Mathf.Lerp(m_highscoreScrollbarY, scrollbarY, 10 * Time.deltaTime);

            m_lines[1].anchoredPosition = new Vector2(dPos.x + m_lineThickness, dPos.y + m_highscoreScrollbarY);
            m_lines[1].sizeDelta = new Vector2(selectorHeight, m_lineThickness);
            m_lines[1].localEulerAngles = new Vector3(0, 0, dAngle);

            m_accuracyText.text = "acc: " + m_highscoreDisplay.SelectedHighscore.Accuracy.ToString("0.00") + "%";
            m_comboText.text = "combo: " + m_highscoreDisplay.SelectedHighscore.Combo.ToString();
            m_timeText.text = m_highscoreDisplay.SelectedHighscore.Time.ToString("dd/MM/yyyy HH:mm:ss");
        }
        else
        {
            m_lines[1].sizeDelta = Vector2.zero;

            m_accuracyText.text = string.Empty;
            m_comboText.text = string.Empty;
            m_timeText.text = string.Empty;
        }


        // Configure the highscore table
        m_highscoreDisplay.State = ActiveState.active;
        m_highscoreDisplay.Position = dPos;

        // The extra highscore info on the right
        float infoCenterX = ((m_rect.sizeDelta.x - m_infoWidth) / 2.0f);
        float infoY = Easing.easeOutExpo(Mathf.Clamp01(m_infoTimer), -300, 300 + m_infoY, 1.0f);
        m_infoTimer += Time.deltaTime;

        m_lines[2].localScale = Vector3.one;
        m_lines[2].sizeDelta = new Vector2(m_infoWidth, m_lineThickness);
        m_lines[2].anchoredPosition = new Vector2(infoCenterX, 0);

        m_accuracyText.rectTransform.localScale = Vector3.one;
        m_comboText.rectTransform.localScale = Vector3.one;
        m_timeText.rectTransform.localScale = Vector3.one;

        m_accuracyText.rectTransform.anchoredPosition = new Vector2(infoCenterX - (m_infoWidth / 2) + 10, infoY);
        m_comboText.rectTransform.anchoredPosition = new Vector2(infoCenterX + (m_infoWidth / 2) - 20, infoY);
        m_timeText.rectTransform.anchoredPosition = new Vector2(infoCenterX, infoY + 50);

        // The start text
        float startY = Easing.easeOutBounce(ClampedStateTimer(FocusState.Finalize, 0.7f), 100, -100 - m_infoY, 0.7f);

        m_startTextParent.localScale = Vector3.one;
        m_startTextParent.anchoredPosition = new Vector2(infoCenterX, startY);

        float beatTime = m_songSelect.BeatTimer;

        float yOffset = Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * (m_songSelect.BeatTimer + 1) * 0.5f * 360)) * 20f * Modifications.Instance.MenuAnimations;

        m_startText.rectTransform.localPosition = new Vector3(m_startText.rectTransform.localPosition.x, -55 + yOffset, 1);

        m_difficultyBackground.sizeDelta = Vector2.zero;
    }

    private void ToFinalize()
    {
        m_focusState = FocusState.ToFinalize;
        AudioManager.Instance.PlaySound(Sounds.DifficultyEnter);

        // Load the highscores of that song's difficulty
        try
        {
            List<ScoreData> scoreData;
            scoreData = HighscoreManager.Highscores[m_currentItem.SongData.title].Scores[m_selectedDifficulty];

            m_highscoreDisplay.SetNewHighscores(scoreData);
        }
        catch
        {
            m_highscoreDisplay.SetHighscoresEmpty();
        }

        var t = m_difficultyTexts[m_selectedDifficulty];
        m_effectText.text = t.text;
        m_effectText.color = t.color;
        m_effectText.fontSize = t.fontSize;
        m_effectText.rectTransform.position = t.rectTransform.position;
        m_effectTextTimer = 0;
    }

    private float ClampedStateTimer(FocusState state, float duration)
    {
        return Mathf.Clamp(m_stateTimers[state], 0, duration);
    }

    private float ClampedStateTimer(FocusState state, float duration, float offset)
    {
        return Mathf.Clamp(m_stateTimers[state] - offset, 0, duration);
    }
    private float CenterAlignment(int index, int total)
    {
        float output = ((total / 2.0f) - index);
        return output;
    }

    /// <summary>
    /// Create all values in which the rect can be, for animation
    /// </summary>
    private void CreateRectSizes()
    {
        m_previewSize = new RectSize(
            new Vector2(420, 0),
            new Vector3(1000, 800, 1));

        m_focusSize = new RectSize(
            new Vector2(0, 0),
            new Vector3(1920-150, 1080-150, 1));

        m_oldSize = new RectSize(
            m_rect.anchoredPosition,
            m_rect.sizeDelta);

        m_currentSize = new RectSize(
            m_rect.anchoredPosition,
            m_rect.sizeDelta);
    }

    /// <summary>
    /// Set the position and size of the window
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    private void SetRectSize(Vector2 pos, Vector3 size)
    {
        m_rect.anchoredPosition = pos;
        m_rect.sizeDelta = size;
    }

    private void UpdateTitleTexts()
    {
        bool isFocus = (m_state == SongSelectState.focus);

        float yPivot = isFocus ? 1f : 0.0f;
        float titleYdest = isFocus ? 0.0f : -(m_rect.sizeDelta.y / 2);
        float artistOffset = isFocus ? -150.0f : 0.0f;
        //float titleXrot = isFocus ? -150.0f : 0.0f;

        float y = Mathf.Lerp(m_texts[0].rectTransform.anchoredPosition.y, titleYdest, 12 * Time.deltaTime);

        m_texts[0].rectTransform.anchoredPosition = new Vector2(0, y);
        m_texts[1].rectTransform.anchoredPosition = new Vector2(0, y + artistOffset);

        m_texts[0].rectTransform.pivot = new Vector2(0.5f, yPivot);
        m_texts[1].rectTransform.pivot = new Vector2(0.5f, 1f);
    }
    /// <summary>
    /// Set text options according to the current state
    /// </summary>
    private void SetTextSettings()
    {
        switch (m_state)
        {
            case SongSelectState.preview:
                m_texts[0].alignment = TextAlignmentOptions.Center;
                m_texts[1].alignment = TextAlignmentOptions.Center;
                break;

            case SongSelectState.focus:
                m_texts[0].alignment = TextAlignmentOptions.Left;
                m_texts[1].alignment = TextAlignmentOptions.Left;
                break;
        }
    }

    /// <summary>
    /// Animate the window size according to the current state
    /// </summary>
    private void UpdateWindowSize()
    {
        RectSize size = RectSize.zero;
        Vector3 rotation = Vector3.zero;

        // Get the size
        switch (m_state)
        {
            case SongSelectState.preview:
                m_focusState = FocusState.Diffselect;
                size = m_previewSize;
                rotation = new Vector3(10.0f, 0, 0);
                break;

            case SongSelectState.focus:
                size = m_focusSize;
                break;
        }

        // Size is zero so we don't want to animate to it
        if(size.size == Vector3.zero)
        {
            m_currentSize = size;
        }
        else
        {
            if (m_sizeEaseTimer < 1)
            {
                var duration = 0.6f;

                m_sizeEaseTimer += Time.deltaTime;
                m_sizeEaseTimer = Mathf.Clamp01(m_sizeEaseTimer);

                if (Modifications.Instance.MenuAnimations == 1)
                {
                    m_currentSize.size.x = Easing.easeOutElastic(m_sizeEaseTimer * duration, m_oldSize.size.x, (size.size.x - m_oldSize.size.x), duration);
                    m_currentSize.size.y = Easing.easeOutElastic(m_sizeEaseTimer * duration, m_oldSize.size.y, (size.size.y - m_oldSize.size.y), duration);
                }
                // A calmer animation for when animations are disabled
                else
                {
                    m_currentSize.size.x = Easing.easeOutQuart(m_sizeEaseTimer * duration, m_oldSize.size.x, (size.size.x - m_oldSize.size.x), duration);
                    m_currentSize.size.y = Easing.easeOutQuart(m_sizeEaseTimer * duration, m_oldSize.size.y, (size.size.y - m_oldSize.size.y), duration);
                }

                m_currentSize.position.x = Easing.easeOutExpo(m_sizeEaseTimer * duration * 0.7f, m_oldSize.position.x, (size.position.x - m_oldSize.position.x), duration * 0.7f);
                m_currentSize.position.y = Easing.easeOutElastic(m_sizeEaseTimer * duration * 0.7f, m_oldSize.position.y, (size.position.y - m_oldSize.position.y), duration * 0.7f);
            }
        }

        m_currentSize.size.z = 1;

        SetRectSize(m_currentSize.position, m_currentSize.size);
        m_rect.localEulerAngles = rotation;
    }

    /// <summary>
    /// Update the text when in preview mode
    /// </summary>
    /// <param name="menuItem"></param>
    public void UpdatePreviews(SongSelectButton menuItem)
    {
        // Update the current metadata
        m_currentItem = menuItem;

        m_songDifficulties = m_currentItem.SongData.difficultyExists;

        m_title.dest = StringToArray(m_currentItem.SongData.title);
        m_artist.dest = StringToArray(m_currentItem.SongData.artist);
        m_bottomRight.dest = StringToArray("Bpm - " + m_currentItem.SongData.bpms[0].bpm.ToString("000"));

        var clipLength = m_currentItem.AudioClip.length * (1 / (float)Modifications.Instance.MusicPitch * 100);

        // Get the time in minutes and seconds
        var sec = ((int)clipLength % 60).ToString("00");
        var min = ((int)clipLength / 60).ToString("00");
        m_bottomLeft.dest = StringToArray("Length - " + min + " : " + sec);
    }

    /// <summary>
    /// Update the values for the text when in focus mode
    /// </summary>
    private void UpdateFocusText()
    {
        if (m_state != SongSelectState.focus)
            return;

        m_bottomLeft.dest = StringToArray(String.Empty);
        m_bottomRight.dest = StringToArray(String.Empty);
    }

    /// <summary>
    /// Animate SSPTexts to their dest string
    /// </summary>
    /// <param name="ssp"></param>
    private void StringTransition(ref SSPText ssp)
    {
        #region Obtain which action to do
        int action; // 0 = fill, 1 = empty

        if (Enumerable.SequenceEqual(ssp.text.ToArray(), ssp.dest)) // Check and return if they're the same
            return;

        if (ssp.text.Count == 0) // Text is empty, so fill
        {
            action = 0;
        }
        else if (ssp.text.Count > ssp.dest.Length) // Text is larger than dest, which shouldn't be possible, so empty
        {
            action = 1;
        }
        else
        {
            var shortCount = Mathf.Min(ssp.text.Count, ssp.dest.Length); // Get the length of the shortest list/array
            var noDifference = true;

            for (int i = 0; i < shortCount; i++)
            {
                if (ssp.text[i] != ssp.dest[i])
                {
                    noDifference = false;
                    break;
                }
            }

            if (noDifference)
            {
                action = 0;
            }
            else
            {
                action = 1;
            }

        }
        #endregion

        switch (action)
        {
            case 0: // Fill

                ssp.text.Add(ssp.dest[ssp.text.Count]);

                break;

            case 1: // Empty

                ssp.text.RemoveAt(ssp.text.Count - 1);

                break;
        }
    }
    private SSPText StringTransition(SSPText ssp)
    {
        #region Obtain which action to do
        int action; // 0 = fill, 1 = empty

        if (Enumerable.SequenceEqual(ssp.text.ToArray(), ssp.dest)) // Check and return if they're the same
            return ssp;

        if (ssp.text.Count == 0) // Text is empty, so fill
        {
            action = 0;
        }
        else if (ssp.text.Count > ssp.dest.Length) // Text is larger than dest, which shouldn't be possible, so empty
        {
            action = 1;
        }
        else
        {
            var shortCount = Mathf.Min(ssp.text.Count, ssp.dest.Length); // Get the length of the shortest list/array
            var noDifference = true;

            for (int i = 0; i < shortCount; i++)
            {
                if (ssp.text[i] != ssp.dest[i])
                {
                    noDifference = false;
                    break;
                }
            }

            if (noDifference)
            {
                action = 0;
            }
            else
            {
                action = 1;
            }

        }
        #endregion

        switch (action)
        {
            case 0: // Fill

                ssp.text.Add(ssp.dest[ssp.text.Count]);

                break;

            case 1: // Empty

                ssp.text.RemoveAt(ssp.text.Count - 1);

                break;
        }

        return ssp;
    }

    /// <summary>
    /// Convert a string to an array where every index is one single character of the original string
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private string[] StringToArray(string text)
    {
        if(text == null)
        {
            Debug.LogError("The string is null");
            return new string[0];
        }

        string[] array = new string[text.Length];

        for (int i = 0; i < text.Length; i++)
        {
            array[i] = text[i].ToString();
        }

        return array;
    }

    /// <summary>
    /// Initalize animated text's lists
    /// </summary>
    /// <param name="ssp"></param>
    private void InitalizeSSP(ref SSPText ssp)
    {
        ssp.text = new List<string>();
        ssp.dest = new string[0];
    }
    private void InitalizeSSP<TKey>(ref Dictionary<TKey, SSPText> dictionary, TKey diff)
    {
        SSPText ssp;
        ssp.text = new List<string>();
        ssp.dest = new string[0];

        dictionary[diff] = ssp;
    }

    /// <summary>
    /// Convert a string list to a single string
    /// </summary>
    /// <param name="stringList"></param>
    /// <returns></returns>
    private string ListToString(List<string> stringList)
    {
        return string.Join("", stringList.ToArray());
    }

    /// <summary>
    /// Add tags for TMPro
    /// </summary>
    /// <param name="text"></param>
    /// <param name="hex"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    private string AddTextTags(string text, string hex, float size)
    {
        return "<#"+hex.ToUpper() + ">" +
            "<size=" + size.ToString() + "%>" +
            text +
            "</color></size>";
    }

    /// <summary>
    /// Add a linebreak or not
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    private string Linebreak(bool condition)
    {
        if (condition)
            return "\n";
        else
            return "";
    }

    /// <summary>
    /// Change the current state
    /// </summary>
    /// <param name="state"></param>
    public void SongSelectStateUpdate(SongSelectState state)
    {
        m_state = state;
        m_sizeEaseTimer = 0;

        m_oldSize = new RectSize(
            m_rect.anchoredPosition,
            m_rect.sizeDelta);

        if (state == SongSelectState.preview)
        {
            UpdatePreviews(m_currentItem);

            m_noteAmount.dest = StringToArray("");
            m_holdAmount.dest = StringToArray("");
        }

        if (state == SongSelectState.focus)
            EnterFocusState();

        if(state == SongSelectState.loadingGame)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    private bool MoveDiffSelection(int amount)
    {
        int diffIndex = m_activeDifficulties.IndexOf(m_selectedDifficulty);
        int newIndex = mod(diffIndex + amount, m_activeDifficulties.Count);

        // Index has stayed the same, there's been no movement
        if (diffIndex == newIndex)
            return false;

        m_selectedDifficulty = m_activeDifficulties[newIndex];

        m_difficultyPointerYStart = m_arrows[0].anchoredPosition.y;
        m_difficultyPointerYEnd = m_difficultyTexts[m_selectedDifficulty].rectTransform.anchoredPosition.y;
        m_difficultyPointerTimer = 0;

        return true;
    }

    /// <summary>
    /// For when variables for selected difficulty and such have changed, makes sure that pointers have the correct position
    /// </summary>
    private void UpdateDiffPointers()
    {
        float newY = m_difficultyTexts[m_selectedDifficulty].rectTransform.anchoredPosition.y;

        m_difficultyPointerYStart = newY;
        m_difficultyPointerYEnd = newY;
        m_difficultyPointerTimer = Int32.MaxValue; // We don't want an animation for this
    }

    /// <summary>
    /// When key up is pressed
    /// </summary>
    private void KeyUp()
    {
        if (m_state != SongSelectState.focus)
            return;

        switch (m_focusState)
        {
            case FocusState.Diffselect:
                {
                    if (MoveDiffSelection(-1))
                    {
                        AudioManager.Instance.PlaySound(Sounds.DifficultySelect); 
                    }
                }
                break;

            case FocusState.Finalize:
                {
                    if(m_highscoreDisplay.MoveSelection(-1))
                    {
                        m_infoTimer = 0.0f;
                    }
                }
                break;
        }
    }
    /// <summary>
    /// When key down is pressed
    /// </summary>
    private void KeyDown()
    {
        if (m_state != SongSelectState.focus)
            return;

        switch (m_focusState)
        {
            case FocusState.Diffselect:
                {
                    if (MoveDiffSelection(1))
                    {
                        AudioManager.Instance.PlaySound(Sounds.DifficultySelect);
                    }
                }
                break;

            case FocusState.Finalize:
                {
                    if (m_highscoreDisplay.MoveSelection(1))
                    {
                        m_infoTimer = 0.0f;
                    }
                }
                break;
        }
    }
    /// <summary>
    /// When key start is pressed
    /// </summary>
    private void KeyStart()
    {
        /*
        if(m_state == SongSelectState.focus)
        {     
            m_songSelect.StartGame(
                m_currentItem.SongData,
                m_selectedDifficulty, 
                m_currentItem.Backdrop, 
                m_currentItem.AudioClip);
        }
        */
        
        if (m_state == SongSelectState.focus)
        {
            switch (m_focusState)
            {
                case FocusState.Diffselect:
                    {
                        ToFinalize();
                    }
                    break;

                case FocusState.Finalize:
                    {
                        m_songSelect.StartGame(
                            m_currentItem.SongData,
                            m_selectedDifficulty,
                            m_currentItem.Backdrop,
                            m_currentItem.AudioClip);
                    }
                    break;
            }
        }
    }

    public RectTransform RectTransform
    {
        get { return m_rect; }
    }

    /// <summary>
    /// mod that handles negative numbers as wel
    /// </summary>
    /// <param name="x"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    int mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    Color DifficultyColor(Difficulties d)
    {
        Color32 c = Color.magenta;
        switch (d)
        {
            case Difficulties.beginner:
                // Light blue
                c = new Color32(153, 217, 234, 255);
                break;

            case Difficulties.easy:
                // Grass green
                c = new Color32(181, 230, 29, 255);
                break;

            case Difficulties.medium:
                // Bright yellow
                c = new Color32(255, 242, 0, 255);
                break;

            case Difficulties.hard:
                // Bright red
                c = new Color32(237, 28, 36, 255);
                break;

            case Difficulties.challenge:
                // Lavender purple
                c = new Color32(163, 73, 164, 255);
                break;

            case Difficulties.insane:
                // Dark purple
                c = new Color32(91, 0, 91, 255);
                break;
        }

        return c;
    }
}
