using SimpleEasing;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;

public class ModifierWindow : MonoBehaviour
{
    [SerializeField] private SongSelect m_songSelect;
    [SerializeField] private RectTransform m_selector, m_uiTabParent;
    private RectTransform m_rectTransform;
    
    private ModifierTab[] m_tabs;
    private ModifierUITab[] m_uiTabs;

    private KeyListener m_keyListener;

    private bool m_isActive;
    private int m_currentTab;

    private float m_windowPositionTimer;

    [SerializeField] private float m_buttonSpacing;

    [SerializeField] private TextMeshProUGUI m_descriptionText;
    [SerializeField] private float m_buttonMoveDuration;

    [SerializeField] private float m_uiTabStartingPos;
    [SerializeField] private float m_uiTabSpacing;

    [SerializeField] [TextArea] private string m_editDescription;
    [SerializeField] [TextArea] private string m_editKeyCodeDescription;

    private ModifierWindowStates m_state;

    [SerializeField] private float m_horizontalScrollDelay;
    private float m_horizontalScrollTimer;

    [SerializeField] private float m_verticalScrollDelay;
    private float m_verticalScrollTimer;


    private void Start()
    {
        m_keyListener = GetComponent<KeyListener>();
        m_rectTransform = GetComponent<RectTransform>();

        m_tabs = transform.GetComponentsInChildren<ModifierTab>();
        m_currentTab = 0;

        m_state = ModifierWindowStates.normal;
        m_isActive = false;
        m_windowPositionTimer = 1;

        var uiTabPrefab = Resources.Load<GameObject>("Prefabs/Menu/SongSelect/UITab");
        m_uiTabs = new ModifierUITab[m_tabs.Length];

        for(int i = 0; i < m_uiTabs.Length; i++)
        {
            var uiTab = Instantiate(uiTabPrefab, m_uiTabParent).GetComponent<ModifierUITab>();

            uiTab.SetTitle(m_tabs[i].TabName);
            m_uiTabs[i] = uiTab;
        }

        m_keyListener.KeyPressedEvent += KeyHit;
        m_verticalScrollTimer = 0;
        m_horizontalScrollTimer = 0;
    }
    private void OnDestroy()
    {
        UnSubscribeKeys();
        m_keyListener.KeyPressedEvent -= KeyHit;
    }

    private void Update()
    {
        // Set the position of the window
        Vector3 windowPos = Vector3.zero;
        Vector3 windowAngle = Vector3.zero;
        Vector3 windowScale = Vector3.one;

        Vector3 localOffset = Vector3.zero;

        if (m_isActive)
        {
            localOffset = new Vector3()
            {
                x = 0,
                y = 0,
                z = Easing.easeOutExpo(Mathf.Clamp01(m_windowPositionTimer * 3.5f), 10000, -10000, 1)
            };
        }
        else
        {
            windowAngle.y = Easing.easeOutExpo(Mathf.Clamp01(m_windowPositionTimer * 2), 0, -180, 1);
            windowPos.z = Easing.easeInExpo(Mathf.Clamp01(m_windowPositionTimer * 2), 0, 10000, 1);

            // Hide the window
            if (m_windowPositionTimer == 1)
                windowScale = Vector3.zero;
        }


        m_rectTransform.anchoredPosition3D = windowPos + localOffset;
        m_rectTransform.localEulerAngles = windowAngle;
        m_rectTransform.localScale = windowScale;

        m_windowPositionTimer += Time.deltaTime;
        m_windowPositionTimer = Mathf.Clamp01(m_windowPositionTimer);

        if (m_songSelect.State != SongSelectState.inModifiers)
            return;

        // Update UI Tabs
        for (int i = 0; i < m_uiTabs.Length; i++)
        {
            bool selected = (i == m_currentTab);
            float colVal = 1 - (Convert.ToInt32(selected) * 0.4f) + 0.5f;

            // Set Circle pos
            float tabAngle = (i - m_currentTab) / (float)m_uiTabs.Length * 360 - 90;

            m_uiTabs[i].SetAngle(tabAngle);
        }

        // Update tabs
        for (int i = 0; i < m_tabs.Length; i++)
        {
            m_tabs[i].gameObject.SetActive(i == m_currentTab);
        }

        // Update Modifier objects
        for (int i = 0; i < m_tabs[m_currentTab].Objects.Count; i++)
        {
            bool selected = (m_tabs[m_currentTab].Objects[i] == m_tabs[m_currentTab].SelectedObject);
            m_tabs[m_currentTab].Objects[i].IsHighlighted = selected;
        }

        // Animate the tab to normal
        float currentLeft = m_tabs[m_currentTab].RectTransform.offsetMin.x;
        float currentRight = -m_tabs[m_currentTab].RectTransform.offsetMax.x;
        float currentTop = -m_tabs[m_currentTab].RectTransform.offsetMax.y;
        float currentBottom = m_tabs[m_currentTab].RectTransform.offsetMin.y;

        float left = Mathf.Lerp(currentLeft, 0, 20 * Time.deltaTime);
        float right = Mathf.Lerp(currentRight, 0, 20 * Time.deltaTime);

        m_tabs[m_currentTab].RectTransform.offsetMin = new Vector2(left, currentBottom);
        m_tabs[m_currentTab].RectTransform.offsetMax = new Vector2(-right, -currentTop);

        // Check if need to listen for keys
        m_keyListener.ListenForKeys = (m_state == ModifierWindowStates.optionSelected && m_tabs[m_currentTab].SelectedObject.ValueIsKey);
    }

