using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static AudioManager m_instance;
    private AudioMixer m_audioMixer;

    private AudioSource m_audioSource;
    private Dictionary<Sounds, AudioClip> m_sfxs;

    public static AudioManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<AudioManager>();
                if (m_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(AudioManager).Name;
                    m_instance = obj.AddComponent<AudioManager>();
                    m_instance.Initalize();
                    DontDestroyOnLoad(obj);
                }
            }

            return m_instance;
        }
    }

    private void Initalize()
    {
        // Load the mixer
        m_audioMixer = Resources.Load("Audio/MainMixer") as AudioMixer;
        
        // Initalize the audioSource
        m_audioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        m_audioSource.outputAudioMixerGroup = m_audioMixer.FindMatchingGroups("SFXGroup")[0];

        // Load all the sound effects
        int enumLength = Enum.GetNames(typeof(Sounds)).Length;
        m_sfxs = new Dictionary<Sounds, AudioClip>();

        for (int i = 0; i < enumLength; i++)
        {
            Sounds sounds = (Sounds)i;
            string stringValue = sounds.ToString();

            m_sfxs.Add((Sounds)i, (AudioClip)Resources.Load("Audio/SFX/" + stringValue));
        }
    }

    public void PlaySound(Sounds sound)
    {
        m_audioSource.PlayOneShot(m_sfxs[sound]);
    }
}
