using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

using SimpleEasing;
using TMPro;
using Coffee.UIExtensions;

public class SongSelect : MonoBehaviour
{
    // If a button has to teleport or not
    private enum ButtonTeleport
    {
        no,
        behindZero,
        afterMax
    }

    [SerializeField] public static List<SpritePack> SpritePacks;
    [SerializeField] private SongSelectState m_state;

    [SerializeField] private AudioClip m_menuTick, m_songStart, m_songSelectSound;
    [SerializeField] private TextMeshProUGUI m_loadingText, m_titleText;
    [SerializeField] private RectTransform m_loadingBar;

    [SerializeField] private Canvas m_worldCanvas;
    [SerializeField] private RectTransform m_screenCanvas;
    [SerializeField] private GameObject m_camera;
    [Space(5)]

    [SerializeField] Camera m_worldCamera;
    [SerializeField] Camera m_uiCamera;
    [Space(5)]

    [SerializeField] private Image m_songPreview;
    [SerializeField] private AspectRatioFitter m_songPreviewAspect;
    [SerializeField] private RectTransform m_songPreviewMask;
    [Space(5)]

    [SerializeField] private Material m_songPreviewMat;
    [SerializeField] private UIEffect m_songPreviewEffects;
    [Space(5)]
    private float m_songPreviewGlitchIntensity;

    [SerializeField] private SongSelectWindow m_songWindow;
    [SerializeField] private ModifierWindow m_modWindow;
    [SerializeField] private AudioPeer m_musicVisualizer;
    [Space(5)]

    private InputHandler m_inputHandler;   
    private RectTransform m_rectTransform;
    private AudioSource m_audioSource;
    private CoroutineQueue m_songloadQueue;
    private List<IEnumerator> m_loadQueueQueue;
    private int m_totalLoadingItems;

    [SerializeField] private Transform m_songButtonParent;
    [SerializeField] private TextMeshPro m_clock;
    [Space(5)]

    private List<SongSelectButton> m_songButtonList;
    private SongSelectButton m_currentSelected;
    private static int m_lastSelectedId;

    private static Dictionary<string, MenuItemContent> m_menuItemContents;
    private List<string> m_keyList;
    private bool m_songLoadingFinished;
    private bool m_songsExist;

    [SerializeField] private int m_songButtonAmount;
    [Space(5)]

    [SerializeField] private float m_minScrollSpeed = 0.6f;
    [SerializeField] private float m_maxScrollSpeed = 0.1f;
    [Space(5)]

    [SerializeField] private float m_buttonSpacing = 180;
    [SerializeField] private float m_extraButtonSpacing = 30;
    [Space(5)]

    [SerializeField] private AudioMixer m_musicMixer;
    private float m_distortion = 0;
    private float m_lowPass = 22000f;

    private ButtonTeleport[] m_teleportIndex;

    private float m_buttonOffset;
    private float m_scrollSpeed, m_scrollTimer;

    private float m_pitchInTimer;
    private float m_pitchOutTimer = 1;
    private float m_selectedTimer; // How long the current song is selected
    private float m_backdropTimer;
    private float m_backdropIntensity = 0;

    private float m_backdropWobbleTimer = 1;
    private float m_backdropWobbleLength = 1;

    private float m_loadCheckBuffer = 0;

    private float m_loadGameTimer;

    private float m_bassIntensity;

    private float m_preTimer = 0;
    public bool m_started = false;

    private bool m_toFocus = false;
    private float m_toFocusLength = 0.2f;

    private float m_introTimer = 0.0f;
    private float m_introLength = 3.0f;

    private string m_currentLoading = "Loading Song libary";
    private List<string> m_songsLoading;
    private float m_loadingBarSize;

    private AsyncOperation m_gameLoadOperation;
    private Texture2D m_backdropTransitionTexture;

    // This variable exists for the game to know the average color
    public static Color AverageColor;

    private void Awake()
    {
        m_scrollTimer = 0;
        m_audioSource = GetComponent<AudioSource>();

        m_songsLoading = new List<string>();

        if (m_menuItemContents == null)
            m_menuItemContents = new Dictionary<string, MenuItemContent>();

        if (m_lastSelectedId == null)
            m_lastSelectedId = 0;

        m_keyList = new List<string>();

        m_songLoadingFinished = false;
    }

    private void Start()
    {
        // Create song buttons
        GameObject buttonPrefab = Resources.Load("Prefabs/Menu/SongSelect/MenuItem") as GameObject;

        for (int i = 0; i < m_songButtonAmount; i++)
        {
            Instantiate(buttonPrefab, m_songButtonParent);
        }

        // Add all the buttons to a list
        m_songButtonList = m_songButtonParent.GetComponentsInChildren<SongSelectButton>().ToList();

        Initalize();
    }

    private void OnDestroy()
    {
        m_songLoadingFinished = false;
    }