    private void MoveTab(int amount)
    {
        int newPos = m_currentTab + amount;

        // Warp the selection
        while (newPos < 0)
        {
            newPos += m_tabs.Length;
        }
        while (newPos >= m_tabs.Length)
        {
            newPos -= m_tabs.Length;
        }

        m_currentTab = newPos;

        // Update the data
        m_tabs[m_currentTab].MoveButtons(0);
    }
    public void ForceMoveUp()
    {
        m_verticalScrollTimer += Time.deltaTime;

        if (m_verticalScrollTimer >= m_verticalScrollDelay)
        {
            m_verticalScrollTimer = 0;
            m_tabs[m_currentTab].MoveButtons(-1);
            AudioManager.Instance.PlaySound(Sounds.ModMoveSelection);
        }
    }
    public void ForceMoveDown()
    {
        m_verticalScrollTimer += Time.deltaTime;

        if (m_verticalScrollTimer >= m_verticalScrollDelay)
        {
            m_verticalScrollTimer = 0;
            m_tabs[m_currentTab].MoveButtons(1);
            AudioManager.Instance.PlaySound(Sounds.ModMoveSelection);
        }
    }
    private void SubscribeKeys()
    {
        InputHandler.Instance.KeyUpHeldEvent += MoveUp;
        InputHandler.Instance.KeyDownHeldEvent += MoveDown;

        InputHandler.Instance.KeyLeftHeldEvent += MoveLeft;
        InputHandler.Instance.KeyRightHeldEvent += MoveRight;

        InputHandler.Instance.KeyLeftEvent += SwitchTabLeft;
        InputHandler.Instance.KeyRightEvent += SwitchTabRight;

        InputHandler.Instance.NoKeyHorizontalEvent += NoHorizontal;
        InputHandler.Instance.NoKeyVerticalEvent += NoVertical;

        InputHandler.Instance.KeyStartEvent += Enter;
        InputHandler.Instance.KeyExitEvent += Escape;

        InputHandler.Instance.NoKeyHorizontalEvent += NoHorizontal;
    }
    private void UnSubscribeKeys()
    {
        InputHandler.Instance.KeyUpHeldEvent -= MoveUp;
        InputHandler.Instance.KeyDownHeldEvent -= MoveDown;

        InputHandler.Instance.KeyLeftHeldEvent -= MoveLeft;
        InputHandler.Instance.KeyRightHeldEvent -= MoveRight;

        InputHandler.Instance.KeyLeftEvent -= SwitchTabLeft;
        InputHandler.Instance.KeyRightEvent -= SwitchTabRight;

        InputHandler.Instance.NoKeyHorizontalEvent -= NoHorizontal;
        InputHandler.Instance.NoKeyVerticalEvent -= NoVertical;

        InputHandler.Instance.KeyStartEvent -= Enter;
        InputHandler.Instance.KeyExitEvent -= Escape;

        InputHandler.Instance.NoKeyHorizontalEvent -= NoHorizontal;
    }

