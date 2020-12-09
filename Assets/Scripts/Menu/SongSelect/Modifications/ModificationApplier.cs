using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.PostProcessing;

public class ModificationApplier : MonoBehaviour
{
    public static ModificationApplier Instance;
    public List<Resolution> Resolutions;
    public bool UpdateResolution;

    [SerializeField] private PostProcessingProfile m_uiProfile;
    [SerializeField] private PostProcessingProfile m_mainCameraProfile;
    [SerializeField] private AudioMixer m_mainMixer;

    [Space(15)]
    [SerializeField] private ModifierObject m_resolutionIndex;
    [SerializeField] private ModifierObject m_fullscreenMode;
    [SerializeField] private ModifierObject m_fpsLimit;
    [SerializeField] private ModifierObject m_vSync;
    [Space(5)]
    [SerializeField] private ModifierObject m_masterVolume;
    [SerializeField] private ModifierObject m_musicVolume;
    [SerializeField] private ModifierObject m_sfxVolume;
    [SerializeField] private ModifierObject m_masterDistortion;
    [SerializeField] private ModifierObject m_masterBassBoost;
    [SerializeField] private ModifierObject m_pitchComp;
    [Space(5)]
    [SerializeField] private ModifierObject m_hitSound;
    [SerializeField] private ModifierObject m_musicPitch;
    [SerializeField] private ModifierObject m_scrollSpeed;
    [SerializeField] private ModifierObject m_screenShake;
    [SerializeField] private ModifierObject m_noteSkin;
    [SerializeField] private ModifierObject m_hitParticleAmount;
    [SerializeField] private ModifierObject m_hitParticleVelocity;
    [Space(5)]
    [SerializeField] private ModifierObject m_songSelectBackground;
    [SerializeField] private ModifierObject m_backgroundBrightness;
    [SerializeField] private ModifierObject m_backgroundType;
    [SerializeField] private ModifierObject m_menuAnimations;
    [Space(5)]
    [SerializeField] private ModifierObject m_noteColor;
    [SerializeField] private ModifierObject m_uiLayer;
    [Space(5)]
    [SerializeField] private ModifierObject m_judgeDiff;
    [SerializeField] private ModifierObject m_noteTwist;
    [SerializeField] private ModifierObject m_receptorTwist;
    [SerializeField] private ModifierObject m_receptorWaveHor;
    [SerializeField] private ModifierObject m_receptorWaveVer;
    [SerializeField] private ModifierObject m_receptorWaveSpeed;
    [SerializeField] private ModifierObject m_notescaleMode;
    [SerializeField] private ModifierObject m_speedOverTimeMode;
    [Space(5)]
    [SerializeField] private ModifierObject m_receptorPosHor;
    [SerializeField] private ModifierObject m_receptorPosVer;
    [SerializeField] private ModifierObject m_scrollDirection;
    [Space(5)]
    [SerializeField] private ModifierObject m_postProcessing;
    [SerializeField] private ModifierObject m_bloom;
    [SerializeField] private ModifierObject m_motionBlur;
    [SerializeField] private ModifierObject m_exposure;
    [SerializeField] private ModifierObject m_chromaticAbberation;
    [SerializeField] private ModifierObject m_grain;
    [SerializeField] private ModifierObject m_vignette;
    [SerializeField] private ModifierObject m_specialEffect;
    [Space(5)]
    [SerializeField] private ModifierObject m_noteUpdates;
    [Space(5)]
    [SerializeField] private ModifierObject m_keyStart;
    [SerializeField] private ModifierObject m_keyRestart;
    [SerializeField] private ModifierObject m_keyExit;
    [SerializeField] private ModifierObject m_keyLeft;
    [SerializeField] private ModifierObject m_keyRight;
    [SerializeField] private ModifierObject m_keyUp;
    [SerializeField] private ModifierObject m_keyDown;

