using System;
using UnityEngine;
using NAudio.Wave;

public static class MP3Converter
{
    public static bool Mp3ToWav(string mp3File, string outputFile)
    {
        try
        {
            using (Mp3FileReader reader = new Mp3FileReader(mp3File))
            {
                WaveFileWriter.CreateWaveFile(outputFile, reader);
            }

            return true;
        }
        catch(Exception e)
        {
            Debug.LogError("Error while trying to convert " + mp3File + ". " + e);
            return false;
        }
    }
}
