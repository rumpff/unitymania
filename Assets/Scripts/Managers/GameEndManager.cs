using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SimpleEasing;
using UnityEngine.SceneManagement;

public class GameEndManager : MonoBehaviour
{
    private GameManager m_gameManager;

    [SerializeField] private RectTransform m_gameUI; // Gameobject that contains all ui elements for in-game
    [SerializeField] private RectTransform m_resultsUI; // Gameobject that contains all ui elements for post-game
    [Space(10)]

    [SerializeField] private List<ParticleSystem> m_particleEmitters;
    [Space(10)]

    [SerializeField] private RectTransform m_fullMask;
    [SerializeField] private RectTransform m_whitePlane;
    [Space(10)]

    [SerializeField] private RectTransform m_seperators;
    [SerializeField] private RectTransform m_judgements;
    [SerializeField] private RectTransform m_topMask;
    [SerializeField] private RectTransform m_bottomMask;
    [Space(10)]

    [SerializeField] private RectTransform m_songTitle;
    [SerializeField] private RectTransform m_rating;
    [SerializeField] private RectTransform m_dateTime;
    [Space(10)]

    [SerializeField] private AudioPeer m_audioPeer;
    [SerializeField] private AudioSource m_musicSource;
    [SerializeField] private AudioClip m_introClip;
    [SerializeField] private AudioClip m_loopClip;
    [Space(10)]

    [SerializeField] private TextMeshProUGUI m_songTitleText;
    [SerializeField] private TextMeshProUGUI m_songDifficultyText;
    [SerializeField] private TextMeshProUGUI m_dateTimeText;
    [Space(4)]
    [SerializeField] private TextMeshProUGUI m_scoreText;
    [SerializeField] private TextMeshProUGUI m_accuracyText;
    [SerializeField] private TextMeshProUGUI m_comboText;
    [SerializeField] private TextMeshProUGUI m_ratingText;
    [Space(4)]
    [SerializeField] private TextMeshProUGUI m_marvelousText;
    [SerializeField] private TextMeshProUGUI m_perfectText;
    [SerializeField] private TextMeshProUGUI m_greatText;
    [SerializeField] private TextMeshProUGUI m_goodText;
    [SerializeField] private TextMeshProUGUI m_badText;
    [SerializeField] private TextMeshProUGUI m_missText;

    private float m_particleEmitRate = 0;
    private bool m_animationEnded = false;

    private bool m_gameEnded = false; // If the last note is hit or not
    private bool m_fullCombo = false; // If the song is cleared with a full combo


    private bool m_statsAnimationActive = true;
    private float m_statsAnimTimer = 0;
    private float m_appearenceLength = 2.9f;

    private bool m_disappear = false;
    private float m_disappearTimer = 0;
    private float m_disappearLength = 1.3f;

    private float m_shakeMagnitude;

    // For loading the song selection
    private string m_levelName = "songSelectScene";
    private bool m_loadingSongSelect = false;

    void Start()
    {
        m_gameManager = GameManager.Instance;
        //m_gameManager.GameplayManager.JudgementEvent += CheckIfGameEnded;

        m_gameUI.gameObject.SetActive(true);
        m_resultsUI.gameObject.SetActive(false);

        InputHandler.Instance.KeyStartEvent += EnterKey;
        InputHandler.Instance.KeyExitEvent += EnterKey;

        // Deactivate the white plane because if we set the widht to 0 it will become invisible
        m_whitePlane.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!m_gameEnded)
        {
            if(!CheckIfGameEnded())
                return;
        }

