using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;

namespace StorybrewScripts
{
    public static class Utility
    {

        public static Vector2 PivotPoint(Vector2 point, Vector2 center, double radians)
        {
            // Translate point back to origin
            point -= center;

            // Rotate point
            Vector2 rotatedPoint = new Vector2(
                point.X * (float)Math.Cos(radians) - point.Y * (float)Math.Sin(radians),
                point.X * (float)Math.Sin(radians) + point.Y * (float)Math.Cos(radians)
            );

            // Translate point back
            return rotatedPoint + center;
        }

        // This method gives the value of the sine wave for a given time 't' with amplitude 'amplitude' and frequency 'frequency'.
        public static double SineWaveValue(double amplitude, double frequency, double t)
        {
            return amplitude * Math.Sin(2 * Math.PI * frequency * t);
        }

        public static double SineWaveValueWithPhase(double amplitude, double frequency, double time, double phase)
        {
            return amplitude * Math.Sin(2 * Math.PI * frequency * time + phase);
        }

        public static float CalculateDistance(Vector2 firstPoint, Vector2 secondPoint)
        {
            float dx = firstPoint.X - secondPoint.X;
            float dy = firstPoint.Y - secondPoint.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy) + 1f;
        }



    }
}