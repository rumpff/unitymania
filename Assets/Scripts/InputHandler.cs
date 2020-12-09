using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private static InputHandler m_instance;

    #region Delegates
    // Delegates for key presses
    public delegate void KeyLeft();
    public delegate void KeyDown();
    public delegate void KeyUp();
    public delegate void KeyRight();

    public delegate void KeyStart();
    public delegate void KeyExit();

    // Delegates for key helds
    public delegate void KeyLeftHeld();
    public delegate void KeyDownHeld();
    public delegate void KeyUpHeld();
    public delegate void KeyRightHeld();

    // Delegates for key releases
    public delegate void KeyLeftRelease();
    public delegate void KeyDownRelease();
    public delegate void KeyUpRelease();
    public delegate void KeyRightRelease();

    // Delegates for when no key is pressed
    public delegate void NoKeyHorizontal();
    public delegate void NoKeyVertical();
    #endregion

    #region Events
    // Events - key press
    public event Action KeyLeftEvent;
    public event Action KeyDownEvent;
    public event Action KeyUpEvent;
    public event Action KeyRightEvent;

    public event Action KeyStartEvent;
    public event Action KeyRestartEvent;
    public event Action KeyExitEvent;

    // Events - key helds
    public event Action KeyLeftHeldEvent;
    public event Action KeyDownHeldEvent;
    public event Action KeyUpHeldEvent;
    public event Action KeyRightHeldEvent;

    // Events - key releases
    public event Action KeyLeftReleaseEvent;
    public event Action KeyDownReleaseEvent;
    public event Action KeyUpReleaseEvent;
    public event Action KeyRightReleaseEvent;

    // Events - no keys
    public event Action NoKeyHorizontalEvent;
    public event Action NoKeyVerticalEvent;
    #endregion

    #region Keys

    private KeyCode m_leftKey;
    private KeyCode m_downKey;
    private KeyCode m_upKey;
    private KeyCode m_rightKey;

    private KeyCode m_startKey;
    private KeyCode m_exitKey;


    private KeyCode m_leftKeyCustom;
    private KeyCode m_downKeyCustom;
    private KeyCode m_upKeyCustom;
    private KeyCode m_rightKeyCustom;

    private KeyCode m_startKeyCustom;
    private KeyCode m_exitKeyCustom;

    private KeyCode m_restartKeyCustom;

    #endregion

    public static InputHandler Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<InputHandler>();
                if (m_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(InputHandler).Name;
                    m_instance = obj.AddComponent<InputHandler>();
                    DontDestroyOnLoad(obj);
                }
            }

            return m_instance;
        }
    }

    void Update()
    {
        UpdateKeys();

        // Events - key press
        if (Input.GetKeyDown(m_leftKey) || Input.GetKeyDown(m_leftKeyCustom)) { OnKeyLeft(); }
        if (Input.GetKeyDown(m_downKey) || Input.GetKeyDown(m_downKeyCustom)) { OnKeyDown(); }
        if (Input.GetKeyDown(m_upKey) || Input.GetKeyDown(m_upKeyCustom)) { OnKeyUp(); }
        if (Input.GetKeyDown(m_rightKey) || Input.GetKeyDown(m_rightKeyCustom)) { OnKeyRight(); }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(m_startKeyCustom)) { OnKeyStart(); }
        if (Input.GetKeyDown(m_restartKeyCustom)) { OnKeyRestart(); }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(m_exitKeyCustom)) { OnKeyExit(); }

        // Events - key helds
        if (Input.GetKey(m_leftKey) || Input.GetKey(m_leftKeyCustom)) { OnKeyLeftHeld(); }
        if (Input.GetKey(m_downKey) || Input.GetKey(m_downKeyCustom)) { OnKeyDownHeld(); }
        if (Input.GetKey(m_upKey) || Input.GetKey(m_upKeyCustom)) { OnKeyUpHeld(); }
        if (Input.GetKey(m_rightKey) || Input.GetKey(m_rightKeyCustom)) { OnKeyRightHeld(); }

        // Events - key releases
        if (Input.GetKeyUp(m_leftKey) || Input.GetKeyUp(m_leftKeyCustom)) { OnKeyLeftRelease(); }
        if (Input.GetKeyUp(m_downKey) || Input.GetKeyUp(m_downKeyCustom)) { OnKeyDownRelease(); }
        if (Input.GetKeyUp(m_upKey) || Input.GetKeyUp(m_upKeyCustom)) { OnKeyUpRelease(); }
        if (Input.GetKeyUp(m_rightKey) || Input.GetKeyUp(m_rightKeyCustom)) { OnKeyRightRelease(); }

        // Events - no keys

        if (!(Input.GetKey(m_leftKey) || Input.GetKey(m_leftKeyCustom)) &&
            !(Input.GetKey(m_rightKey) || Input.GetKey(m_rightKeyCustom)))
            { OnNoKeyHorizontal(); }

        if (!(Input.GetKey(m_upKey) || Input.GetKey(m_upKeyCustom)) &&
            !(Input.GetKey(m_downKey) || Input.GetKey(m_downKeyCustom)))
            { OnNoKeyVertical(); }
    }

    void UpdateKeys()
    {
        m_leftKey = Modifications.Instance.DefaultKeys[KeyTypes.LeftKey];
        m_downKey = Modifications.Instance.DefaultKeys[KeyTypes.DownKey];
        m_upKey = Modifications.Instance.DefaultKeys[KeyTypes.UpKey];
        m_rightKey = Modifications.Instance.DefaultKeys[KeyTypes.RightKey];

        m_startKey = Modifications.Instance.DefaultKeys[KeyTypes.StartKey];
        m_exitKey = Modifications.Instance.DefaultKeys[KeyTypes.ExitKey];

        m_leftKeyCustom = Modifications.Instance.CustomKeys[KeyTypes.LeftKey];
        m_downKeyCustom = Modifications.Instance.CustomKeys[KeyTypes.DownKey];
        m_upKeyCustom = Modifications.Instance.CustomKeys[KeyTypes.UpKey];
        m_rightKeyCustom = Modifications.Instance.CustomKeys[KeyTypes.RightKey];

        m_startKeyCustom = Modifications.Instance.CustomKeys[KeyTypes.StartKey];
        m_exitKeyCustom = Modifications.Instance.CustomKeys[KeyTypes.ExitKey];

        m_restartKeyCustom = Modifications.Instance.CustomKeys[KeyTypes.RestartKey];
    }

    #region Event Raisers

    protected virtual void OnKeyLeft()
    {
        if (KeyLeftEvent != null)
            KeyLeftEvent();
            
    }
    protected virtual void OnKeyDown()
    {
        if (KeyDownEvent != null)
            KeyDownEvent();

    }
    protected virtual void OnKeyUp()
    {
        if (KeyUpEvent != null)
            KeyUpEvent();

    }
    protected virtual void OnKeyRight()
    {
        if (KeyRightEvent != null)
            KeyRightEvent();

    }

    protected virtual void OnKeyStart()
    {
        if (KeyStartEvent != null)
            KeyStartEvent();

    }
    protected virtual void OnKeyRestart()
    {
        if (KeyRestartEvent != null)
            KeyRestartEvent();
    }
    protected virtual void OnKeyExit()
    {
        if (KeyExitEvent != null)
            KeyExitEvent();

    }

    protected virtual void OnKeyLeftHeld()
    {
        if (KeyLeftHeldEvent != null)
            KeyLeftHeldEvent();

    }
    protected virtual void OnKeyDownHeld()
    {
        if (KeyDownHeldEvent != null)
            KeyDownHeldEvent();

    }
    protected virtual void OnKeyUpHeld()
    {
        if (KeyUpHeldEvent != null)
            KeyUpHeldEvent();

    }
    protected virtual void OnKeyRightHeld()
    {
        if (KeyRightHeldEvent != null)
            KeyRightHeldEvent();

    }

    protected virtual void OnKeyLeftRelease()
    {
        if (KeyLeftReleaseEvent != null)
            KeyLeftReleaseEvent();

    }
    protected virtual void OnKeyDownRelease()
    {
        if (KeyDownReleaseEvent != null)
            KeyDownReleaseEvent();

    }
    protected virtual void OnKeyUpRelease()
    {
        if (KeyUpReleaseEvent != null)
            KeyUpReleaseEvent();

    }
    protected virtual void OnKeyRightRelease()
    {
        if (KeyRightReleaseEvent != null)
            KeyRightReleaseEvent();

    }

    protected virtual void OnNoKeyHorizontal()
    {
        if (NoKeyHorizontalEvent != null)
            NoKeyHorizontalEvent();

    }
    protected virtual void OnNoKeyVertical()
    {
        if (NoKeyVerticalEvent != null)
            NoKeyVerticalEvent();

    }

    #endregion
}