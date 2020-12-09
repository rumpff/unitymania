using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleEasing;

public class PauseManager : MonoBehaviour
{
    private enum PauseOptions
    {
        Continue,
        Restart,
        GiveUp
    };


    private Dictionary<PauseOptions, Sprite> m_pointerSprites;
    private Dictionary<PauseOptions, RectTransform> m_optionTexts;

    [SerializeField] private GameObject m_pauseUI;
    [SerializeField] private GameObject m_gameUI;

    private GameManager m_gameManager;

    private RectTransform m_seperator;
    private RectTransform m_pointer;
    private RectTransform m_selectionEffect;

    private Image m_pointerImage;
    private Image m_darkPlane;

    private RectTransform m_pauseText;
    private RectTransform m_textGroup;

    private bool m_pauseActive;

    // For when the player has choosen an option
    private bool m_optionPicked = false;
    private bool m_optionExecuted = false;

    // Values for the appear animation
    private float m_appearTimer = 0f;
    private float m_appearLength = 1f;
    private float m_seperatorEndWidth = 1440f;
    private float m_textGroupOffset = 50f;

    // Values for the pause options
    private float m_optionInactiveX = 50f;
    private float m_optionActiveX = 100f;

    // Values for the pointer
    private float m_pointerXOrigin;
    private float m_pointerInactiveY = 310f;
    private Vector2 m_pointerScale = Vector2.one;
    private float m_pointerScaleTimer = 0;
    private float m_pointerScaleDuration = 0.5f;
    private float m_pointerBumpDistance = 1920;

    // Values for the selection effect
    private float m_selectionEffectTimer = 0f;
    private float m_selectionEffectDuration = 0.2f;

    private PauseOptions m_currentSelected;

    private void Start()
    {
        m_gameManager = GameManager.Instance;

        // Retrieve all necessary instances
        Transform[] transforms = m_pauseUI.GetComponentsInChildren<Transform>();

        m_seperator = FindObjectByNameInArray(transforms, "Seperator").GetComponent<RectTransform>();
        m_selectionEffect = FindObjectByNameInArray(transforms, "SelectionEffect").GetComponent<RectTransform>();
        m_pointer = FindObjectByNameInArray(transforms, "Pointer").GetComponent<RectTransform>();
        m_pointerImage = m_pointer.GetComponent<Image>();
        m_pointerXOrigin = m_pointer.position.x;

        m_darkPlane = FindObjectByNameInArray(transforms, "DarkPlane").GetComponent<Image>();

        m_pauseText = FindObjectByNameInArray(transforms, "PausedTitle").GetComponent<RectTransform>();
        m_textGroup = FindObjectByNameInArray(transforms, "TextsGroup").GetComponent<RectTransform>();

        m_optionTexts = new Dictionary<PauseOptions, RectTransform>
        {
            { PauseOptions.Continue, FindObjectByNameInArray(transforms, "ContinueText").GetComponent<RectTransform>() },
            { PauseOptions.Restart, FindObjectByNameInArray(transforms, "RestartText").GetComponent<RectTransform>() },
            { PauseOptions.GiveUp, FindObjectByNameInArray(transforms, "GiveUpText").GetComponent<RectTransform>() }
        };

        // Load the pointer sprites
        m_pointerSprites = new Dictionary<PauseOptions, Sprite>
        {
            { PauseOptions.Continue, Resources.Load<Sprite>("Sprites/PauseScreen/Continue") },
            { PauseOptions.Restart, Resources.Load<Sprite>("Sprites/PauseScreen/Restart") },
            { PauseOptions.GiveUp, Resources.Load<Sprite>("Sprites/PauseScreen/GiveUp") }
        };

        InputHandler.Instance.KeyStartEvent += SelectOption;
        InputHandler.Instance.KeyRestartEvent += InstantRestart;
        InputHandler.Instance.KeyExitEvent += TogglePause;
        InputHandler.Instance.KeyUpEvent += MoveSelectionUp;
        InputHandler.Instance.KeyDownEvent += MoveSelectionDown;

        m_pauseActive = false;
        DeactivatePause();
    }

    private void OnDestroy()
    {
        InputHandler.Instance.KeyStartEvent -= SelectOption;
        InputHandler.Instance.KeyRestartEvent -= InstantRestart;
        InputHandler.Instance.KeyExitEvent -= TogglePause;
        InputHandler.Instance.KeyUpEvent -= MoveSelectionUp;
        InputHandler.Instance.KeyDownEvent -= MoveSelectionDown;
    }