    private void Initalize()
    {
        PreLoadSongs();
        PreLoadNoteSkins();

        m_rectTransform = transform as RectTransform;
        m_teleportIndex = new ButtonTeleport[m_songButtonList.Count];

        for (int i = 0; i < m_teleportIndex.Length; i++)
        {
            m_teleportIndex[i] = ButtonTeleport.no;
        }

        SongData songData = new SongData();
            

        m_buttonOffset = m_songButtonList[0].RectTransform.sizeDelta.y;
        m_currentSelected = m_songButtonList[0];

        ChangeState(SongSelectState.loadingSongs);
        m_state = SongSelectState.intro;

        m_inputHandler = InputHandler.Instance;

        // Subscribe to the keyinput events
        m_inputHandler.KeyUpHeldEvent += KeyHoldUp;
        m_inputHandler.KeyDownHeldEvent += KeyHoldDown;
        m_inputHandler.NoKeyVerticalEvent += NoKeyVertical;
        m_inputHandler.KeyStartEvent += OnKeyStart;
        m_inputHandler.KeyExitEvent += OnKeyExit;

        
        m_started = true;
    }

    private void OnDisable()
    {
        // UnSubscribe to the keyinput events
        m_inputHandler.KeyUpHeldEvent -= KeyHoldUp;
        m_inputHandler.KeyDownHeldEvent -= KeyHoldDown;
        m_inputHandler.NoKeyVerticalEvent -= NoKeyVertical;
        m_inputHandler.KeyStartEvent -= OnKeyStart;
        m_inputHandler.KeyExitEvent -= OnKeyExit;
    }

    private void Update()
    {
        IntroUpdate();
        LoadingStateUpdate();
        AdditiveLoadUpdate();

        if (m_started)
        {
            LoadGame();
            UpdatePitchTimers();
            UpdateSelectionTime();
            UpdateBassIntensity();
            UpdateBackdropTransition();
            UpdateBackdropMask();
            UpdateBackdrop();
            UpdateBackdropEffects();
            UpdateMusicEffects();
            AverageColor = GetMenuItemContent(CurrentSelected.ContentKey).averageColor;
        }

        UpdateCameras();
        UpdateClock();        
    }

