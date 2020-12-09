using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private GameplayManager m_gameplayManager;

    private float m_score;
    private float m_combo;
    private float m_comboHigh;
    private float m_accuracy;

    private int m_judgementAmount = 0;
    private int m_fullJudgement;

    private float m_largestScore = 500f; // The highest score for a note (without combo multiplier)

    private Dictionary<Judgements, int> m_judgementWeights;
    private Dictionary<Judgements, int> m_judgementHits;

    #region Events
    public delegate void ComboAdd();
    public delegate void ComboBreak();

    public event ComboAdd ComboAddEvent;
    public event ComboBreak ComboBreakEvent;
    #endregion

    void Start ()
    {
        m_gameplayManager = GameManager.Instance.GameplayManager;

        m_gameplayManager.JudgementEvent += OnJudgement;

        m_judgementWeights = new Dictionary<Judgements, int>()
        {
            { Judgements.marvelous, 100 },
            { Judgements.perfect, 90 },
            { Judgements.great, 70 },
            { Judgements.good, 30 },
            { Judgements.bad, 10 },
            { Judgements.miss, 0 }
        };

        m_judgementHits = new Dictionary<Judgements, int>()
        {
            { Judgements.marvelous, 0 },
            { Judgements.perfect, 0 },
            { Judgements.great, 0 },
            { Judgements.good, 0 },
            { Judgements.bad, 0 },
            { Judgements.miss, 0 }
        };

        m_score = 0;
    }
	
    public void ResetScore()
    {
        m_combo = 0;
        m_comboHigh = 0;
        m_score = 0;
        m_accuracy = 0;

        m_judgementAmount = 0;
        m_fullJudgement = 0;
    }

    public void SaveScores()
    {
        ScoreData scoreData = new ScoreData
        {
            Accuracy = m_accuracy,
            Combo = m_comboHigh,
            Rating = CalculateRating(m_accuracy),
            Score = (int)m_score,
            Time = System.DateTime.Now
        };

        HighscoreManager.AddScore(SongData.Song.title, SongData.Difficulty, scoreData);
    }

    private void OnJudgement(Judgements j)
    {
        switch (j)
        {
            case Judgements.marvelous:
                OnComboAdd();
                break;

            case Judgements.perfect:
                OnComboAdd();
                break;

            case Judgements.great:
                OnComboAdd();
                break;

            case Judgements.good:
                OnComboBreak();
                break;

            case Judgements.bad:
                OnComboBreak();
                break;

            case Judgements.miss:
                OnComboBreak();
                break;
        }

        var noteScore = m_largestScore / 100 * m_judgementWeights[j];
        m_score += noteScore * (1.0f + (Combo / 100.0f));

        m_judgementHits[j]++;

        // Calculate accuracy
        m_judgementAmount++;
        m_fullJudgement += m_judgementWeights[j];

        m_accuracy = ((float)m_fullJudgement / m_judgementAmount);
    }

    private void OnComboAdd()
    {
        m_combo++;

        // Update highest combo
        if (m_combo > m_comboHigh)
            m_comboHigh = m_combo;

        if (ComboAddEvent != null)
            ComboAddEvent();
    }

    private void OnComboBreak()
    {
        if (ComboBreakEvent != null)
            ComboBreakEvent();

        // Reset the combo after the event so that other functions can use the last combo
        m_combo = 0;
    }

    public static Ratings CalculateRating(float accuracy)
    {
        Ratings result;

        if(accuracy >= 100.0f)
        {
            result = Ratings.ss;
        }
        else if (accuracy >= 95.0f)
        {
            result = Ratings.s;
        }
        else if (accuracy >= 90.0f)
        {
            result = Ratings.a;
        }
        else if (accuracy >= 80.0f)
        {
            result = Ratings.b;
        }
        else if (accuracy >= 70.0f)
        {
            result = Ratings.c;
        }
        else
        {
            result = Ratings.d;
        }

        return result;
    }


    public float Combo
    {
        get { return m_combo; }
    }

    public float ComboHigh
    {
        get { return m_comboHigh; }
    }

    public float Score
    {
        get { return m_score; }
    }

    public float Accuracy
    {
        get { return m_accuracy; }
    }

    public Dictionary<Judgements, int> JudgementHits
    {
        get { return m_judgementHits; }
    }
}
