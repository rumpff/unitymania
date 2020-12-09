using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JudgementValues
{
    private static List<Dictionary<Judgements, float>> m_judgementTimes;

    public static Dictionary<Judgements, float> GetJugementTimes(int index)
    {
        if (m_judgementTimes == null)
            GenerateTimes();

        return m_judgementTimes[index];
    }

    private static void GenerateTimes()
    {
        m_judgementTimes = new List<Dictionary<Judgements, float>>()
        {
            /// Stepmania 1
            { new Dictionary<Judgements, float>()
            {
                { Judgements.marvelous, 0.033f },
                { Judgements.perfect, 0.068f },
                { Judgements.great, 0.135f },
                { Judgements.good, 0.203f },
                { Judgements.bad, 0.270f },
                { Judgements.miss, 0.33f }
            } },

            /// Stepmania 2
            { new Dictionary<Judgements, float>()
            {
                { Judgements.marvelous, 0.029f },
                { Judgements.perfect, 0.060f },
                { Judgements.great, 0.0120f },
                { Judgements.good, 0.180f },
                { Judgements.bad, 0.239f },
                { Judgements.miss, 0.29f }
            } },

             /// Stepmania 3
            { new Dictionary<Judgements, float>()
            {
                { Judgements.marvelous, 0.026f },
                { Judgements.perfect, 0.052f },
                { Judgements.great, 0.104f },
                { Judgements.good, 0.157f },
                { Judgements.bad, 0.209f },
                { Judgements.miss, 0.26f }
            } },

            /// Stepmania 4
            { new Dictionary<Judgements, float>()
            {
                { Judgements.marvelous, 0.022f },
                { Judgements.perfect, 0.045f },
                { Judgements.great, 0.09f },
                { Judgements.good, 0.135f },
                { Judgements.bad, 0.180f },
                { Judgements.miss, 0.22f }
            } },

            /// Stepmania 5
            { new Dictionary<Judgements, float>()
            {
                { Judgements.marvelous, 0.018f },
                { Judgements.perfect, 0.038f },
                { Judgements.great, 0.076f },
                { Judgements.good, 0.113f },
                { Judgements.bad, 0.151f },
                { Judgements.miss, 0.18f }
            } },

            /// Stepmania 6
            { new Dictionary<Judgements, float>()
            {
                { Judgements.marvelous, 0.015f },
                { Judgements.perfect, 0.03f },
                { Judgements.great, 0.059f },
                { Judgements.good, 0.89f },
                { Judgements.bad, 0.119f },
                { Judgements.miss, 0.15f }
            } },

            /// Stepmania 7
            { new Dictionary<Judgements, float>()
            {
                { Judgements.marvelous, 0.011f },
                { Judgements.perfect, 0.023f },
                { Judgements.great, 0.045f },
                { Judgements.good, 0.068f },
                { Judgements.bad, 0.09f },
                { Judgements.miss, 0.11f }
            } },

            /// Stepmania 8
            { new Dictionary<Judgements, float>()
            {
                { Judgements.marvelous, 0.007f },
                { Judgements.perfect, 0.015f },
                { Judgements.great, 0.030f },
                { Judgements.good, 0.045f },
                { Judgements.bad, 0.059f },
                { Judgements.miss, 0.07f }
            } },

            /// Stepmania JUSTICE
            { new Dictionary<Judgements, float>()
            {
                { Judgements.marvelous, 0.004f },
                { Judgements.perfect, 0.009f },
                { Judgements.great, 0.018f },
                { Judgements.good, 0.027f },
                { Judgements.bad, 0.036f },
                { Judgements.miss, 0.04f }
            } },
        };
    }
}