        AnimateAppearence();
        InfiniteAnimation();
        AnimateDisappearance();
    }

    private void OnDestroy()
    {
        InputHandler.Instance.KeyStartEvent -= EnterKey;
        InputHandler.Instance.KeyExitEvent -= EnterKey;
    }

    /// <summary>
    /// Each frame we check if the last 4 notes are hit
    /// If they are, then we enable the gameEnding sequence
    /// -could use optimization-
    /// </summary>
    bool CheckIfGameEnded()
    {
        var noteList = m_gameManager.Notes;

        // Check if the last 4 notes are disabled
        for (int i = 0; i < 4; i++)
        {
            Note[] noteArray = m_gameManager.Notes[(Directions)i];
            if (noteArray[noteArray.Length - 1].IsActive)
                return false;
        }

        // We looped through the list but we didn't return so there's no note left
        InitiateGameEnd();

        return true;
    }

    private void InitiateGameEnd()
    {
        m_gameEnded = true;

        m_gameUI.gameObject.SetActive(false);
        m_resultsUI.gameObject.SetActive(true);
        m_gameManager.ScoreManager.SaveScores();
        
        m_musicSource.clip = m_introClip;
        m_musicSource.loop = false;
        m_musicSource.Play();

        foreach (KeyValuePair<Directions, Receptor> receptor in m_gameManager.Receptors)
        {
            receptor.Value.gameObject.SetActive(false);
        }

        SetTexts();
    }

    public void EnterKey()
    {
        if (!m_gameEnded)
            return;

        if (!m_animationEnded)
        {
            ActivateExplosion();
            return;
        }

        if(!m_disappear)
        {
            m_disappear = true;
        }
    }

    private void SetTexts()
    {
        var sm = m_gameManager.ScoreManager;

        m_songTitleText.text = SongData.Song.title;
        m_songDifficultyText.text = ("-" + SongData.Difficulty.ToString() + "-");
        m_scoreText.text += sm.Score.ToString("0");
        m_accuracyText.text += (sm.Accuracy.ToString("00.00") + "%");
        m_comboText.text += sm.ComboHigh.ToString("000");
        m_ratingText.text = ScoreManager.CalculateRating(sm.Accuracy).ToString();
        m_marvelousText.text += sm.JudgementHits[Judgements.marvelous].ToString();
        m_perfectText.text += sm.JudgementHits[Judgements.perfect].ToString();
        m_greatText.text += sm.JudgementHits[Judgements.great].ToString();
        m_goodText.text += sm.JudgementHits[Judgements.good].ToString();
        m_badText.text += sm.JudgementHits[Judgements.bad].ToString();
        m_missText.text += sm.JudgementHits[Judgements.miss].ToString();

        m_dateTimeText.text = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

    private void AnimateAppearence()
    {
        if (!m_statsAnimationActive)
            return;

        float seperatorWidth, judgementHeight, songTitleHeight, dateTimeHeight, masksY, ratingScale, ratingAngle, musicVolume;

        // Calculate the values based on the same timer
        songTitleHeight = Easing.easeOutElastic(Mathf.Clamp(m_statsAnimTimer, 0f, 0.6f), 260f, -300f, 0.6f);
        dateTimeHeight = Easing.easeOutElastic(Mathf.Clamp(m_statsAnimTimer, 0f, 0.6f), -70f, 75f, 0.6f);
        seperatorWidth = Easing.easeInOutCubic(Mathf.Clamp(m_statsAnimTimer- 1.0f, 0f, 0.6f), 0f, 930, 0.6f);
        masksY = Easing.easeOutQuint(Mathf.Clamp(m_statsAnimTimer - 1.7f, 0, 0.3f), -80, 80, 0.3f);
        judgementHeight = Easing.easeOutElastic(Mathf.Clamp(m_statsAnimTimer - 2f, 0f, 0.8f), 0f, 232, 0.8f);

        ratingScale = Easing.easeInQuint(Mathf.Clamp(m_statsAnimTimer - 2.6f, 0f, 0.3f), 0f, 1f, 0.3f);
        ratingAngle = Easing.easeInQuint(Mathf.Clamp(m_statsAnimTimer - 2.6f, 0f, 0.3f), -10f, 30f, 0.3f);

        musicVolume = Easing.easeInQuad(m_statsAnimTimer, 1f, -0.7f, m_appearenceLength);

        // Apply the values
        m_seperators.sizeDelta = new Vector2(seperatorWidth, 0);

        m_topMask.anchoredPosition = new Vector2(0, masksY);
        m_bottomMask.anchoredPosition = new Vector2(0, -masksY);

        m_judgements.sizeDelta = new Vector2(m_judgements.rect.width, judgementHeight);

        m_rating.transform.localScale = new Vector3(ratingScale, ratingScale, ratingScale);
        m_rating.transform.localEulerAngles = new Vector3(0, 0, ratingAngle);

        m_songTitle.anchoredPosition = new Vector2(m_songTitle.anchoredPosition.x, songTitleHeight);
        m_dateTime.anchoredPosition = new Vector2(m_dateTime.anchoredPosition.x, dateTimeHeight);

        m_gameManager.ChartManager.AudioSource.volume = musicVolume;

        // Time the timer
        m_statsAnimTimer += Time.deltaTime;

        // Activate particles
        if (m_statsAnimTimer > m_appearenceLength && !m_animationEnded)
        {
            ActivateExplosion();
        }

        // Shake the screen
        m_resultsUI.anchoredPosition = new Vector2(Random.Range(-m_shakeMagnitude, m_shakeMagnitude), Random.Range(-m_shakeMagnitude, m_shakeMagnitude));
        m_shakeMagnitude = Mathf.Lerp(m_shakeMagnitude, 0, 6 * Time.deltaTime);
    }

    private void AnimateDisappearance()
    {
        if (!m_disappear)
            return;

        m_whitePlane.gameObject.SetActive(true);

        float maskWidth, whiteWidth;

        // Calculate values
        whiteWidth = Easing.easeInQuint(Mathf.Clamp(m_disappearTimer, 0, 0.5f), 1920, -1900, 0.5f);
        maskWidth = Easing.easeInOutQuint(Mathf.Clamp(m_disappearTimer - 0.3f, 0, 1.0f), 1920, -1920, 1.0f);

        // Apply Values
        m_whitePlane.sizeDelta = new Vector2(whiteWidth, m_whitePlane.rect.height);
        m_fullMask.sizeDelta = new Vector2(maskWidth, m_fullMask.rect.height);

        m_disappearTimer += Time.deltaTime;

        // Set the backdrop to un-dimmed
        m_gameManager.BackDrop.color = Color.white;
        m_gameManager.BackDrop.transform.eulerAngles = Vector3.zero;

        // Set the pitch of the background music with the width of the mask
        m_musicSource.pitch = maskWidth / 1920;

        // Return to the songSelect
        if (m_disappearTimer > m_disappearLength && !m_loadingSongSelect)
        {
            m_gameManager.LoadSongSelect();
            m_loadingSongSelect = true;
        }

    }

    void ActivateExplosion()
    {
        foreach (ParticleSystem ps in m_particleEmitters)
        {
            ps.Emit(800);
            m_shakeMagnitude = 50;

            m_musicSource.clip = m_loopClip;
            m_musicSource.loop = true;
            m_musicSource.Play();

            m_gameManager.ChartManager.PauseMusic();
        }

        m_statsAnimTimer = m_appearenceLength;
        m_animationEnded = true;
    }

    // Animation for when the animation is finished
    void InfiniteAnimation()
    {
        if (!m_animationEnded)
            return;

        float beatCounter, ratingScale, ratingAngle;

        // Update the beat timer
        beatCounter = m_musicSource.time * (140.0f / 60.0f);

        float beatRad = beatCounter * Mathf.Deg2Rad;

        //ratingScale = Mathf.Abs(Mathf.Sin(beatRad * 0.5f * 360)) * 0.5f + 1;
        ratingScale = m_audioPeer._audioBandBuffer.Max();
        ratingAngle = Mathf.Sin(beatRad * 0.25f * 360) * 5 + 20;

        

        m_rating.transform.localScale = new Vector3(ratingScale, ratingScale, ratingScale);
        m_rating.transform.localEulerAngles = new Vector3(0, 0, ratingAngle);
    }

    public bool GameEnded
    {
        get { return m_gameEnded; }
    }
}
