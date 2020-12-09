using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class SongParser
{
    private const float SAMPLE_LENGTH_DEFAULT = 15.0f;

    private static AudioClip m_clip;
    private static AudioClip m_clipOld;

    public static Metadata Parse(string filePath)
    {
        m_clipOld = m_clip;

        if (IsNullOrWhiteSpace(filePath))
        {
            //Error
            Metadata tempMeta = new Metadata();
            tempMeta.valid = false;
            return tempMeta;
        }

        bool inNotes = false;

        Metadata songData = new Metadata();

        // Initialise Metadata
        // If it encounters any major errors during parsing, this is set to false and the song cannot be selected
        songData.valid = true;

        songData.notedata = new Dictionary<Difficulties, NoteData>();
        songData.difficultyExists = new Dictionary<Difficulties, bool>();


        foreach (Difficulties diff in (Difficulties[])Enum.GetValues(typeof(Difficulties)))
        {
            songData.difficultyExists[diff] = false;
        }

        //Collect the data from the sm file
        List<string> fileData = File.ReadAllLines(filePath).ToList();

        string fileDir = Path.GetDirectoryName(filePath);
        if (!fileDir.EndsWith("\\") && !fileDir.EndsWith("/"))
        {
            fileDir += "\\";
        }

        for (int i = 0; i < fileData.Count; i++)
        {
            string line = fileData[i].Trim();
            

            if (line.StartsWith("//"))
            {
                continue;
            }
            else if (line.StartsWith("#"))
            {
                string key = line.Substring(0, line.IndexOf(':')).Trim('#').Trim(':');

                switch (key.ToUpper())
                {
                    case "TITLE":
                        songData.title = line.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                        break;

                    case "SUBTITLE":
                        songData.subtitle = line.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                        break;

                    case "ARTIST":
                        songData.artist = line.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                        break;

                    case "BANNER":
                        songData.bannerPath = fileDir + line.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                        break;

                    case "BACKGROUND":
                        songData.backgroundPath = fileDir + line.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                        break;

                    case "MUSIC":
                        var musicName = line.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                        
                        // Check if file is mp3
                        if(musicName.ToLower().EndsWith(".mp3"))
                        {
                            // Convert to wav
                            var newName = musicName.ToLower().Replace("mp3", "wav");
                            bool succes = MP3Converter.Mp3ToWav(fileDir + musicName, fileDir + newName);

                            if (succes)
                            {
                                // Change the sm file
                                string text = File.ReadAllText(filePath);
                                text = text.Replace(musicName, newName);
                                File.WriteAllText(filePath, text);

                                // Delete the old mp3 file
                                File.Delete(fileDir + musicName);

                                // Apply new name
                                musicName = newName;

                                Debug.Log("Converted a mp3 to wav");
                            }
                        }

                        if (IsNullOrWhiteSpace(songData.musicPath) || !File.Exists(songData.musicPath))
                        {
                            // No music file found!
                            songData.musicPath = null;
                            songData.valid = false;
                        }

                        songData.musicPath = fileDir + musicName;
                        break;

                    case "OFFSET":
                        if (!float.TryParse(line.Substring(line.IndexOf(':')).Trim(':').Trim(';'), out songData.offset))
                        {
                            // Error Parsing
                            songData.offset = 0.0f;
                        }
                        break;

                    case "SAMPLESTART":
                        if (!float.TryParse(line.Substring(line.IndexOf(':')).Trim(':').Trim(';'), out songData.sampleStart))
                        {
                            // Error Parsing
                            songData.sampleStart = 0.0f;
                        }
                        break;

                    case "SAMPLELENGTH":
                        if (!float.TryParse(line.Substring(line.IndexOf(':')).Trim(':').Trim(';'), out songData.sampleLength))
                        {
                            // Error Parsing
                            songData.sampleLength = SAMPLE_LENGTH_DEFAULT;
                        }
                        break;

                    case "BPMS":
                        {
                            // The bpms can be listed under each other so we need to collect them all in one string
                            string allBpm = line;
                            int iterator = 1;

                            while(!fileData[i + iterator].StartsWith("#"))
                            {
                                allBpm += fileData[i + iterator];
                                iterator++;
                            }

                            var a = allBpm.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                            var b = a.Split(','); // split the line so we have all the bpms seperated

                            songData.bpms = new BPMS[b.Length]; // set the array to the amount of bpms(

                            for (int c = 0; c < songData.bpms.Length; c++)
                            {
                                var values = b[c].Split('='); // Seperate the bar and the bpm
                                var bpms = new BPMS();

                                // parse the values into bpms
                                float.TryParse(values[0], out bpms.bar);
                                float.TryParse(values[1], out bpms.bpm);

                                // Devide it by four because in stepmania every beat is one bar and here its a quarter of a bar
                                bpms.bar = bpms.bar / 4;

                                // give the values back to the metadata
                                songData.bpms[c] = bpms;
                            }
                        }
                        break;

                    case "NOTES":
                        inNotes = true;
                        break;
                }
            }

            if (inNotes)
            {
                // Skip dance-double
                if (line.ToLower().Contains("dance-double"))
                {
                    for (int j = i; j < fileData.Count; j++)
                    {
                        if (fileData[j].Contains(";"))
                        {
                            i = j - 1;
                            break;
                        }
                    }
                }

                // Check if it's a difficulty
                if (line.ToLower().Contains("beginner") ||
                    line.ToLower().Contains("easy") ||
                    line.ToLower().Contains("medium") ||
                    line.ToLower().Contains("hard") ||
                    line.ToLower().Contains("challenge") ||
                    line.ToLower().Contains("edit"))
                {
                    string difficulty = line.Trim().Trim(':').ToLower();

                    // Code to make sure that the string only contains the difficulty
                    if (difficulty.Contains("beginner"))
                        difficulty = "beginner";

                    if (difficulty.Contains("easy"))
                        difficulty = "easy";

                    if (difficulty.Contains("medium"))
                        difficulty = "medium";

                    if (difficulty.Contains("hard"))
                        difficulty = "hard";

                    if (difficulty.Contains("challenge"))
                        difficulty = "challenge";

                    if (difficulty.Contains("edit"))
                        difficulty = "edit";

                    List<string> noteChart = new List<string>();
                    for (int j = i; j < fileData.Count; j++)
                    {
                        string noteLine = fileData[j].Trim();
                        if (noteLine.EndsWith(";"))
                        {
                            // Add the semicolon to the chart so we know when it's finished
                            // so that we can add the last bar
                            noteChart.Add(noteLine[0].ToString());

                            i = j - 1;
                            break;
                        }
                        else if(noteLine.Length == 4)
                        {
                            noteChart.Add(noteLine);
                        }
                        else if(noteLine.StartsWith(","))
                        {
                            // Add the comma to the chart so we can seperate bars
                            noteChart.Add(noteLine[0].ToString());
                        }
                    }

                    if (difficulty == "edit")
                        difficulty = "insane";

                    Difficulties d = (Difficulties)Enum.Parse(typeof(Difficulties), difficulty);

                    songData.difficultyExists[d] = true;
                    songData.notedata[d] = ParseNotes(noteChart);
                }
                if (line.EndsWith(";"))
                {
                    inNotes = false;
                }
            }
        }

        return songData;
    }

    private static NoteData ParseNotes(List<string> notes)
    {
        NoteData barData = new NoteData();
        barData.bars = new List<Bar>();

        Bar bar = new Bar();
        bar.notes = new int[0, 4];

        var lineCount = 0;
        for (int i = 0; i < notes.Count; i++)
        {
            string line = notes[i].Trim();

            if (line.StartsWith(";")) // Chart seperator
            {
                // Add saved bar to the list
                barData.bars.Add(bar);

                // Exit the loop because the chart is finished
                break;
            }

            if (line.StartsWith(",")) // bar seperator
            {
                // Add saved bar to the list and make a new one
                barData.bars.Add(bar);
                bar = new Bar();
                bar.notes = new int[0,4];
                lineCount = 0;
            }
            else if (line.Length == 4) // a line that contains note data
            {
                ResizeArray(ref bar.notes, lineCount+1, 4);

                for (int c = 0; c < 4; c++) // c stands for character in the curernt line
                {
                    char note = line[c];

                    // Convert roll heads to normal heads
                    if (note == '4') { note = '2'; }

                    // Convert mines, lifts and fakes to empty space
                    if (note == 'M') { note = '0'; }
                    if (note == 'L') { note = '0'; }
                    if (note == 'F') { note = '0'; }

                    // add the note to the bar
                    bar.notes[lineCount, c] = (int)char.GetNumericValue(note);
                }

                lineCount++;
            }
        }

        return barData;
    }

    // Function to check if a string is empty or empty
    private static bool IsNullOrWhiteSpace(string value)
    {
        if (value != null)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Function for rezising 2d arrays
    private static void ResizeArray<T>(ref T[,] original, int newCoNum, int newRoNum)
    {
        var newArray = new T[newCoNum, newRoNum];
        int columnCount = original.GetLength(1);
        int columnCount2 = newRoNum;
        int columns = original.GetUpperBound(0);
        for (int co = 0; co <= columns; co++)
            Array.Copy(original, co * columnCount, newArray, co * columnCount2, columnCount);
        original = newArray;
    }
}