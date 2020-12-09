using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleEasing;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RectTransform m_gameUI;
    [SerializeField] private GameObject m_pauseUI;
    [Space(10)]

    [SerializeField] private TextMeshProUGUI m_songTitle;
    [SerializeField] private TextMeshProUGUI m_judgementTextComp;
    [SerializeField] private TextMeshProUGUI m_comboTextComp;
    [SerializeField] private TextMeshProUGUI m_comboBreakTextComp;
    [SerializeField] private TextMeshProUGUI m_accuracyTextComp;

    [SerializeField] private RectTransform m_progress;
    [SerializeField] private Image m_progressPie;

    private GameplayManager m_gameplayManager;
    private ScoreManager m_scoreManager;
    private ChartManager m_chartManager;
    private GameCamera m_gameCamera;


    private TMP_ColorGradient m_judgementGradient;
    private Dictionary<Judgements, TMP_ColorGradient> m_judgementGradients;

    private string m_judgementText = "Let's go!";
    private float m_judgementTimer = 1.0f;
    private float m_judgementInitiateScale = 0;

    private float m_comboScale = 1.0f;
    private float m_displayAccuracy = 100.0f;

    private float m_comboBreakTimer = 1.0f;

    private float m_songLength = 0f;
    private float m_songPos = 0f;

    private void Awake()
    {
        // Add all the gradient presets for the judgements
        m_judgementGradients = new Dictionary<Judgements, TMP_ColorGradient>()
        {
            { Judgements.marvelous, (TMP_ColorGradient)Resources.Load("GradientPresets/Judgements/MarvelousGradient") },
            { Judgements.perfect, (TMP_ColorGradient)Resources.Load("GradientPresets/Judgements/PerfectGradient") },
            { Judgements.great, (TMP_ColorGradient)Resources.Load("GradientPresets/Judgements/GreatGradient") },
            { Judgements.good, (TMP_ColorGradient)Resources.Load("GradientPresets/Judgements/GoodGradient") },
            { Judgements.bad, (TMP_ColorGradient)Resources.Load("GradientPresets/Judgements/BadGradient") },
            { Judgements.miss, (TMP_ColorGradient)Resources.Load("GradientPresets/Judgements/MissGradient") }
        };
    }

    private void Start ()
    {
        m_gameplayManager = GameManager.Instance.GameplayManager;
        m_scoreManager = GameManager.Instance.ScoreManager;
        m_chartManager = GameManager.Instance.ChartManager;
        m_gameCamera = GameManager.Instance.GameCameraComp;

        string[] startTexts = new string[]
        {
            "Get Ready!",
            "Let's Jam!",
            "3, 2, 1",
            "Get your hands warmed up!",
            "Let's do this!",
            "Are you ready?"
        };

        m_songTitle.text = startTexts[UnityEngine.Random.Range(0, startTexts.Length - 1)];

        // Set the ui layering
        m_gameUI.GetComponentInParent<Canvas>().sortingOrder = Modifications.Instance.UILayer * 5;

        // Move the progress to the bottom if ddr style is active
        if (Modifications.Instance.ScrollDirection == ScrollDirction.up)
            m_progress.anchoredPosition = new Vector3(m_progress.anchoredPosition.x, m_progress.anchoredPosition.y * -1);

        // Subscribe to events
        m_gameplayManager.JudgementEvent += Judgement;
        m_scoreManager.ComboAddEvent += ComboEffect;
        m_scoreManager.ComboBreakEvent += ComboBreakEffect;
    }
    private void OnDestroy()
    {
        m_gameplayManager.JudgementEvent -= Judgement;
        m_scoreManager.ComboAddEvent -= ComboEffect;
        m_scoreManager.ComboBreakEvent -= ComboBreakEffect;
    }
    private void Update()
    {
        m_songLength = GameManager.Instance.ChartManager.AudioSource.clip.length;
        m_songPos = GameManager.Instance.ChartManager.SongTime;

        m_progressPie.fillAmount = (m_songPos / m_songLength);

        // Update start text
        float scale = 1;
        float angle = 0;
        Color color = m_songTitle.color;
        float startTextTimer = m_chartManager.SongTime + m_chartManager.CountdownTime;

        if (m_chartManager.CountdownStarted)
        {

            if (startTextTimer < 1)
            {
                scale = Easing.easeOutElastic(startTextTimer, 0.0f, 1.0f, 1.0f);
            }
            else if (startTextTimer > 1.5f)
            {
                scale = Easing.easeOutExpo(startTextTimer - 1.5f, 1.0f, 3.0f, 0.7f);
                angle = Easing.easeOutExpo(startTextTimer - 1.5f, 0.0f, -30.0f, 0.7f);
                color.a = Easing.easeOutExpo(startTextTimer - 1.5f, 1.0f, -1.0f, 0.7f);

                m_gameCamera.Idle = false;
            }
            if (startTextTimer > 2)
            {
                m_songTitle.rectTransform.localScale = Vector2.zero;
            }
        }
        else
        {
            scale = 0.0f;
        }

        m_songTitle.rectTransform.localScale = new Vector3(scale, scale, 1);
        m_songTitle.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
        m_songTitle.color = color;

        // Update judgement text
        float judgementScale;

        if (m_judgementTimer <= 0.2f)
        {
            judgementScale = Easing.easeOutBack(m_judgementTimer, m_judgementInitiateScale, 1 - m_judgementInitiateScale, 0.2f);
        }
        else if (m_judgementTimer > 0.2f && m_judgementTimer < 0.6f)
        {
            judgementScale = Easing.easeInQuint(m_judgementTimer - 0.2f, 1.0f, -1.0f, 0.4f);
        }
        else
        {
            judgementScale = 0;
        }

        m_judgementTextComp.text = m_judgementText;
        m_judgementTextComp.rectTransform.localScale = new Vector3(judgementScale, judgementScale, 1.0f);
        m_judgementTextComp.colorGradientPreset = m_judgementGradient;

        m_judgementTimer += Time.deltaTime;

        // Update combo text
        m_comboScale = Mathf.Lerp(m_comboScale, 1, 15 * Time.deltaTime);
        m_comboTextComp.text = m_scoreManager.Combo.ToString();
        m_comboTextComp.transform.localScale = new Vector3(1, m_comboScale, 1);

        // Update combobreak text
        var breakOrigin = m_comboTextComp.rectTransform.anchoredPosition.y;
        var breakColor = m_comboBreakTextComp.color;
        var breakAlpha = Easing.easeInExpo(m_comboBreakTimer, 1f, -1f, 1f);
        var breakY = Easing.easeOutElastic(m_comboBreakTimer, breakOrigin, -0.54f, 1);

        breakColor.a = breakAlpha;

        m_comboBreakTextComp.rectTransform.anchoredPosition = new Vector2(m_comboBreakTextComp.rectTransform.anchoredPosition.x, breakY);
        m_comboBreakTextComp.color = breakColor;

        m_comboBreakTimer += Time.deltaTime;

        // Update accuracy text
        m_displayAccuracy = Mathf.Lerp(m_displayAccuracy, m_scoreManager.Accuracy, 17 * Time.deltaTime);
        m_accuracyTextComp.text = m_displayAccuracy.ToString("0.00") + "%";

        // Position the world canvas on the receptors
        float receptorCenterX = 0;
        float receptorCenterZ = 0;

        foreach (Directions dir in (Directions[])Enum.GetValues(typeof(Directions)))
        {
            receptorCenterX += GameManager.Instance.Receptors[dir].transform.position.x;
            receptorCenterZ += GameManager.Instance.Receptors[dir].transform.position.z;
        }

        receptorCenterX /= Enum.GetNames(typeof(Directions)).Length;
        receptorCenterZ /= Enum.GetNames(typeof(Directions)).Length;


        m_gameUI.position = new Vector3(receptorCenterX, m_gameUI.position.y, receptorCenterZ);
    }

    // Methods for jugement events
    private void Judgement(Judgements j)
    {
        m_judgementTimer = 0;
        m_judgementText = j.ToString();
        m_judgementGradient = m_judgementGradients[j];
        m_judgementInitiateScale = Mathf.Clamp(m_judgementTextComp.rectTransform.localScale.x, 0.4f, 0.8f);
    }

    private void ComboEffect()
    {
        m_comboScale = 1.4f;
    }
    private void ComboBreakEffect()
    {
        if (m_scoreManager.Combo < 25)
            return;

        m_comboBreakTextComp.text = m_scoreManager.Combo.ToString();
        m_comboBreakTimer = 0f;
    }

    public GameObject PauseUI
    {
        get { return m_pauseUI; }
    }
}
