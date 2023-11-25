using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Animations;

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

        public static double CosWaveValue(double amplitude, double frequency, double t)
        {
            return amplitude * Math.Cos(2 * Math.PI * frequency * t);
        }

        public static double CosWaveValueWithPhase(double amplitude, double frequency, double t, double phase)
        {
            return amplitude * Math.Cos(2 * Math.PI * frequency * t + phase);
        }

        public static double TanValue(double amplitude, double frequency, double t)
        {
            return amplitude * Math.Tan(2 * Math.PI * frequency * t);
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

        public static float SmoothAmplitudeByTime(double currentTime, double starttime, double endtime, double startValue, double endValue, float defaultValue = 0)
        {

            float smoothedAmplitude = 0;

            // If within the first time range
            if (currentTime >= starttime && currentTime <= endtime)
            {
                double start = starttime;
                double end = endtime;  // Ending before the second segment starts

                // Calculate progress in the range of [0, 1]
                double progress = (currentTime - start) / (end - start);

                // Use a starting amplitude and an ending amplitude to calculate the current amplitude
                double startAmplitude = startValue;
                double endAmplitude = endValue;
                smoothedAmplitude = (float)(startAmplitude + progress * (endAmplitude - startAmplitude));
            }
            else
            {
                smoothedAmplitude = defaultValue;
            }

            return smoothedAmplitude;
        }

        public static float SmoothAmplitudeByProgress(float progress, float start, float end, double startValue, double endValue, float defaultValue = 0)
        {

            float smoothedAmplitude = 0f;

            // If within the first time range
            if (progress >= start && progress <= end)
            {
                double remappedProgress = (progress - start) / (end - start);

                // Use a starting amplitude and an ending amplitude to calculate the current amplitude
                double starScale = startValue;
                double endScale = endValue;
                smoothedAmplitude = (float)(starScale + remappedProgress * (endScale - starScale));
            }
            else
            {
                smoothedAmplitude = defaultValue;
            }

            return smoothedAmplitude;
        }

        public static void Log(string text)
        {
            // Define the directory path for the logs
            string logDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");

            // Ensure that the log directory exists
            Directory.CreateDirectory(logDirectoryPath);

            // Define the full path for the log file
            string logFilePath = Path.Combine(logDirectoryPath, "notosu.log");

            // Append the text to the log file
            File.AppendAllText(logFilePath, text + Environment.NewLine);

            // Optionally, display the text in the console
            Console.WriteLine(text);
        }

    }

}