    private void Update()
    {
        // When the game is paused
        if(m_pauseActive)
        {
            #region (dis)Appear Animation

            // Values that are going to be animated
            float seperatorWidth, pauseTextY, textGroupY, darkPlaneAlpha;
            float pauseTextHeight = m_pauseText.rect.height;
            float textGroupHeight = m_textGroup.rect.height;
            Color darkplaneColor = m_darkPlane.color;

            darkPlaneAlpha = Easing.easeInOutQuint(Mathf.Clamp(m_appearTimer, 0, m_appearLength * 0.3f), 0, 0.5f, m_appearLength * 0.3f);
            seperatorWidth = Easing.easeInOutQuint(Mathf.Clamp(m_appearTimer, 0, m_appearLength * 0.6f), 0, m_seperatorEndWidth, m_appearLength * 0.6f);
            // These animations start at 50% of the timer
            pauseTextY = Easing.easeOutBack(Mathf.Clamp(m_appearTimer - (m_appearLength * 0.45f), 0, m_appearLength * 0.45f), -pauseTextHeight, pauseTextHeight, m_appearLength * 0.5f);
            textGroupY = Easing.easeOutBack(Mathf.Clamp(m_appearTimer - (m_appearLength * 0.5f), 0, m_appearLength * 0.5f), textGroupHeight, -(textGroupHeight + m_textGroupOffset), m_appearLength * 0.5f);

            // Apply the values
            darkplaneColor.a = darkPlaneAlpha;
            m_darkPlane.color = darkplaneColor;
            m_seperator.sizeDelta = new Vector2(seperatorWidth, 1);
            m_pauseText.anchoredPosition = new Vector2(0, pauseTextY);
            m_textGroup.anchoredPosition = new Vector2(0, textGroupY);

            // Time the timer and make sure it doesn't overshoot
            if (!m_optionPicked)
            {
                if (m_appearTimer < m_appearLength)
                    m_appearTimer += Time.deltaTime;
                if (m_appearTimer > m_appearLength)
                    m_appearTimer = m_appearLength;
            }
            else // Reverse the animation
            {
                if (m_appearTimer > 0)
                    m_appearTimer -= Time.deltaTime;
                if (m_appearTimer < 0)
                    m_appearTimer = 0;
            }

            #endregion
            #region Pointer stuff
            Vector2 pointerpos = m_pointer.position;

            // Calculate the position
            float pointerX = m_optionTexts[m_currentSelected].position.x - 50;
            float pointerYDest = m_optionTexts[m_currentSelected].position.y;

            // Lerp from current position
            pointerpos.x = Mathf.Lerp(pointerpos.x, m_pointerXOrigin, 13 * Time.deltaTime); 
            pointerpos.y = Mathf.Lerp(pointerpos.y, pointerYDest, 13 * Time.deltaTime);

            // Ease the scale
            m_pointerScale.x = Easing.easeOutElastic(m_pointerScaleTimer, 1.33f, -0.33f, m_pointerScaleDuration);
            m_pointerScale.y = Easing.easeOutElastic(m_pointerScaleTimer, 0.77f, 0.33f, m_pointerScaleDuration);            

            // Apply modified values
            m_pointer.position = pointerpos;
            m_pointer.localScale = m_pointerScale;

            // Change the sprite
            m_pointerImage.sprite = m_pointerSprites[m_currentSelected];

            // Time the timer
            if (m_pointerScaleTimer < m_pointerScaleDuration)
                m_pointerScaleTimer += Time.deltaTime;
            if (m_pointerScaleTimer > m_pointerScaleDuration)
                m_pointerScaleTimer = m_pointerScaleDuration;
            #endregion
            #region Position the options

            foreach (KeyValuePair<PauseOptions, RectTransform> entry in m_optionTexts)
            {
                float xCurrent = entry.Value.anchoredPosition.x;

                float xDest = m_optionInactiveX;
                if (entry.Key == m_currentSelected)
                    xDest = m_optionActiveX;

                xCurrent = Mathf.Lerp(xCurrent, xDest, 16 * Time.deltaTime);
                entry.Value.anchoredPosition = new Vector2(xCurrent, entry.Value.anchoredPosition.y);
            }

            #endregion
            #region Handle when an option is picked

            if (m_optionPicked && m_appearTimer <= 0)
            {
                DeactivatePause();

                if (!m_optionExecuted)
                {
                    switch (m_currentSelected)
                    {
                        case PauseOptions.Restart:
                            m_gameManager.RestartGame();
                            break;

                        case PauseOptions.GiveUp:
                            m_gameManager.LoadSongSelect();
                            break;
                    }

                    m_optionExecuted = true;
                }
            }

            #endregion
            #region Selection Effect
            // Set the parent
            m_selectionEffect.transform.parent = m_optionTexts[m_currentSelected].transform;

            // Animate the width
            float leftWidth, rightWidth, textWidth, singleLength;

            singleLength = m_selectionEffectDuration * 0.5f;
            textWidth = m_optionTexts[m_currentSelected].rect.width;

            rightWidth = Easing.easeLiniear(Mathf.Clamp(m_selectionEffectTimer, 0, singleLength), textWidth, -textWidth, singleLength);
            leftWidth = Easing.easeLiniear(Mathf.Clamp(m_selectionEffectTimer - singleLength, 0, singleLength), 0, textWidth, singleLength);

            // Apply the values
            m_selectionEffect.offsetMin = new Vector2(leftWidth, 0f);
            m_selectionEffect.offsetMax = new Vector2(-rightWidth, 0f);

            // Time the timer
            if (m_selectionEffectTimer < m_selectionEffectDuration && m_optionPicked)
                m_selectionEffectTimer += Time.deltaTime;

            if (m_selectionEffectTimer > m_selectionEffectDuration)
                m_selectionEffectTimer = m_selectionEffectDuration;
            #endregion
        }
        else
        {

        }
    }