    // Input Handlers
    public void MoveUp()
    {
        if (m_state == ModifierWindowStates.normal)
        {
            m_verticalScrollTimer += Time.deltaTime;

            if (m_verticalScrollTimer >= m_verticalScrollDelay)
            {
                m_verticalScrollTimer = 0;
                m_tabs[m_currentTab].MoveButtons(-1);
                AudioManager.Instance.PlaySound(Sounds.ModMoveSelection);
            }
        }
    }
    public void MoveDown()
    {
        if (m_state == ModifierWindowStates.normal)
        {
            m_verticalScrollTimer += Time.deltaTime;

            if (m_verticalScrollTimer >= m_verticalScrollDelay)
            {
                m_verticalScrollTimer = 0;
                m_tabs[m_currentTab].MoveButtons(1);
                AudioManager.Instance.PlaySound(Sounds.ModMoveSelection);
            }
        }
    }
    public void MoveLeft()
    {
        Move(-1);
    }
    public void MoveRight()
    {
        Move(1);
    }
    private void Move(int amount)
    {
        if (m_state == ModifierWindowStates.optionSelected)
        {
            if ((m_horizontalScrollTimer <= 0) || m_horizontalScrollTimer == m_horizontalScrollDelay)
            {
                if (m_tabs[m_currentTab].SelectedObject.MoveValue(amount))
                {
                    if(m_tabs[m_currentTab].SelectedObject.IsHitSound)
                        AudioManager.Instance.PlaySound(Utility.GetHitsound((HitSounds)(int)m_tabs[m_currentTab].SelectedObject.GetValue())); // Play hitsound
                    else
                        AudioManager.Instance.PlaySound(Sounds.ModValueChange);
                }

                if (m_horizontalScrollTimer <= 0)
                    m_horizontalScrollTimer = 0.03f;
            }

            m_horizontalScrollTimer -= Time.deltaTime;
        }
    }
    public void SwitchTabLeft()
    {
        if (m_state == ModifierWindowStates.normal)
        {
            float currentTop = -m_tabs[m_currentTab].RectTransform.offsetMax.y;
            float currentBottom = m_tabs[m_currentTab].RectTransform.offsetMin.y;

            m_tabs[m_currentTab].RectTransform.offsetMin = new Vector2(0, currentBottom);
            m_tabs[m_currentTab].RectTransform.offsetMax = new Vector2(-0, -currentTop);

            MoveTab(-1);
            AudioManager.Instance.PlaySound(Sounds.SwitchTab);

            m_tabs[m_currentTab].RectTransform.offsetMax = new Vector2(-m_rectTransform.sizeDelta.x, -currentTop);
        }
    }
    public void SwitchTabRight()
    {
        if (m_state == ModifierWindowStates.normal)
        {
            float currentTop = -m_tabs[m_currentTab].RectTransform.offsetMax.y;
            float currentBottom = m_tabs[m_currentTab].RectTransform.offsetMin.y;

            m_tabs[m_currentTab].RectTransform.offsetMin = new Vector2(0, currentBottom);
            m_tabs[m_currentTab].RectTransform.offsetMax = new Vector2(-0, -currentTop);

            MoveTab(1);
            AudioManager.Instance.PlaySound(Sounds.SwitchTab);

            m_tabs[m_currentTab].RectTransform.offsetMin = new Vector2(m_rectTransform.sizeDelta.x, currentBottom);
        }
    }
    public void NoVertical()
    {
        m_verticalScrollTimer = m_verticalScrollDelay;
    }
    public void NoHorizontal()
    {
        m_horizontalScrollTimer = m_horizontalScrollDelay;
    }
    public void Enter()
    {
        switch (m_state)
        {
            // Select option
            case ModifierWindowStates.normal:
                m_state = ModifierWindowStates.optionSelected;
                m_tabs[m_currentTab].UpdateButtons();

                if(m_tabs[m_currentTab].SelectedObject.ValueIsKey)
                {
                    SetNewDescription(m_editKeyCodeDescription);
                }
                else
                {
                    SetNewDescription(m_editDescription);
                }

                AudioManager.Instance.PlaySound(Sounds.ModSelect);

                break;

            // Apply changes
            case ModifierWindowStates.optionSelected:
                // Apply values
                m_state = ModifierWindowStates.normal;
                m_tabs[m_currentTab].UpdateButtons();

                Modifications.WriteData();

                // Update the description
                m_tabs[m_currentTab].MoveButtons(0);

                // Check if the object wants the resolution updated
                if(m_tabs[m_currentTab].SelectedObject.UpdateResolution)
                {
                    ModificationApplier.Instance.UpdateResolution = true;
                }

                // Play a sound
                AudioManager.Instance.PlaySound(Sounds.ModApply);

                m_verticalScrollTimer = -1;
                break;
        }
    }
    public void Escape()
    {
        switch (m_state)
        {
            // Exit modifications
            case ModifierWindowStates.normal:
                m_songSelect.ChangeState(SongSelectState.preview);
                m_songSelect.RevealButtons();
                break;

            // Revert changes
            case ModifierWindowStates.optionSelected:
                m_tabs[m_currentTab].SelectedObject.SetDefault();

                // Play a sound
                AudioManager.Instance.PlaySound(Sounds.ModRevert);

                // Pretent like we hit enter so that the new value saves
                Enter();
                break;
        }
    }
    public void KeyHit(KeyCode key)
    {
        // Check if we are in selection and if the object wants a key
        if (m_state == ModifierWindowStates.optionSelected && m_tabs[m_currentTab].SelectedObject.ValueIsKey)
        {
            bool isDefaultKey = (Modifications.Instance.DefaultKeys.ContainsValue(key));
            bool isCustomKey = (Modifications.Instance.CustomKeys.ContainsValue(key));

            if(isDefaultKey || isCustomKey)
            {
                AudioManager.Instance.PlaySound(Sounds.Error);
                if(isDefaultKey)
                {
                    SetNewDescription("You can't use the default keys!");
                }
                else if(isCustomKey)
                {
                    SetNewDescription("Key already in use!");
                }
            }
            else
            {
                // Set the key
                m_tabs[m_currentTab].SelectedObject.SetValue((int)key);

                // Pretend like we've hit enter so that we apply the value
                Enter();
            }
        }
    }

