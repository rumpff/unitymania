using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleEasing;
using System;

public class ModifierTab : MonoBehaviour
{
    [SerializeField] private ModifierWindow m_modWindow;
    [SerializeField] private string m_tabName;

    private List<ModifierObject> m_objects;
    private int m_currentSelected;
    private RectTransform m_selector;
    private RectTransform m_rectTransform;

    private List<float> m_buttonAnimationStarts;
    private List<float> m_buttonDestinations;
    private float m_buttonMoveTimer;

    private void Start()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_selector = m_modWindow.Selector;
        m_objects = transform.GetComponentsInChildren<ModifierObject>().ToList();
        m_buttonDestinations = new List<float>();
        m_buttonAnimationStarts = new List<float>();

        for (int i = 0; i < m_objects.Count; i++)
        {
            m_buttonDestinations.Add(0);
            m_buttonAnimationStarts.Add(0);

            m_objects[i].ModWindow = m_modWindow;
        }

        m_currentSelected = 0;

        // Set the buttons on the correct position
        MoveButtons(0);

        // Skip the move animation
        m_buttonMoveTimer = m_modWindow.MoveDuration;
    }

    private void Update()
    {
        // Set the positions of the buttons
        for (int i = 0; i < m_objects.Count; i++)
        {
            Vector2 buttonPosition = Vector2.zero;
            switch (m_modWindow.State)
            {
                case ModifierWindowStates.optionSelected:
                    buttonPosition = new Vector2
                    {
                        x = 0,
                        y = Easing.easeOutBounce(Mathf.Clamp(m_buttonMoveTimer, 0, m_modWindow.MoveDuration * 2), m_buttonAnimationStarts[i], m_buttonDestinations[i] - m_buttonAnimationStarts[i], m_modWindow.MoveDuration * 2)
                    };
                    break;

                default:
                    buttonPosition = new Vector2
                    {
                        x = 0,
                        y = Easing.easeOutBack(Mathf.Clamp(m_buttonMoveTimer, 0, m_modWindow.MoveDuration), m_buttonAnimationStarts[i], m_buttonDestinations[i] - m_buttonAnimationStarts[i], m_modWindow.MoveDuration)
                    };
                    break;
            }


            m_objects[i].RectTransform.anchoredPosition = buttonPosition;
        }

        // Update the selector
        m_selector.anchoredPosition = new Vector2(0, m_objects[m_currentSelected].RectTransform.anchoredPosition.y + 50);
        m_selector.sizeDelta = Vector2.Lerp(m_selector.sizeDelta, new Vector2(0, 100 + (m_modWindow.ButtonSpacing * 2 * Convert.ToInt32(m_modWindow.ObjectSelected))), 32 * Time.deltaTime);

        // Time the timer
        m_buttonMoveTimer += Time.deltaTime;
    }

    public void MoveButtons(int amount)
    {
        int newPos = m_currentSelected + amount;

        // Warp the selection
        while (newPos < 0)
        {
            newPos += m_objects.Count;
        }
        while (newPos >= m_objects.Count)
        {
            newPos -= m_objects.Count;
        }

        m_currentSelected = newPos;

        UpdateButtons();

        // Update the description
        m_modWindow.SetNewDescription(m_objects[m_currentSelected].Description);
    }
    public void UpdateButtons()
    {
        // Calculate the new positions for the buttons
        for (int i = 0; i < m_objects.Count; i++)
        {
            float extraSpacing = Utility.GamemakerSign(i - m_currentSelected) * m_modWindow.ButtonSpacing;
            float distanceFromCenter = (i - m_currentSelected) * m_modWindow.ButtonSpacing + (extraSpacing * Convert.ToInt32(m_modWindow.ObjectSelected));
            m_buttonDestinations[i] = -distanceFromCenter;
            m_buttonAnimationStarts[i] = m_objects[i].RectTransform.anchoredPosition.y;
        }

        // Reset the timer
        m_buttonMoveTimer = 0;
    }
    public void SetWindow(ModifierWindow w)
    {
        m_modWindow = w;
    }
    public RectTransform RectTransform
    {
        get { return m_rectTransform; }
    }
    public string TabName
    {
        get { return m_tabName; }
    }
    public List<ModifierObject> Objects
    {
        get { return m_objects; }
    }
    public ModifierObject SelectedObject
    {
        get { return m_objects[m_currentSelected]; }
    }
}