    public void TogglePause()
    {
        if (!m_pauseActive)
        {
            if (CanPause())
            {
                ActivatePause();
            }
        }
        else
        {
            m_currentSelected = PauseOptions.Continue;
            SelectOption();
        }
    }

    public void ActivatePause()
    {
        // Pause the music
        m_gameManager.ChartManager.PauseMusic();

        // Disable the game UI
        m_gameUI.SetActive(false);

        // Enable the pause UI
        m_pauseUI.SetActive(true);

        // Disable the game-input
        m_gameManager.GameplayManager.InputDisabled = true;

        // Reset the timer(s)
        m_appearTimer = 0f;
        m_selectionEffectTimer = 0f;

        // Reset the selection
        m_currentSelected = PauseOptions.Continue;

        // Reset the position of the pointer
        m_pointer.anchoredPosition = new Vector2(m_pointer.anchoredPosition.x, m_pointerInactiveY);

        // Set PauseActive to true
        m_pauseActive = true;

        // Reset any previouses
        m_optionPicked = false;
        m_optionExecuted = false;
}
    public void DeactivatePause()
    {
        // Resume the music
        m_gameManager.ChartManager.ResumeMusic();

        // Enable the game UI
        m_gameUI.SetActive(true);

        // Disable the pause UI
        m_pauseUI.SetActive(false);

        // Enable the game-input
        m_gameManager.GameplayManager.InputDisabled = false;

        // Set PauseActive to false
        m_pauseActive = false;
    }
    public void MoveSelectionUp()
    {
        if (m_optionPicked)
            return;
        
        var enumMemberCount = PauseOptions.GetNames(typeof(PauseOptions)).Length;

        int selected = ((int)m_currentSelected - 1 < 0) ? enumMemberCount-1 : (int)m_currentSelected - 1;

        m_currentSelected = (PauseOptions) selected;

        m_pointerScaleTimer = 0;
        AudioManager.Instance.PlaySound(Sounds.PausedSelect);
    }
    public void MoveSelectionDown()
    {
        if (m_optionPicked)
            return;

        var enumMemberCount = PauseOptions.GetNames(typeof(PauseOptions)).Length;
        m_currentSelected = (PauseOptions)((int)(m_currentSelected + 1) % enumMemberCount);

        m_pointerScaleTimer = 0;
        AudioManager.Instance.PlaySound(Sounds.PausedSelect);
    }
    public void SelectOption()
    {
        if (!m_pauseActive || m_optionPicked)
            return;

        m_pointer.position = new Vector2(m_pointerXOrigin + (m_pointerBumpDistance / 1920 * 50), m_pointer.position.y);
        m_optionPicked = true;

        switch (m_currentSelected)
        {
            case PauseOptions.Continue:
                AudioManager.Instance.PlaySound(Sounds.PausedContinue);
                break;
            case PauseOptions.Restart:
                AudioManager.Instance.PlaySound(Sounds.PausedRestart);
                break;
            case PauseOptions.GiveUp:
                AudioManager.Instance.PlaySound(Sounds.PausedGiveUp);
                break;
        }
    }

    private Transform FindObjectByNameInArray(Transform[] array, string name)
    {
        Transform output = null;
        foreach (Transform t in array)
        {
            if (t.name == name)
            {
                output = t;
                break;
            }
        }

        return output;
    }

    public void InstantRestart()
    {
        if (!m_pauseActive)
        {
            if (CanPause())
            {
                AudioManager.Instance.PlaySound(Sounds.PausedRestart);
                m_gameManager.RestartGame();
            }
        }
        else
        {
            m_currentSelected = PauseOptions.Restart;
            SelectOption();
        }
    }

    private bool CanPause()
    {
        return (m_gameManager.ChartManager.AudioSource.isPlaying && !m_gameManager.GameEndManager.GameEnded);
    }
}