    private void UpdateCameras()
    {
        Vector3 posDest = Vector3.zero;
        Vector3 rotDest = Vector3.zero;

        if(m_state == SongSelectState.preview)
        {
            posDest.x = 300f;
            rotDest.y = -15f;
        }

        if (m_state == SongSelectState.inModifiers)
        {
            posDest.y = 500f;
            posDest.z = 1000f;
        }

        var rotate = Quaternion.Euler(rotDest);

        m_camera.transform.position = Vector3.Lerp(m_camera.transform.position, posDest, 8 * Time.deltaTime);
        m_camera.transform.rotation = Quaternion.Lerp(m_camera.transform.rotation, rotate, 8 * Time.deltaTime);

        bool isStateWithoutBg = (m_state == SongSelectState.loadingSongs || m_state == SongSelectState.intro);
        m_worldCamera.enabled = (Modifications.Instance.SongselectBackground == 1 && !isStateWithoutBg);
        m_uiCamera.clearFlags = (CameraClearFlags)(Modifications.Instance.SongselectBackground * 2 + 2 * Convert.ToInt32(!isStateWithoutBg));
    }
    private void UpdatePitchTimers()
    {
        // Don't update the pitch when there's no song playing
        if (m_audioSource.clip == null)
            return;

        // Control pitch enter
        if (m_pitchInTimer < 1 && m_currentSelected.AudioClip == m_audioSource.clip)
            m_pitchInTimer += Time.deltaTime;

        var timerIn = Mathf.Clamp01(m_pitchInTimer) * 0.3f; ;

        m_audioSource.pitch = Easing.easeOutExpo(timerIn, 0, 1, 0.3f);

        // Control pitch exit;
        if (m_pitchOutTimer < 1 && m_currentSelected.AudioClip != m_audioSource.clip)
        {
            m_pitchOutTimer += (Time.deltaTime * 2);

            if (m_pitchOutTimer >= 1)
            {
                m_audioSource.Stop();
            }
            else
            {
                var timerOut = Mathf.Clamp01(m_pitchOutTimer) * 0.3f; ;
                m_audioSource.pitch = Easing.easeInOutSine(timerOut, 1, -1, 0.3f);
            }
        }

        // Also lerp volume to 1
        m_audioSource.volume = Mathf.Lerp(m_audioSource.volume, 1, 4 * Time.deltaTime);
    }
    private void UpdateSelectionTime()
    {
        m_selectedTimer += Time.deltaTime;

        // Do stuff for when the song is selected for a longer than short
        if (m_selectedTimer > 0.5f)
        {
            if (m_currentSelected.AudioClip != null)
            {
                if (!m_audioSource.isPlaying || m_currentSelected.AudioClip != m_audioSource.clip)
                {
                    m_audioSource.clip = m_currentSelected.AudioClip;

                    m_audioSource.time = 0.0f;
                    m_audioSource.Play();
                    m_audioSource.time = m_currentSelected.SongData.sampleStart;

                    m_pitchOutTimer = 1;
                }
            }
        }
    }
    private void UpdateBackdrop()
    {
        // Set the backdrop
        if (m_currentSelected.Backdrop != null && m_songPreview.sprite != m_currentSelected.Backdrop)
        {
            m_songPreview.sprite = m_currentSelected.Backdrop;
            m_songPreviewAspect.aspectRatio = (m_songPreview.sprite.bounds.size.x / m_songPreview.sprite.bounds.size.y);
            m_backdropWobbleTimer = 0;
            m_songPreviewGlitchIntensity = 1;
        }

        float backdropScaleX = Easing.easeOutElastic(m_backdropWobbleTimer, 0.5f, 0.5f, m_backdropWobbleLength);
        float backdropScaleY = Easing.easeOutElastic(m_backdropWobbleTimer, 1.5f, -0.5f, m_backdropWobbleLength);

        if (Modifications.Instance.MenuAnimations == 0)
        {
            backdropScaleX = 1;
            backdropScaleY = 1;
        }

        if(m_backdropWobbleTimer < m_backdropWobbleLength)
        {
            m_backdropWobbleTimer += Time.deltaTime;
            m_backdropWobbleTimer = Mathf.Clamp(m_backdropWobbleTimer, 0, m_backdropWobbleLength);
        }


        float scale = m_songPreview.rectTransform.localScale.x;
        float rotation = m_songPreview.rectTransform.localEulerAngles.z;

        // Get size, rotation based on the state
        switch (m_state)
        {
            // Bounce on the music
            case SongSelectState.preview:

                float intensityDest = 0.3f;

                m_backdropIntensity = Mathf.Lerp(m_backdropIntensity, intensityDest, 10 * Time.deltaTime);
                scale = m_bassIntensity * m_backdropIntensity * Modifications.Instance.MenuAnimations + 1;

                // Revert rotation for when we come back from focus
                rotation = Mathf.LerpAngle(rotation, 0, 14 * Time.deltaTime);
                break;

            // Enlarge when focused
            case SongSelectState.focus:
                scale = Mathf.Lerp(scale, 2.5f + (Mathf.Sin(Time.time * Modifications.Instance.MenuAnimations * 0.47683f) * 0.5f), 6 * Time.deltaTime);
                rotation = Mathf.LerpAngle(rotation, Mathf.Sin(Time.time / 2 * Modifications.Instance.MenuAnimations) * 30, 6 * Time.deltaTime);
                break;

            // When nothing is going on, background in "rest"
            default:
                scale = Mathf.Lerp(scale, 1.0f, 6 * Time.deltaTime);
                rotation = Mathf.LerpAngle(rotation, 0.0f, 6 * Time.deltaTime);
                break;
        }

        // Apply the new values
        m_songPreview.rectTransform.localScale = new Vector2(scale * backdropScaleX, scale * backdropScaleY);
        m_songPreview.rectTransform.localEulerAngles = new Vector3(0, 0, rotation);
    }
    private void UpdateBassIntensity()
    {
        float[] Bands = new float[] {
                m_musicVisualizer._audioBandBuffer[0],
                m_musicVisualizer._audioBandBuffer[1],
                m_musicVisualizer._audioBandBuffer[2],
                m_musicVisualizer._audioBandBuffer[3],
                m_musicVisualizer._audioBandBuffer[4],
                m_musicVisualizer._audioBandBuffer[5]
                };

        m_bassIntensity = Bands.Max();
    }
    private void UpdateBackdropEffects()
    {
        // Set current effects
        float blurdest = 0;

        switch (m_state)
        {
            case SongSelectState.preview:
                m_songPreviewEffects.enabled = false;
                m_songPreview.material = m_songPreviewMat;
                break;

            case SongSelectState.focus:
                m_songPreviewEffects.enabled = true;
                blurdest = 1;
                break;
        }

        // Glitch
        float disp;
        float col;

        switch (m_state)
        {
            default:
                m_songPreviewGlitchIntensity = Mathf.Lerp(m_songPreviewGlitchIntensity, 0.0f, 12 * Time.deltaTime);

                disp = 1;
                col = 1;
                break;
        }

        m_songPreviewMat.SetFloat("_DispIntensity", m_songPreviewGlitchIntensity * disp * Modifications.Instance.MenuAnimations);
        m_songPreviewMat.SetFloat("_ColorIntensity", m_songPreviewGlitchIntensity * col * Modifications.Instance.MenuAnimations);

        // Blur
        m_songPreviewEffects.blurFactor = Mathf.Lerp(m_songPreviewEffects.blurFactor, blurdest, 3 * Time.deltaTime);
    }
    private void UpdateBackdropMask()
    {
        Vector2 position = m_songPreviewMask.anchoredPosition;
        Vector2 size = m_songPreviewMask.sizeDelta;
        Vector3 rotation = m_songWindow.RectTransform.localEulerAngles;

        switch (m_state)
        {
            // Set the backdrop fullscreen
            case SongSelectState.loadingGame:
                position = Vector2.zero;
                size = Vector2.Lerp(size, new Vector2(1920, 1080), 14 * Time.deltaTime);
                break;

            // Give it the size of the ss-windowthing
            default:
                position = m_songWindow.RectTransform.anchoredPosition;
                size = m_songWindow.RectTransform.sizeDelta;
                break;
        }

        // Apply new values
        m_songPreviewMask.anchoredPosition = position;
        m_songPreviewMask.sizeDelta = size;
        m_songPreviewMask.localEulerAngles = rotation;
    }
    private void UpdateBackdropTransition()
    {
        // Animate the backdrop
        if (m_backdropTimer <= 1)
        {
            if (m_backdropTimer >= 0.5f &&
                m_currentSelected.AudioClip == m_audioSource.clip &&
                m_audioSource.isPlaying) // Only continue if the current song is playing
            {

                m_backdropTimer += Time.deltaTime;
            }
            else if (m_backdropTimer < 0.5f && m_currentSelected.AudioClip != m_audioSource.clip)
            {
                m_backdropTimer += Time.deltaTime;
            }
            else
            {
                m_backdropTimer = 0.5f;
            }
        }
    }
    private void LoadingStateUpdate()
    {
        if (m_state != SongSelectState.loadingSongs)
            return;

        // Check if all the songs are loaded
        if (m_songloadQueue.Queue.Count == 0 && m_songLoadingFinished) 
        {
            // Sort the songs
            m_keyList.Sort();

            // initalize buttons that already exist
            for (int i = 0; i < m_songButtonList.Count; i++)
            {
                var obj = m_songButtonList[i];
                var comp = obj.GetComponent<SongSelectButton>();
                comp.Initalize(this, i % m_keyList.Count);
            }

            m_songPreview.rectTransform.localScale = Vector2.one;
            UpdateButtonPositions();
            m_loadingText.gameObject.SetActive(false);
            m_loadingBar.gameObject.SetActive(false);
            ChangeState(SongSelectState.preview);
            MoveButtons(Mathf.FloorToInt(m_songButtonList.Count / 2) - m_lastSelectedId);
        }
        else
        {
            int index = m_keyList.IndexOf(m_songsLoading[m_songsLoading.Count-1]) + 1;
            string loadText = "";

            for (int i = 0; i < m_songsLoading.Count-1; i++)
            {
                float alpha = Mathf.Clamp(99 - ((m_songsLoading.Count - i) * 10), 0, 99);
                loadText += "<alpha=#" + alpha.ToString("00") + ">" + m_songsLoading[i] + "<alpha=#FF>\n";
            }

            loadText += "Loading: " + m_songsLoading[m_songsLoading.Count - 1] + "\n[" + index.ToString() + "/" + m_keyList.Count + "]";

            m_loadingText.text = loadText;
            m_loadingText.gameObject.SetActive(true);

            float barleft = ((float)m_songloadQueue.Queue.Count / (float)m_totalLoadingItems) * (float)m_screenCanvas.sizeDelta.x;
            m_loadingBarSize = Mathf.Lerp(m_loadingBarSize, barleft, 18 * Time.deltaTime);
            

            m_loadingBar.gameObject.SetActive(true);
            m_loadingBar.offsetMax = new Vector2(-m_loadingBarSize, 0.015f);
        }
    }
    private void UpdateMusicEffects()
    {
        switch (m_state)
        {
            case SongSelectState.preview:
                m_distortion = Mathf.Lerp(m_distortion, 0, 12 * Time.deltaTime);
                m_lowPass = Mathf.Lerp(m_lowPass, 22000, 12 * Time.deltaTime);
                break;

            case SongSelectState.focus:
                m_distortion = Mathf.Lerp(m_distortion, 0.7f, 12 * Time.deltaTime);
                m_lowPass = Mathf.Lerp(m_lowPass, 700, 12 * Time.deltaTime);
                break;
        }

        m_musicMixer.SetFloat("musicDistortion", m_distortion);
        m_musicMixer.SetFloat("musicLowPass", m_lowPass);
    }
    private void UpdateClock()
    {
        string time = String.Empty;

        time = DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00");

        m_clock.text = time;
    }
    private void LoadGame()
    {
        if (m_state != SongSelectState.loadingGame)
            return;

        float length = 2.3f;

        m_loadGameTimer += Time.deltaTime;
        m_loadGameTimer = Mathf.Clamp(m_loadGameTimer, 0, length);
        float hsV = Easing.easeOutExpo(m_loadGameTimer, 1, (Modifications.Instance.BackdropBrightness / 100.0f) - 1, length);

        m_audioSource.volume = 0;
        m_songPreview.color = Color.HSVToRGB(0, 0, hsV);

        if(m_loadGameTimer >= length && m_gameLoadOperation == null)
        {
            StartCoroutine(LoadSceneAsync("gameScene"));
        }
    }
    private void KeyHoldUp()
    {
        switch (m_state)
        {
            case SongSelectState.preview:
                MoveSongSelection(1);
                break;
        }

    }
    private void KeyHoldDown()
    {
        switch (m_state)
        {
            case SongSelectState.preview:
                MoveSongSelection(-1);
                break;
        }
    }
    private void NoKeyVertical()
    {
        m_scrollSpeed = m_minScrollSpeed;
        m_scrollTimer = 0;
    }
    IEnumerator InitiateToFocus()
    {
        m_toFocus = true;
        yield return new WaitForSeconds(m_toFocusLength);
        ChangeState(SongSelectState.focus);
        m_audioSource.volume = 0;
        m_lastSelectedId = m_currentSelected.ContentKey;
        HideButtons();

        m_toFocus = false;
    }
    private void OnKeyStart()
    {
        switch (m_state)
        {
            case SongSelectState.preview:
                if (!m_toFocus)
                {
                    AudioManager.Instance.PlaySound(Sounds.SelectSong);
                    m_currentSelected.OnHit();
                    StartCoroutine(InitiateToFocus());
                }
                break;

            case SongSelectState.focus:
                break;
        }
    }
    private void OnKeyExit()
    {
        switch (m_state)
        {
            case SongSelectState.preview:
                ChangeState(SongSelectState.inModifiers);
                HideButtons();
                break;

            case SongSelectState.focus:
                ChangeState(SongSelectState.preview);
                RevealButtons();
                break;
        }
    }
    public void ChangeState(SongSelectState state)
    {
        m_state = state;
        m_songWindow.SongSelectStateUpdate(state);
        m_modWindow.SetActive(state == SongSelectState.inModifiers);
    }
    private void MoveSongSelection(int amount)
    {
        if (m_scrollTimer <= 0)
        {
            if (m_scrollSpeed > m_maxScrollSpeed) { m_scrollSpeed = m_scrollSpeed * 0.6f; }

            MoveButtons(amount);
            m_scrollTimer = m_scrollSpeed;
            AudioManager.Instance.PlaySound(Sounds.MenuTickNew);
        }
        else
        {
            m_scrollTimer -= Time.deltaTime;
        }
    }
    private void MoveButtons(int amount)
    {
        int length = m_songButtonList.Count;
        SongSelectButton[] bArray = new SongSelectButton[length];

        for (int i = 0; i < length; i++)
        {
            int newIndex = i + amount;
            ButtonTeleport teleport = ButtonTeleport.no;

            if (newIndex > length - 1)
            {
                newIndex = (newIndex - (length));
                teleport = ButtonTeleport.afterMax;
            }

            if (newIndex < 0)
            {
                newIndex = (newIndex + (length));
                teleport = ButtonTeleport.behindZero;
            }

            bArray[newIndex] = m_songButtonList[i];
            m_teleportIndex[newIndex] = teleport;
        }

        m_songButtonList = bArray.ToList<SongSelectButton>();

        UpdateButtonPositions();

        m_pitchInTimer = 0;
        if (m_pitchOutTimer >= 1) { m_pitchOutTimer = 0; } // Do this to ensure that pitch out isn't canceled and that the song doesn't keep playing
        if (m_backdropTimer >= 1) { m_backdropTimer = 0; }
        m_selectedTimer = 0;

    }
    public void UpdateButtonPositions()
    {
        for (int i = 0; i < m_songButtonList.Count; i++)
        {
            int middle = Mathf.FloorToInt(m_songButtonList.Count / 2);
            bool currentSelected = (i == middle);
            float extraSpacing = GamemakerSign(i - middle) * m_extraButtonSpacing;
            float scale = 1;
            float spacing;

            // Calculate the scale of the button
            scale = 1;
            if (!currentSelected)
                scale = 0.6f;
            
            spacing = (m_buttonSpacing * scale);

            Vector2 pos = new Vector2(0, -(i - middle) * spacing - extraSpacing);

            
            if (currentSelected)
            {
                m_currentSelected = m_songButtonList[i];

                m_songWindow.UpdatePreviews(m_songButtonList[i]);
            }

            if (m_teleportIndex[i] != ButtonTeleport.no) // Button needs to teleport to new pos
            {
                // Check what the new song for the button is
                int newKey = 0;
                switch (m_teleportIndex[i])
                {
                    case ButtonTeleport.behindZero:
                        newKey = (m_songButtonList[i].ContentKey + (m_songButtonList.Count));

                        while (newKey >= m_keyList.Count)
                        {
                            newKey -= m_keyList.Count;
                        }
                        break;

                    case ButtonTeleport.afterMax:
                        newKey = (m_songButtonList[i].ContentKey - (m_songButtonList.Count));

                        while (newKey < 0)
                        {
                            newKey += m_keyList.Count;
                        }
                        break;
                }

                m_songButtonList[i].SetPosition(pos);
                m_songButtonList[i].UpdateKey(newKey);
            }
            else // Button can lerp to new pos
            { m_songButtonList[i].SetDestination(pos); }

            if (currentSelected && !m_songButtonList[i].IsSelected)
            {
                m_songButtonList[i].OnSelect();
            }

            m_songButtonList[i].IsSelected = currentSelected;
        }
    }
    public void HideButtons()
    {
        for (int i = 0; i < m_songButtonList.Count; i++)
        {
            var middle = Mathf.FloorToInt(m_songButtonList.Count / 2);
            var pos = new Vector2(m_rectTransform.anchoredPosition.x - 1000, 0);

            var rotate = new Vector3(0, 0, -90);

            if (m_teleportIndex[i] != ButtonTeleport.no) // Button needs to teleport to new pos
            { m_songButtonList[i].SetPosition(pos); }
            else // Button can lerp to new pos
            { m_songButtonList[i].SetDestination(pos); }

            m_songButtonList[i].RotateDest(Quaternion.Euler(rotate));
        }
    }
    public void RevealButtons()
    {
        for (int i = 0; i < m_songButtonList.Count; i++)
        {
            int middle = Mathf.FloorToInt(m_songButtonList.Count / 2);
            bool currentSelected = (i == middle);
            float extraSpacing = GamemakerSign(i - middle) * m_extraButtonSpacing;
            float scale = 1;
            float spacing;

            // Calculate the scale of the button
            scale = 1;
            if (!currentSelected)
                scale = 0.6f;

            spacing = (m_buttonSpacing * scale);

            Vector2 pos = new Vector2(0, -(i - middle) * spacing - extraSpacing);

            m_songButtonList[i].SetDestination(pos);

            m_songButtonList[i].ScaleDest = Mathf.Abs(scale);
        }
    }
    private void PreLoadSongs()
    {
        if(!Directory.Exists(Utility.GetRoot() + "/Songs"))
            Directory.CreateDirectory(Utility.GetRoot() + "/Songs");

        List<string> songDirectories = new List<string>();
        songDirectories.Add(Utility.GetRoot() + "/Songs");

        if(File.Exists(songDirectories[0] + "/redirect.txt"))
        {
            songDirectories.AddRange(File.ReadAllLines(songDirectories[0] + "/redirect.txt"));
        }
        
        // Make sure that the directories are correct
        foreach(string song in songDirectories)
        {
            song.Replace('\\', '/');
        }

        List<string> fileList = new List<string>();
        for (int i = 0; i < songDirectories.Count; i++)
        {  
            fileList.AddRange(Directory.GetFiles(songDirectories[i], "*.sm", SearchOption.AllDirectories).ToList());
        }
       
        var buttonParent = m_worldCanvas.transform.Find("MenuButtons").transform;
        m_songloadQueue = new CoroutineQueue(2, StartCoroutine);
        m_loadQueueQueue = new List<IEnumerator>();


        for (int i = 0; i < fileList.Count; i++)
        {
            var metaData = SongParser.Parse(fileList[i]);
            var songTitle = metaData.title;

            m_keyList.Add(songTitle);

            // Load the song data
            if (!m_menuItemContents.ContainsKey(songTitle))
            {
                var content = new MenuItemContent();

                content.metaData = metaData;

                m_menuItemContents.Add(songTitle, content);
                m_loadQueueQueue.Add(LoadImage("file:///" + metaData.backgroundPath, songTitle));
                m_loadQueueQueue.Add(LoadAudioClip("file:///" + metaData.musicPath, songTitle));
            }
        }

        m_songsExist = (fileList.Count != 0) ? true : false;
        m_introTimer = (m_loadQueueQueue.Count == 0 && m_songsExist) ? m_introLength : 0;

    }
    private void PreLoadNoteSkins()
    {
        // Sprites are already loaded
        if (SpritePacks != null)
            return;

        if (!Directory.Exists(Utility.GetRoot() + "/NoteSkins"))
            Directory.CreateDirectory(Utility.GetRoot() + "/NoteSkins");

        SpritePacks = new List<SpritePack>
        { Resources.Load<SpritePack>("Sprite Packs/ExactV2") };

        string[] fileList = Directory.GetFiles(Utility.GetRoot() + "/NoteSkins", "skin_settings.txt", SearchOption.AllDirectories);

        for (int skinSettingIndex = 0; skinSettingIndex < fileList.Length; skinSettingIndex++)
        {
            string[] fileContent = File.ReadAllLines(fileList[skinSettingIndex]);
            SpritePackInfo packInfo = new SpritePackInfo()
            {
                dir = fileList[skinSettingIndex].Replace("skin_settings.txt", ""),
                receptorPivot = new Vector2(),
                tapPivot = new Vector2(),
                holdTopPivot = new Vector2(),
                holdCenterPivot = new Vector2(),
                holdBottomPivot = new Vector2()
            };

            for (int i = 0; i < fileContent.Length; i++)
            {
                string[] line = fileContent[i].Split('/');

                // Try to parse the required values out of the settings file
                switch (line[0].ToLower())
                {
                    case "name":
                        {
                            packInfo.name = line[1];
                        }
                        break;

                    case "filenames":
                        {
                            switch (line[1].ToLower())
                            {
                                case "receptor":
                                    {
                                        packInfo.receptorName = line[2];
                                    }
                                    break;

                                case "tapnote":
                                    {
                                        packInfo.tapnoteName = line[2];
                                    }
                                    break;

                                case "hold":
                                    {
                                        switch (line[2].ToLower())
                                        {
                                            case "active":
                                                {
                                                    switch (line[3].ToLower())
                                                    {
                                                        case "head":
                                                            {
                                                                packInfo.holdTopActiveName = line[4];
                                                            }
                                                            break;

                                                        case "center":
                                                            {
                                                                packInfo.holdCenterActiveName = line[4];
                                                            }
                                                            break;

                                                        case "tail":
                                                            {
                                                                packInfo.holdBottomActiveName = line[4];
                                                            }
                                                            break;
                                                    }
                                                }
                                                break;

                                            case "inactive":
                                                {
                                                    switch (line[3].ToLower())
                                                    {
                                                        case "head":
                                                            {
                                                                packInfo.holdTopInactiveName = line[4];
                                                            }
                                                            break;

                                                        case "center":
                                                            {
                                                                packInfo.holdCenterInactiveName = line[4];
                                                            }
                                                            break;

                                                        case "tail":
                                                            {
                                                                packInfo.holdBottomInactiveName = line[4];
                                                            }
                                                            break;
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;

                    case "pivot":
                        {
                            switch (line[1].ToLower())
                            {
                                case "receptor":
                                    {
                                        switch (line[2].ToLower())
                                        {
                                            case "x":
                                                {
                                                    packInfo.receptorPivot.x = float.Parse(line[3]);
                                                }
                                                break;

                                            case "y":
                                                {
                                                    packInfo.receptorPivot.y = float.Parse(line[3]);
                                                }
                                                break;
                                        }
                                    }
                                    break;

                                case "tapnote":
                                    {
                                        switch (line[2].ToLower())
                                        {
                                            case "x":
                                                {
                                                    packInfo.tapPivot.x = float.Parse(line[3]);
                                                }
                                                break;

                                            case "y":
                                                {
                                                    packInfo.tapPivot.y = float.Parse(line[3]);
                                                }
                                                break;
                                        }
                                    }
                                    break;

                                case "hold":
                                    switch(line[2].ToLower())
                                    {
                                        case "head":
                                            {
                                                switch (line[3].ToLower())
                                                {

                                                    case "x":
                                                        {
                                                            packInfo.holdTopPivot.x = float.Parse(line[4]);
                                                        }
                                                        break;

                                                    case "y":
                                                        {
                                                            packInfo.holdTopPivot.y = float.Parse(line[4]);
                                                        }
                                                        break;
                                                }
                                            }
                                            break;

                                        case "center":
                                            {
                                                switch (line[3].ToLower())
                                                {
                                                    case "x":
                                                        {
                                                            packInfo.holdCenterPivot.x = float.Parse(line[4]);
                                                        }
                                                        break;

                                                    case "y":
                                                        {
                                                            packInfo.holdCenterPivot.y = float.Parse(line[4]);
                                                        }
                                                        break;
                                                }
                                            }
                                            break;

                                        case "tail":
                                            {
                                                switch (line[3].ToLower())
                                                {
                                                    case "x":
                                                        {
                                                            packInfo.holdBottomPivot.x = float.Parse(line[4]);
                                                        }
                                                        break;

                                                    case "y":
                                                        {
                                                            packInfo.holdBottomPivot.y = float.Parse(line[4]);
                                                        }
                                                        break;
                                                }
                                            }
                                            break;
                                    }
                                    break;

                                
                            }
                        }
                        break;
                }
            }

            SpritePacks.Add(new SpritePack());
            m_loadQueueQueue.Add(LoadSpritePack(skinSettingIndex + 1, packInfo));
        }
    }
    private void IntroUpdate()
    {
        if (m_introTimer < m_introLength)
        {
            m_state = SongSelectState.intro;

            float textScale;
            float textAngle;
            string text;

            textScale = Easing.easeOutElastic(Mathf.Clamp(m_introTimer - 0.5f, 0, 0.5f), 3, -2, 0.5f);
            textAngle = Easing.easeInOutExpo(Mathf.Clamp(m_introTimer - 2.0f, 0, 0.25f), 0, 360, 0.25f);

            // Set the text
            if (m_introTimer > (2.0f + (0.25f / 2.0f)))
            {
                text = (m_songsExist) ? "Loading" : "No songs\nfound!\n<size=20%>or something went terribly wrong!";
            }
            else
            {
                text = "Unity Mania";
            }

            m_titleText.text = text;
            m_titleText.rectTransform.localScale = new Vector3(textScale, textScale, 1);
            m_titleText.rectTransform.localEulerAngles = new Vector3(0, 0, textAngle);

            m_introTimer += Time.deltaTime;

            m_loadingBar.gameObject.SetActive(false);
        }
        else if(m_state == SongSelectState.intro)
        {
            // Initiate loading if the intro is finished
            m_totalLoadingItems = m_loadQueueQueue.Count;
            m_loadingBarSize = m_screenCanvas.sizeDelta.x;

            if (m_introTimer > m_introLength && m_songsExist)
            {
                for (int i = 0; i < m_loadQueueQueue.Count; i++)
                {
                    m_songloadQueue.Run(m_loadQueueQueue[i]);
                }

                m_songLoadingFinished = true;
                m_state = SongSelectState.loadingSongs;
            }
        }
        else if(m_state != SongSelectState.loadingSongs)
        {
            // Disable the text when the game isn't loading
            m_titleText.gameObject.SetActive(false);
        }
    }
    private void AdditiveLoadUpdate()
    {
        var scene = GameManager.SceneInstance;

        // No extra scene here so return
        if (!scene.isLoaded)
            return;

        m_songPreviewMask.anchoredPosition = Vector2.zero;
        m_songPreviewMask.sizeDelta = new Vector2(1920, 1080);

        // We're done loading so we revert the modifications and unload the old scene
        //if (m_audioSource.isPlaying)
        {
            m_camera.SetActive(true);
            SceneManager.UnloadScene(scene);

            m_introTimer = m_introLength;
            m_songLoadingFinished = true;
            m_state = SongSelectState.loadingSongs;
            return;
        }

        m_camera.SetActive(false);
    }
    public void StartGame(Metadata songData, Difficulties difficulty, Sprite backdrop, AudioClip music)
    {
        SongData.Song = songData;
        SongData.Difficulty = difficulty;
        SongData.Backdrop = backdrop;
        SongData.AudioClip = music;

        m_loadGameTimer = 0;
        AudioManager.Instance.PlaySound(Sounds.SongStart);
        ChangeState(SongSelectState.loadingGame);
    }
    public SongSelectState State
    {
        get { return m_state; }
    }
    public MenuItemContent GetMenuItemContent(int id)
    {
        return m_menuItemContents[m_keyList[id]];
    }
    public SongSelectButton CurrentSelected
    {
        get { return m_currentSelected; }
    }
    public Dictionary<string, MenuItemContent> MenuItemContents
    {
        get { return m_menuItemContents; }
    }
    public AudioSource AudioSource
    {
        get { return m_audioSource; }
    }

    public bool ToFocus
    {
        get { return m_toFocus; }
    }
    public float ToFocusLength
    {
        get { return m_toFocusLength; }
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        m_gameLoadOperation = SceneManager.LoadSceneAsync(sceneName);

        // Wait until the asynchronous scene fully loads
        while (!m_gameLoadOperation.isDone)
        {
            yield return null;
        }
    }

    IEnumerator LoadAudioClip(string path, string listKey)
    {
        // Set the loading text
        m_songsLoading.Add(listKey);
        AudioType audioType = AudioType.WAV;

        if (path.ToLower().EndsWith("ogg"))
            audioType = AudioType.OGGVORBIS;
            

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
        {
            yield return www.SendWebRequest();
            Debug.Log(path);

            var content = m_menuItemContents[listKey];

            content.musicClip = DownloadHandlerAudioClip.GetContent(www);
            content.musicClip.name = content.metaData.title;

            // Because there's a small possibility that the value(s) that we sotred are overwritten and we might revert them
            // I set the other value(s) to the current ones
            content.backdrop = m_menuItemContents[listKey].backdrop;


            m_menuItemContents[listKey] = content;
        }
    }

    IEnumerator LoadImage(string path, string listKey)
    {
        Texture2D tex;
        tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        using (WWW www = new WWW(path))
        {
            yield return www;

            var content = m_menuItemContents[listKey];

            www.LoadImageIntoTexture(tex);

            content.backdrop = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            content.averageColor = AverageColorFromTexture(tex, 250);

            // Because there's a small possibility that the value(s) that we sotred are overwritten and we might revert them
            // I set the other value(s) to the current ones
            content.musicClip = m_menuItemContents[listKey].musicClip;

            m_menuItemContents[listKey] = content;

        }
    }

    IEnumerator LoadSpritePack(int listIndex, SpritePackInfo info)
    {
        List<Texture2D> texs = new List<Texture2D>();

        List<string> paths = new List<string>
        {
            info.receptorName,
            info.tapnoteName,

            info.holdTopInactiveName,
            info.holdTopActiveName,

            info.holdCenterInactiveName,
            info.holdCenterActiveName,

            info.holdBottomInactiveName,
            info.holdBottomActiveName
        };

        // Load all the textures
        for (int currentIndex = 0; currentIndex < paths.Count; currentIndex++)
        {
            WWW www = new WWW(info.dir + paths[currentIndex]);

            while(!www.isDone)
            { }

            texs.Add(new Texture2D(16, 16, TextureFormat.DXT5, false));
            www.LoadImageIntoTexture(texs[currentIndex]);
        }


        SpritePack sPack = new SpritePack()
        {
            packName = info.name,

            holdBottom = new Sprite[2],
            holdCenter = new Sprite[2],
            holdTop = new Sprite[2]
        };

        for (int i = 0; i < texs.Count; i++)
        {
            Vector2 pivot = Vector2.one / 2.0f;

            // Obtain the pivot
            switch (i)
            {
                // Receptor
                case 0:
                    {
                        pivot = info.receptorPivot;
                    }
                    break;

                // Tapnote
                case 1:
                    {
                        pivot = info.tapPivot;
                    }
                    break;
                    
                // Hold head
                case 2:
                case 3:
                    {
                        pivot = info.holdTopPivot;
                    }
                    break;

                // Hold center
                case 4:
                case 5:
                    {
                        pivot = info.holdCenterPivot;
                    }
                    break;

                // Hold bottom
                case 6:
                case 7:
                    {
                        pivot = info.holdBottomPivot;
                    }
                    break;
            }

            Sprite s = Sprite.Create(texs[i], new Rect(0.0f, 0.0f, texs[i].width, texs[i].height), pivot, texs[i].width);

            // Assign the sprite
            switch (i)
            {
                // Receptor
                case 0:
                    {
                        sPack.receptor = s;  
                    }
                    break;

                // Tapnote
                case 1:
                    {
                        sPack.note = s;
                    }
                    break;

                // Hold head
                case 2:
                    {
                        sPack.holdTop[0] = s;
                    }
                    break;
                case 3:
                    {
                        sPack.holdTop[1] = s;
                    }
                    break;

                // Hold center
                case 4:
                    {
                        sPack.holdCenter[0] = s;
                    }
                    break;
                case 5:
                    {
                        sPack.holdCenter[1] = s;
                    }
                    break;

                // Hold bottom
                case 6:
                    {
                        sPack.holdBottom[0] = s;
                    }
                    break;
                case 7:
                    {
                        sPack.holdBottom[1] = s;
                    }
                    break;
            }
        }

        SpritePacks[listIndex] = sPack;

        yield return null;
    }

    Color32 AverageColorFromTexture(Texture2D tex, int downscaleRes)
    {

        Color32[] texColors = tex.GetPixels32();

        int total = texColors.Length;

        float r = 0;
        float g = 0;
        float b = 0;

        for (int i = 0; i < total; i += downscaleRes)
        {
            for (int c = 0; c < downscaleRes; c++)
            {
                r += texColors[i].r;

                g += texColors[i].g;

                b += texColors[i].b;
            }
        }

        return new Color32((byte)(r / total), (byte)(g / total), (byte)(b / total), 255);
    }

    private int GamemakerSign(float s)
    {
        if (s < 0) return -1;
        if (s > 0) return 1;
        return 0;
    }


    public float BeatTimer
    {
        get
        {
            return (AudioSource.time - CurrentSelected.SongData.offset) * (CurrentSelected.SongData.bpms[0].bpm / 60.0f);
        }
    }
}