    private void Start()
    {
        Instance = this;
        CreateResolutions();
        UpdateResolution = true;

        // Give base values
        m_masterVolume.SetValue(Modifications.Instance.MasterVolume);
        m_masterDistortion.SetValue(Modifications.Instance.MasterDistortion);
        m_masterBassBoost.SetValue(Modifications.Instance.MasterBassBoost);

        m_musicVolume.SetValue(Modifications.Instance.MusicVolume);
        m_sfxVolume.SetValue(Modifications.Instance.SFXVolume);

        m_hitSound.SetValue((int)Modifications.Instance.HitSound);
        m_musicPitch.SetValue(Modifications.Instance.MusicPitch);

        m_scrollSpeed.SetValue(Modifications.Instance.ScrollSpeed);
        m_screenShake.SetValue(Modifications.Instance.ScreenshakeIntensity);

        m_notescaleMode.SetValue((int)Modifications.Instance.NoteScaleMode);
        m_speedOverTimeMode.SetValue(Modifications.Instance.SlowdownMod);

        m_noteSkin.SetValue(Modifications.Instance.NoteSkinIndex);

        m_songSelectBackground.SetValue(Modifications.Instance.SongselectBackground);
        m_backgroundBrightness.SetValue(Modifications.Instance.BackdropBrightness);
        m_backgroundType.SetValue((int)Modifications.Instance.BackgroundType);
        m_menuAnimations.SetValue(Modifications.Instance.MenuAnimations);

        m_noteColor.SetValue((int)Modifications.Instance.NoteColors);
        m_uiLayer.SetValue(Modifications.Instance.UILayer);
        m_hitParticleAmount.SetValue(Modifications.Instance.ParticleEmmisionScale);
        m_hitParticleVelocity.SetValue(Modifications.Instance.ParticleSpeed);

        m_judgeDiff.SetValue(Modifications.Instance.JudgeDiff);
        m_noteTwist.SetValue(Modifications.Instance.NoteTwist);
        m_receptorTwist.SetValue(Modifications.Instance.ReceptorTwist);
        m_receptorWaveHor.SetValue(Modifications.Instance.ReceptorWaveHor);
        m_receptorWaveVer.SetValue(Modifications.Instance.ReceptorWaveVer);

        m_receptorPosHor.SetValue(Modifications.Instance.ReceptorPosition+1);
        m_receptorPosVer.SetValue(Modifications.Instance.ReceptorHeight+1);
        m_receptorWaveSpeed.SetValue(Modifications.Instance.ReceptorWaveSpeed);
        m_scrollDirection.SetValue((int)Modifications.Instance.ScrollDirection);

        m_postProcessing.SetValue(Modifications.Instance.PostProcessing);
        m_bloom.SetValue(Modifications.Instance.Bloom);
        m_motionBlur.SetValue(Modifications.Instance.MotionBlur);
        m_exposure.SetValue(Modifications.Instance.PostExposure);
        m_chromaticAbberation.SetValue(Modifications.Instance.ChromaticAberration);
        m_grain.SetValue(Modifications.Instance.Grain);
        m_vignette.SetValue(Modifications.Instance.Vignette);
        m_specialEffect.SetValue(Modifications.Instance.SpecialEffect);

        m_noteUpdates.SetValue(Modifications.Instance.MaxNoteUpdates);

        m_resolutionIndex.SetValue(Modifications.Instance.ResolutionIndex);
        m_fullscreenMode.SetValue(Modifications.Instance.Fullscreen);
        m_fpsLimit.SetValue(Modifications.Instance.FPSLimit);
        m_vSync.SetValue(Modifications.Instance.VSync);
        m_pitchComp.SetValue(Modifications.Instance.PitchCompensation);

        m_keyStart.SetValue((int)Modifications.Instance.CustomKeys[KeyTypes.StartKey]);
        m_keyRestart.SetValue((int)Modifications.Instance.CustomKeys[KeyTypes.RestartKey]);
        m_keyExit.SetValue((int)Modifications.Instance.CustomKeys[KeyTypes.ExitKey]);
        m_keyLeft.SetValue((int)Modifications.Instance.CustomKeys[KeyTypes.LeftKey]);
        m_keyRight.SetValue((int)Modifications.Instance.CustomKeys[KeyTypes.RightKey]);
        m_keyUp.SetValue((int)Modifications.Instance.CustomKeys[KeyTypes.UpKey]);
        m_keyDown.SetValue((int)Modifications.Instance.CustomKeys[KeyTypes.DownKey]);
    }

