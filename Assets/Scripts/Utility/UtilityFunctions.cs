using System.IO;
using System.Linq;
using UnityEngine;

public static class Utility
{
    /// <summary>
    /// Returns -1 if negative, 1 if positive and 0 if 0
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static int GamemakerSign(float s)
    {
        if (s < 0) return -1;
        if (s > 0) return 1;
        return 0;
    }

    public static float CalculateBackdropScale(Camera camera, Sprite backdrop)
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float imageAspectRatio = backdrop.rect.width / backdrop.rect.height;
        float camHalfHeight = camera.orthographicSize;
        float camHalfWidth = screenAspect * camHalfHeight;

        // We devide the screenaspect with the image aspect to check if we need to fill it to the width or to the height
        if(screenAspect / imageAspectRatio >= 1)
        {
            return 2.0f * camHalfWidth;
        }
        else
        {
            return 2.0f * camHalfHeight;
        }       
    }
    
    public static Sounds GetHitsound(HitSounds hitSound)
    {
        Sounds sound = Sounds.NoteHitDefault;

        switch (hitSound)
        {
            case HitSounds.Default:
                sound = Sounds.NoteHitDefault;
                break;

            case HitSounds.Kick:
                sound = Sounds.SwitchTab;
                break;

            case HitSounds.Osu:
                sound = Sounds.NoteHitOsu;
                break;

            case HitSounds.Trumpet:
                sound = Sounds.NoteHitTrumpet;
                break;

            case HitSounds.Eat:
                sound = Sounds.NoteHitEat;
                break;

            case HitSounds.Bruh:
                sound = Sounds.NoteHitBruh;
                break;

            case HitSounds.Distortion:
                sound = Sounds.NoteHitDistortion;
                break;

            case HitSounds.Sans:
                sound = Sounds.NoteHitSans;
                break;
        }

        return sound;
    }

    /// <summary>
    /// Returns the root directory of the game
    /// </summary>
    /// <returns></returns>
    public static string GetRoot()
    {
        return Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/');
    }
}
