using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StorybrewScripts
{

    public enum EasingType
    {
        None,
        InSine,
        OutSine,
        InOutSine,
        OutQuad,
        InBounce,
        // ... Add other easing types as needed
    }

    public static class EasingFunctions
    {
        public static float None(float t)
        {
            return t;  // No change in progress value
        }

        // ... [Other easing functions as defined earlier]

        public static float ApplyEasing(EasingType type, float t)
        {
            switch (type)
            {
                case EasingType.None:
                    return None(t);
                case EasingType.InSine:
                    return InSine(t);
                case EasingType.OutSine:
                    return OutSine(t);
                case EasingType.InOutSine:
                    return InOutSine(t);
                case EasingType.OutQuad:
                    return OutQuad(t);
                case EasingType.InBounce:
                    return InBounce(t);
                // ... Add other easing types as needed
                default:
                    throw new ArgumentException("Unsupported easing type", nameof(type));
            }
        }

        public static float InSine(float t)
        {
            return (float)(1 - Math.Cos(t * Math.PI / 2));
        }

        public static float OutSine(float t)
        {
            return (float)Math.Sin(t * Math.PI / 2);
        }

        public static float InOutSine(float t)
        {
            return (float)(-(Math.Cos(Math.PI * t) - 1) / 2);
        }

        public static float OutQuad(float t)
        {
            return 1 - (1 - t) * (1 - t);
        }

        public static float InBounce(float t)
        {
            return 1 - OutBounce(1 - t);
        }

        public static float OutBounce(float t)
        {
            if (t < 1 / 2.75)
                return (float)(7.5625 * t * t);
            else if (t < 2 / 2.75)
                return (float)(7.5625 * (t -= 1.5f / 2.75f) * t + 0.75);
            else if (t < 2.5 / 2.75)
                return (float)(7.5625 * (t -= 2.25f / 2.75f) * t + 0.9375);
            else
                return (float)(7.5625 * (t -= 2.625f / 2.75f) * t + 0.984375);
        }
    }
}