    void Update ()
    {
        // Apply all values
        Modifications.Instance.MasterVolume = (int)m_masterVolume.GetValue();
        Modifications.Instance.MasterDistortion = (int)m_masterDistortion.GetValue();
        Modifications.Instance.MasterBassBoost = (int)m_masterBassBoost.GetValue();

        Modifications.Instance.MusicVolume = (int)m_musicVolume.GetValue();      
        Modifications.Instance.SFXVolume = (int)m_sfxVolume.GetValue();

        Modifications.Instance.HitSound = (HitSounds)m_hitSound.GetValue();
        Modifications.Instance.MusicPitch = (int)m_musicPitch.GetValue();

        Modifications.Instance.JudgeDiff = (int)m_judgeDiff.GetValue();
        Modifications.Instance.ScrollSpeed = (int)m_scrollSpeed.GetValue();
        Modifications.Instance.ScreenshakeIntensity = (int)m_screenShake.GetValue();
        Modifications.Instance.ParticleEmmisionScale = (int)m_hitParticleAmount.GetValue();
        Modifications.Instance.ParticleSpeed = (int)m_hitParticleVelocity.GetValue();

        Modifications.Instance.NoteSkinIndex = (int)m_noteSkin.GetValue();
        Modifications.Instance.NoteColors = (NoteColors)m_noteColor.GetValue();
        Modifications.Instance.UILayer = (int)m_uiLayer.GetValue();

        Modifications.Instance.PostProcessing = (int)m_postProcessing.GetValue();
        Modifications.Instance.Bloom = (int)m_bloom.GetValue();
        Modifications.Instance.MotionBlur = (int)m_motionBlur.GetValue();
        Modifications.Instance.PostExposure = (int)m_exposure.GetValue();
        Modifications.Instance.ChromaticAberration = (int)m_chromaticAbberation.GetValue();
        Modifications.Instance.Grain = (int)m_grain.GetValue();
        Modifications.Instance.Vignette = (int)m_vignette.GetValue();
        Modifications.Instance.SpecialEffect = (int)m_specialEffect.GetValue();

        Modifications.Instance.ScrollDirection = (ScrollDirction)m_scrollDirection.GetValue();

        Modifications.Instance.ResolutionIndex = (int)m_resolutionIndex.GetValue();
        Modifications.Instance.Fullscreen = (int)m_fullscreenMode.GetValue();
        Modifications.Instance.FPSLimit = (int)m_fpsLimit.GetValue();
        Modifications.Instance.VSync = (int)m_vSync.GetValue();

        Modifications.Instance.PitchCompensation = (int)m_pitchComp.GetValue();

        Modifications.Instance.SongselectBackground = (int)m_songSelectBackground.GetValue();
        Modifications.Instance.BackdropBrightness = (int)m_backgroundBrightness.GetValue();
        Modifications.Instance.BackgroundType = (BackgroundTypes)m_backgroundType.GetValue();
        Modifications.Instance.MaxNoteUpdates = (int)m_noteUpdates.GetValue();
        Modifications.Instance.MenuAnimations = (int)m_menuAnimations.GetValue();
        

        Modifications.Instance.NoteTwist = (int)m_noteTwist.GetValue();
        Modifications.Instance.ReceptorTwist = (int)m_receptorTwist.GetValue();
        Modifications.Instance.ReceptorWaveHor = (int)m_receptorWaveHor.GetValue();
        Modifications.Instance.ReceptorWaveVer = (int)m_receptorWaveVer.GetValue();
        Modifications.Instance.ReceptorWaveSpeed = (int)m_receptorWaveSpeed.GetValue();
        Modifications.Instance.NoteScaleMode = (NoteScaleMode)m_notescaleMode.GetValue();
        Modifications.Instance.SlowdownMod = (int)m_speedOverTimeMode.GetValue();
        

        Modifications.Instance.ReceptorPosition = (int)m_receptorPosHor.GetValue()-1;
        Modifications.Instance.ReceptorHeight = (int)m_receptorPosVer.GetValue()-1;

        Modifications.Instance.CustomKeys[KeyTypes.StartKey] = (KeyCode)m_keyStart.GetValue();
        Modifications.Instance.CustomKeys[KeyTypes.RestartKey] = (KeyCode)m_keyRestart.GetValue();
        Modifications.Instance.CustomKeys[KeyTypes.ExitKey] = (KeyCode)m_keyExit.GetValue();
        Modifications.Instance.CustomKeys[KeyTypes.LeftKey] = (KeyCode)m_keyLeft.GetValue();
        Modifications.Instance.CustomKeys[KeyTypes.RightKey] = (KeyCode)m_keyRight.GetValue();
        Modifications.Instance.CustomKeys[KeyTypes.UpKey] = (KeyCode)m_keyUp.GetValue();
        Modifications.Instance.CustomKeys[KeyTypes.DownKey] = (KeyCode)m_keyDown.GetValue();

        // Some values need direct applying as well
        float masterVolume = Mathf.Clamp((float)Modifications.Instance.MasterVolume / 100, 0.001f, Modifications.Instance.MasterVolume);
        m_mainMixer.SetFloat("masterVolume", Mathf.Log(masterVolume) * 20);

        float musicVolume = Mathf.Clamp((float)Modifications.Instance.MusicVolume / 100, 0.001f, Modifications.Instance.MusicVolume);
        m_mainMixer.SetFloat("musicVolume", Mathf.Log(musicVolume) * 20);

        float sfxVolume = Mathf.Clamp((float)Modifications.Instance.SFXVolume / 100, 0.001f, Modifications.Instance.SFXVolume);
        m_mainMixer.SetFloat("sfxVolume", Mathf.Log(sfxVolume) * 20);

        float masterDistortion = Mathf.Clamp((float)Modifications.Instance.MasterDistortion / 100, 0.001f, Modifications.Instance.MasterDistortion);
        m_mainMixer.SetFloat("masterDistortion", masterDistortion);

        float masterBassBoost = (float)Modifications.Instance.MasterBassBoost / 100.0f * 2 + 1;
        m_mainMixer.SetFloat("masterBassBoost", masterBassBoost);

        #region PostProcessing
        MotionBlurModel.Settings motionBlur = m_uiProfile.motionBlur.settings;
        BloomModel.Settings bloom = m_uiProfile.bloom.settings;
        ColorGradingModel.Settings colorGrading = m_uiProfile.colorGrading.settings;
        ChromaticAberrationModel.Settings chromaticAbberation = m_uiProfile.chromaticAberration.settings;
        GrainModel.Settings grain = m_uiProfile.grain.settings;
        VignetteModel.Settings vignette = m_uiProfile.vignette.settings;
        BuiltinDebugViewsModel.Settings debugMode = m_uiProfile.debugViews.settings;

        motionBlur.frameBlending = 0.01f * (Modifications.Instance.MotionBlur) * Modifications.Instance.PostProcessing;
        bloom.bloom.intensity = 0.5f * (Modifications.Instance.Bloom) * Modifications.Instance.PostProcessing;
        colorGrading.basic.postExposure = 0.35f * (Modifications.Instance.PostExposure) * Modifications.Instance.PostProcessing;
        chromaticAbberation.intensity = 0.25f * (Modifications.Instance.ChromaticAberration) * Modifications.Instance.PostProcessing;
        grain.intensity = 0.5f * (Modifications.Instance.Grain) * Modifications.Instance.PostProcessing;
        vignette.intensity = 0.4f * (Modifications.Instance.Vignette) * Modifications.Instance.PostProcessing;
        debugMode.mode = (Modifications.Instance.SpecialEffect == 0) ? BuiltinDebugViewsModel.Mode.None : BuiltinDebugViewsModel.Mode.Depth;

        m_uiProfile.bloom.settings = bloom;
        m_uiProfile.chromaticAberration.settings = chromaticAbberation;
        m_uiProfile.colorGrading.settings = colorGrading;
        m_uiProfile.motionBlur.settings = motionBlur;
        m_uiProfile.grain.settings = grain;
        m_uiProfile.vignette.settings = vignette;
        m_uiProfile.debugViews.settings = debugMode;

        m_mainCameraProfile = m_uiProfile;

        m_mainCameraProfile.depthOfField.enabled = true;
        m_uiProfile.depthOfField.enabled = false;
        #endregion
        float currentPitch;

        m_mainMixer.GetFloat("musicPitch", out currentPitch);
        m_mainMixer.SetFloat("musicPitch", Mathf.Lerp(currentPitch, Modifications.Instance.MusicPitch / 100.0f, 11 * Time.deltaTime));

        float pitchCompDest, currentPitchComp;

        m_mainMixer.GetFloat("musicPitchShifter", out currentPitchComp);
        pitchCompDest = (Modifications.Instance.PitchCompensation == 1) ? 1 / (Modifications.Instance.MusicPitch / 100.0f) : 1;

        m_mainMixer.SetFloat("musicPitchShifter", Mathf.Lerp(currentPitchComp, pitchCompDest, 11 * Time.deltaTime));

        QualitySettings.vSyncCount = Modifications.Instance.VSync;

        if(UpdateResolution)
        {
            Resolution r = Resolutions[Modifications.Instance.ResolutionIndex];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
            UpdateResolution = false;
        }

        Screen.fullScreen = Convert.ToBoolean(Modifications.Instance.Fullscreen);
    }

    private void CreateResolutions()
    {
        int amount = 30;
        Resolutions = new List<Resolution>();

        for (int i = 0; i <= amount; i++)
        {
            Resolution r = new Resolution();
            r.width = (16 * (i+1)) * 8;
            r.height = (9 * (i+1)) * 8;

            Resolutions.Add(r);
        }
    }
}
