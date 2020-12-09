// Simple easing for c#
// For infomation about the easing types: http://easings.net
// (ctrl + M + L)

using UnityEngine;

namespace SimpleEasing
{
    public static class Easing
    {
        public static float easeInBack(float time, float start, float change, float duration)
        {
            float s = 1.70158f; // feel free to modify this value

            time /= duration;
            return change * (time) * time * ((s + 1) * time - s) + start;
        }
        public static float easeInBounce(float time, float start, float change, float duration)
        {
            return change - easeOutBounce(duration - time, 0, change, duration) + start;
        }
        public static float easeInCirc(float time, float start, float change, float duration)
        {
            time /= duration;
            return change * (1 - Mathf.Sqrt(1 - time * time)) + start;
        }
        public static float easeInCubic(float time, float start, float change, float duration)
        {
            return change * Mathf.Pow(time / duration, 3) + start;
        }
        public static float easeInElastic(float time, float start, float change, float duration)
        {
            float s = 1.70158f;
            float p = 0;
            float a = change;

            if (time == 0 || a == 0)
            {
                return start;
            }

            time /= duration;

            if (time == 1)
            {
                return start + change;
            }

            if (p == 0)
            {
                p = duration * 0.3f;
            }

            if (a < Mathf.Abs(change))
            {
                a = change;
                s = p * 0.25f;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(change / a);
            }

            return -(a * Mathf.Pow(2, 10 * (--time)) * Mathf.Sin((time * duration - s) * (2 * Mathf.PI) / p)) + start;

        }
        public static float easeInExpo(float time, float start, float change, float duration)
        {
            return change * Mathf.Pow(2, 10 * (time / duration - 1)) + start;
        }
        public static float easeInQuad(float time, float start, float change, float duration)
        {
            time /= duration;
            return change * time * time + start;
        }
        public static float easeInQuart(float time, float start, float change, float duration)
        {
            return change * Mathf.Pow(time / duration, 4) + start;
        }
        public static float easeInQuint(float time, float start, float change, float duration)
        {
            return change * Mathf.Pow(time / duration, 5) + start;
        }
        public static float easeInSine(float time, float start, float change, float duration)
        {
            return change * (1 - Mathf.Cos(time / duration * (Mathf.PI / 2))) + start;
        }
        public static float easeInOutBack(float time, float start, float change, float duration)
        {
            float s = 1.70158f;

            time /= duration;
            time *= 2;

            if ((time) < 1)
            {
                s *= (1.525f);
                return change * 0.5f * (time * time * ((s + 1) * time - s)) + start;
            }

            time -= 2;
            s *= 1.525f;
            return change * 0.5f * ((time) * time * ((s + 1) * time + s) + 2) + start;
        }
        public static float easeInOutBounce(float time, float start, float change, float duration)
        {
            if (time < duration * 0.5f)
            {
                return (easeInBounce(time * 2, 0, change, duration) * 0.5f + start);
            }

            return (easeOutBounce(time * 2 - duration, 0, change, duration) * 0.5f + change * 0.5f + start);
        }
        public static float easeInOutCirc(float time, float start, float change, float duration)
        {
            time /= duration * 0.5f;

            if (time < 1)
            {
                return change * 0.5f * (1 - Mathf.Sqrt(1 - time * time)) + start;
            }

            time -= 2;
            return change * 0.5f * (Mathf.Sqrt(1 - time * time) + 1) + start;
        }
        public static float easeInOutCubic(float time, float start, float change, float duration)
        {
            time /= duration * 0.5f;

            if (time < 1)
            {
                return (change * 0.5f) * Mathf.Pow(time, 3) + start;
            }

            return (change * 0.5f) * (Mathf.Pow(time - 2, 3) + 2) + start;
        }
        public static float easeInOutElastic(float time, float start, float change, float duration)
        {
            float s = 1.70158f;
            float p = 0;
            float a = change;
            if (time == 0 || a == 0)
            {
                return start;
            }
            time /= (duration * 0.5f);
            if (time == 2)
            {
                return start + change;
            }
            if (p < 0.5f)
            {
                p = duration * (0.3f * 1.5f);
            }
            if (a < Mathf.Abs(change))
            {
                a = change;
                s = p * 0.25f;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(change / a);
            }
            if (time < 1)
            {
                return -0.5f * (a * Mathf.Pow(2, 10 * (--time)) * Mathf.Sin((time * duration - s) * (2 * Mathf.PI) / p)) + start;
            }
            return a * Mathf.Pow(2, -10 * (--time)) * Mathf.Sin((time * duration - s) * (2 * Mathf.PI) / p) * 0.5f + change + start;
        }
        public static float easeInOutExpo(float time, float start, float change, float duration)
        {
            time /= duration * 0.5f;

            if (time < 1)
            {
                return change * 0.5f * Mathf.Pow(2, 10 * (time - 1)) + start;
            }

            time -= 1;
            return change * 0.5f * (-Mathf.Pow(2, -10 * time) + 2) + start;

        }
        public static float easeInOutQuad(float time, float start, float change, float duration)
        {
            time /= (duration * 0.5f);

            if (time < 1)
            {
                return (change * 0.5f) * (time * time) + start;
            }

            return (-change * 0.5f) * (--time * (time - 2) - 1) + start;
        }
        public static float easeInOutQuart(float time, float start, float change, float duration)
        {
            time /= duration * 0.5f;

            if (time < 1)
            {
                return change * 0.5f * Mathf.Pow(time, 4) + start;
            }

            return -change * 0.5f * (Mathf.Pow(time - 2, 4) - 2) + start;

        }
        public static float easeInOutQuint(float time, float start, float change, float duration)
        {
            time /= duration * 0.5f;

            if (time < 1)
            {
                return change * 0.5f * Mathf.Pow(time, 5) + start;
            }

            return change * 0.5f * (Mathf.Pow(time - 2, 5) + 2) + start;

        }
        public static float easeInOutSine(float time, float start, float change, float duration)
        {
            return change * 0.5f * (1 - Mathf.Cos(Mathf.PI * time / duration)) + start;
        }
        public static float easeLiniear(float time, float start, float change, float duration)
        {
            return change * time / duration + start;
        }
        public static float easeOutBack(float time, float start, float change, float duration)
        {
            float s = 1.70158f;

            time = (time / duration) - 1;
            return change * ((time) * time * ((s + 1) * time + s) + 1) + start;

        }
        public static float easeOutBounce(float time, float start, float change, float duration)
        {
            time /= duration;

            if (time < (1 / 2.75f))
            {
                return change * (7.5625f * time * time) + start;
            }
            else
            if (time < (2 / 2.75f))
            {
                time -= (1.5f / 2.75f);
                return change * (7.5625f * time * time + 0.75f) + start;
            }
            else
            if (time < (2.5f / 2.75f))
            {
                time -= (2.25f / 2.75f);
                return change * (7.5625f * time * time + 0.9375f) + start;
            }
            else
            {
                time -= (2.625f / 2.75f);
                return change * (7.5625f * time * time + 0.984375f) + start;
            }

        }
        public static float easeOutCirc(float time, float start, float change, float duration)
        {
            time = time / duration - 1;
            return change * Mathf.Sqrt(1 - time * time) + start;
        }
        public static float easeOutCubic(float time, float start, float change, float duration)
        {
            return change * (Mathf.Pow(time / duration - 1, 3) + 1) + start;
        }
        public static float easeOutElastic(float time, float start, float change, float duration)
        {
            float s = 1.70158f;
            float p = 0;
            float time2 = time;
            float start2 = start;
            float change2 = change;
            float duration2 = duration;

            if (time2 == 0 || change2 == 0)
        {
                return start2;
            }
            time2 /= duration2;
            if (time2 == 1)
            {
                return start2 + change;
            }
            if (p < 0.5f)
            {
                p = duration2 * 0.3f;
            }
            if (change2 < Mathf.Abs(change))
            {
                change2 = change;
                s = p * 0.25f;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(change / change2);
            }
            return change2 * Mathf.Pow(2, -10 * time2) * Mathf.Sin((time2 * duration2 - s) * (2 * Mathf.PI) / p) + change + start2;
        }
        public static float easeOutExpo(float time, float start, float change, float duration)
        {
            return change * (-Mathf.Pow(2, -10 * time / duration) + 1) + start;
        }
        public static float easeOutQuad(float time, float start, float change, float duration)
        {
            time /= duration;
            return -change * time * (time - 2) + start;
        }
        public static float easeOutQuart(float time, float start, float change, float duration)
        {
            return -change * (Mathf.Pow(time / duration - 1, 4) - 1) + start;
        }
        public static float easeOutQuint(float time, float start, float change, float duration)
        {
            return change * (Mathf.Pow(time / duration - 1, 5) + 1) + start;
        }
        public static float easeOutSine(float time, float start, float change, float duration)
        {
            return change * Mathf.Sin(time / duration * (Mathf.PI / 2)) + start;
        }
    }
}