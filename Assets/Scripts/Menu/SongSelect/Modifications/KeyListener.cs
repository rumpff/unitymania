using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyListener : MonoBehaviour
{
    public delegate void KeyPressed(KeyCode key);
    public event KeyPressed KeyPressedEvent;

    public bool ListenForKeys { get; set; }

    private void Awake()
    {
        ListenForKeys = false;
    }

    private void Update()
    {
        if(ListenForKeys)
        {
            foreach(KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if(Input.GetKeyDown(key))
                {
                    OnKeyPress(key);
                }
            }
        }
    }

    protected virtual void OnKeyPress(KeyCode key)
    {
        if (KeyPressedEvent != null)
            KeyPressedEvent(key);
    }
}