    // Public functions
    public void SetActive(bool active)
    {
        if(active && !m_isActive)
        {
            m_windowPositionTimer = 0;
            SubscribeKeys();
        }
        if(!active && m_isActive)
        {
            m_windowPositionTimer = 0;
            UnSubscribeKeys();
        }

        m_isActive = active;
    }
    public void SetNewDescription(string description)
    {
        //m_descriptionText.text = description;
        StartCoroutine(ChangeDescription(description));
    }
    public ModifierWindowStates State
    {
        get { return m_state; }
    }
    public float MoveDuration
    {
        get { return m_buttonMoveDuration; }
    }
    public float ButtonSpacing
    {
        get { return m_buttonSpacing; }
    }
    public bool ObjectSelected
    {
        get { return m_state == ModifierWindowStates.optionSelected; }
    }
    public RectTransform Selector
    {
        get { return m_selector; }
    }

    // Courotines
    private IEnumerator ChangeDescription(string newText)
    {
        string oldText = m_descriptionText.text;
        float timer = 0;
        float duration = 0.33f;
        float yMove = 60;

        while(timer < duration)
        {
            // Calculate
            float alpha = Mathf.Abs(Easing.easeInOutExpo(timer, -1, 2, duration));
            float y = 0;
            string description = "";

            if(timer <= duration / 2.0f)
            {
                y = Easing.easeInExpo(timer, 0, yMove, duration / 2);
                description = oldText;
            }
            else
            {
                y = Easing.easeOutExpo(timer - (duration / 2), -yMove, yMove, duration / 2);
                description = newText;
            }

            // Apply
            m_descriptionText.text = description;
            m_descriptionText.color = new Color(m_descriptionText.color.r, m_descriptionText.color.g, m_descriptionText.color.b, alpha);
            m_descriptionText.rectTransform.anchoredPosition = new Vector2(0, y);

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // Set to normal
        m_descriptionText.text = newText;
        m_descriptionText.color = new Color(m_descriptionText.color.r, m_descriptionText.color.g, m_descriptionText.color.b, 1);
        m_descriptionText.rectTransform.anchoredPosition = new Vector2(0, 0);
    }
}
