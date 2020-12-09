using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using Newtonsoft.Json;

public static class HighscoreManager
{
    private static bool m_isInitalized = false;
    private static PlayerHighscores m_highscores;
    private static string m_scoreDirectory;

    private static void InitalizeScoreData()
    {
        if (!Directory.Exists(Utility.GetRoot() + "/UserData"))
            Directory.CreateDirectory(Utility.GetRoot() + "/UserData");

        m_scoreDirectory = Utility.GetRoot() + "/UserData/Highscores.um";

        if(File.Exists(m_scoreDirectory))
        {
            string jsonData = File.ReadAllText(m_scoreDirectory);

            try
            {
                m_highscores = JsonConvert.DeserializeObject<PlayerHighscores>(jsonData);
            }
            catch
            {
                NewHighscores();
            }
        }
        else
        {
            NewHighscores();
        }

        m_isInitalized = true;
    }

    private static void NewHighscores()
    {
        m_highscores = new PlayerHighscores();
        m_highscores.Highscores = new Dictionary<string, SongScoreData>();

        Debug.Log("new highscores");
    }

    public static void AddScore(string songName, Difficulties difficulty, ScoreData scoreData)
    {
        // Check if the song exists in the data
        if (!Highscores.ContainsKey(songName))
        {
            Highscores.Add(songName, new SongScoreData());
        }

        // Check if the difficulty exists in the data
        if (!Highscores[songName].Scores.ContainsKey(difficulty))
        {
            Highscores[songName].Scores.Add(difficulty, new List<ScoreData>());
        }

        // Add the new score
        Highscores[songName].Scores[difficulty].Add(scoreData);

        // Save to file
        string jsonData = JsonConvert.SerializeObject(m_highscores, Formatting.Indented);

        File.WriteAllText(m_scoreDirectory, jsonData);
    }

    public static Dictionary<string, SongScoreData> Highscores
    {
        get
        {
            if (m_isInitalized == false)
                InitalizeScoreData();

            return m_highscores.Highscores;
        }
    }
}
