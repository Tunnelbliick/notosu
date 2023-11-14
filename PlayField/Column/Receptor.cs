
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StorybrewCommon.Mapset;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding.Util;
using StorybrewCommon.Subtitles;
using StorybrewCommon.Util;
using OpenTK;
using StorybrewCommon.Storyboarding.CommandValues;
using storyboard.scriptslibrary.maniaModCharts.utility;
using StorybrewCommon.Animations;
using System.IO;
using OpenTK.Graphics;

namespace StorybrewScripts
{
    public class Receptor
    {

        public string receptorSpritePath = "";
        public Vector2 position = new Vector2(0, 0);
        public StoryboardLayer layer;
        public OsbSprite renderedSprite;
        public OsbSprite debug;
        public string appliedTransformation = "";

        public SortedDictionary<double, float> positionX = new SortedDictionary<double, float>();
        public SortedDictionary<double, float> positionY = new SortedDictionary<double, float>();

        // Rotation in radiants
        public double rotation = 0f;
        public double startRotation = 0f;
        public ColumnType columnType;
        public double bpmOffset;
        public double bpm;
        private double deltaIncrement = 1;

        public Receptor(String receptorSpritePath, double rotation, StoryboardLayer layer, CommandScale scale, double starttime, ColumnType type, double delta)
        {

            this.deltaIncrement = delta;

            OsbSprite receptorSprite = layer.CreateSprite(receptorSpritePath, OsbOrigin.Centre);

            positionX.Add(0, 0);
            positionY.Add(0, 0);

            switch (type)
            {
                case ColumnType.one:
                    receptorSprite.Rotate(starttime - 1, 1 * Math.PI / 2);
                    break;
                case ColumnType.two:
                    receptorSprite.Rotate(starttime - 1, 0 * Math.PI / 2);
                    break;
                case ColumnType.three:
                    receptorSprite.Rotate(starttime - 1, 2 * Math.PI / 2);
                    break;
                case ColumnType.four:
                    receptorSprite.Rotate(starttime - 1, 3 * Math.PI / 2);
                    break;
            }

            receptorSprite.ScaleVec(starttime, scale);

            this.columnType = type;
            this.receptorSpritePath = receptorSpritePath;
            this.renderedSprite = receptorSprite;
            this.rotation = rotation;
            this.startRotation = rotation;
            this.layer = layer;

        }

        public Receptor(string receptorSpritePath, double rotation, StoryboardLayer layer, Vector2 position, ColumnType type, double delta)
        {
            OsbSprite receptor = layer.CreateSprite("sb/transparent.png", OsbOrigin.Centre);
            OsbSprite receptorSprite = layer.CreateSprite(receptorSpritePath, OsbOrigin.Centre);

            this.deltaIncrement = delta;

            positionX.Add(0, 0);
            positionY.Add(0, 0);

            switch (type)
            {
                case ColumnType.one:
                    receptor.Rotate(0 - 1, 1 * Math.PI / 2);
                    break;
                case ColumnType.two:
                    receptor.Rotate(0 - 1, 0 * Math.PI / 2);
                    break;
                case ColumnType.three:
                    receptor.Rotate(0 - 1, 2 * Math.PI / 2);
                    break;
                case ColumnType.four:
                    receptor.Rotate(0 - 1, 3 * Math.PI / 2);
                    break;
            }

            this.columnType = type;
            this.receptorSpritePath = receptorSpritePath;
            this.renderedSprite = receptorSprite;
            this.rotation = rotation;
            this.layer = layer;
            this.position = position;

        }

        // Absolute Movements overwrite any and all relative movements that might have existed before them hence why they are absolute!
        public void MoveReceptorAbsolute(double starttime, Vector2 endPos)
        {

            AddXValue(starttime, endPos.X, endPos.Y, true);
            AddYValue(starttime, endPos.Y, endPos.Y, true);


        }

        // Absolute Movements overwrite any and all relative movements that might have existed before them hence why they are absolute!
        public void MoveReceptorAbsolute(OsbEasing ease, double starttime, double endtime, Vector2 startPos, Vector2 endPos)
        {

            if (starttime == endtime)
            {
                AddXValue(starttime, endPos.X, endPos.X, true);
                AddYValue(starttime, endPos.Y, endPos.Y, true);
                return;
            }

            easeProgressAbsolute(ease, starttime, endtime, startPos, endPos);

        }

        public void MoveReceptorRelative(OsbEasing ease, double starttime, double endtime, Vector2 offset)
        {

            if (starttime == endtime)
            {
                AddXValue(starttime, offset.X, offset.X);
                AddYValue(starttime, offset.Y, offset.Y);
                return;
            }

            easeProgressRelative(ease, starttime, endtime, offset);

        }

        public void MoveReceptorRelative(OsbEasing ease, double starttime, double endtime, Vector2 offset, Vector2 absolute)
        {

            if (starttime == endtime)
            {
                AddXValue(starttime, offset.X, absolute.X);
                AddYValue(starttime, offset.Y, absolute.Y);
                return;
            }

            easeProgressRelative(ease, starttime, endtime, offset);

        }

        public void MoveReceptorRelativeX(OsbEasing ease, double starttime, double endtime, float value)
        {

            if (starttime == endtime)
            {
                AddXValue(starttime, value, value);
                return;
            }

            easeProgressRelative(ease, starttime, endtime, new Vector2(value, 0));

        }

        public void MoveReceptorRelativeY(OsbEasing ease, double starttime, double endtime, float value)
        {
            if (starttime == endtime)
            {
                AddYValue(starttime, value, value);
                return;
            }

            easeProgressRelative(ease, starttime, endtime, new Vector2(0, value));

        }

        public void ScaleReceptor(OsbEasing ease, double starttime, double endtime, Vector2 newScale)
        {
            OsbSprite receptor = this.renderedSprite;

            Vector2 originalScale = ScaleAt(starttime);

            if (starttime == endtime)
            {
                receptor.ScaleVec(starttime, newScale);
            }
            else
            {
                receptor.ScaleVec(ease, starttime, endtime, originalScale, newScale);
            }
        }

        public void RotateReceptorAbsolute(OsbEasing ease, double starttime, double endtime, double rotation)
        {
            OsbSprite receptor = this.renderedSprite;


            if (starttime == endtime)
            {
                receptor.Rotate(starttime, rotation);
            }
            else
            {
                receptor.Rotate(ease, starttime, endtime, RotationAt(starttime), rotation);
            }

            this.rotation = rotation;

        }

        public void RotateReceptor(OsbEasing ease, double starttime, double endtime, double rotation)
        {
            OsbSprite receptor = this.renderedSprite;
            var currentRot = RotationAt(starttime);
            var newRotation = currentRot + rotation;

            if (starttime == endtime)
            {
                receptor.Rotate(starttime, newRotation);
            }
            else
            {
                receptor.Rotate(ease, starttime, endtime, currentRot, newRotation);
            }

            this.rotation = newRotation;

        }

        public void PivotReceptor(OsbEasing ease, double starttime, double endtime, double rotation, Vector2 center)
        {
            Vector2 point = PositionAt(starttime);

            double duration = Math.Max(endtime - starttime, 1);
            double endRadians = rotation; // Total rotation in radians

            Vector2 currentPosition = point;
            double currentTime = starttime;

            while (currentTime <= endtime)
            {
                currentTime += deltaIncrement;
                double progress = Math.Max(currentTime - starttime, 1) / duration; // Calculate progress as a ratio

                // Adjust the rotation based on progress and easing
                double easedProgress = ease.Ease(progress); // Assuming ease.Ease() applies the easing to the progress
                double currentRotation = endRadians * easedProgress; // Total rotation adjusted by eased progress

                Vector2 rotatedPoint = Utility.PivotPoint(point, center, currentRotation);

                Vector2 relativeMovement = rotatedPoint - currentPosition;
                Vector2 absoluteMovement = rotatedPoint - point;

                MoveReceptorRelative(ease, currentTime, currentTime, relativeMovement, absoluteMovement);

                currentPosition = rotatedPoint;
            }
        }


        public Vector2 ScaleAt(double currentTime)
        {
            return renderedSprite.ScaleAt(currentTime);
        }

        public float RotationAt(double currentTIme)
        {
            return this.renderedSprite.RotationAt(currentTIme);
        }

        public void Render(double starttime, double endtime)
        {

            if (this.appliedTransformation != "")
            {
                return;
            }

            OsbSprite sprite = this.renderedSprite;

            switch (this.columnType)
            {
                case ColumnType.one:
                    sprite.Rotate(starttime - 1, 1 * Math.PI / 2);
                    break;
                case ColumnType.two:
                    sprite.Rotate(starttime - 1, 0 * Math.PI / 2);
                    break;
                case ColumnType.three:
                    sprite.Rotate(starttime - 1, 2 * Math.PI / 2);
                    break;
                case ColumnType.four:
                    sprite.Rotate(starttime - 1, 3 * Math.PI / 2);
                    break;
            }

            double currentTime = starttime; // Initialize currentTime
            double beatDuration = 60000.0 / bpm; // Calculate beat duration
            double halfDuration = beatDuration / 2;

            // Calculate the new adjusted currentTime with the offset
            double adjustedTime = Math.Ceiling((currentTime - bpmOffset) / beatDuration) * beatDuration + bpmOffset;

            sprite.Color(starttime, new Color4(97, 97, 97, 0));

            while (adjustedTime < endtime)
            {
                sprite.Color(OsbEasing.OutCirc, adjustedTime, adjustedTime + halfDuration, new Color4(255, 255, 255, 255), new Color4(97, 97, 97, 0));
                sprite.Color(OsbEasing.InCirc, adjustedTime + halfDuration, adjustedTime + beatDuration, new Color4(97, 97, 97, 0), new Color4(255, 255, 255, 255));

                adjustedTime += beatDuration;
            }

        }

        public void RenderTransformed(double starttime, double endtime, string reference)
        {

            if (this.appliedTransformation == reference)
            {
                return;
            }

            OsbSprite oldSprite = this.renderedSprite;
            this.appliedTransformation = reference;
            oldSprite.Fade(starttime, 0);
            OsbSprite sprite = layer.CreateSprite(Path.Combine("sb", "transformation", reference, this.columnType.ToString(), "receptor", "receptor" + ".png"), OsbOrigin.Centre, PositionAt(starttime));

            sprite.Rotate(starttime, 0);
            sprite.ScaleVec(starttime, renderedSprite.ScaleAt(starttime));
            sprite.Fade(starttime, 1);
            sprite.Fade(endtime, 0);

            this.renderedSprite = sprite;

            // oldSprite = null;
        }

        public Color4 LerpColor(Color4 colorA, Color4 colorB, double t)
        {
            byte r = (byte)(colorA.R + t * (colorB.R - colorA.R));
            byte g = (byte)(colorA.G + t * (colorB.G - colorA.G));
            byte b = (byte)(colorA.B + t * (colorB.B - colorA.B));
            byte a = (byte)(colorA.A + t * (colorB.A - colorA.A));

            return new Color4(r, g, b, a);
        }

        private void AddXValue(double time, float value, float progressed, bool absolute = false)
        {

            // Ensure time is a multiple of deltaTime
            if (time % deltaIncrement != 0)
            {
                // Handle the case where time is not a multiple of deltaTime
                // Option 1: Adjust time to the nearest multiple of deltaTime
                time = Math.Ceiling(time / deltaIncrement) * deltaIncrement;

                // Option 2: Throw an exception
                // throw new ArgumentException("Time must be a multiple of deltaTime.");
            }

            if (positionX.ContainsKey(time))
            {

                if (absolute)
                    positionX[time] = value;
                else
                {
                    positionX[time] += progressed;
                }

            }
            else
            {

                float lastValue = getLastX(time);

                if (absolute)
                    positionX.Add(time, value);
                else
                    positionX.Add(time, lastValue + value);

            }
        }



        private void AddYValue(double time, float value, float progressed, bool absolute = false)
        {

            // Ensure time is a multiple of deltaTime
            if (time % deltaIncrement != 0)
            {
                // Handle the case where time is not a multiple of deltaTime
                // Option 1: Adjust time to the nearest multiple of deltaTime
                time = Math.Ceiling(time / deltaIncrement) * deltaIncrement;

                // Option 2: Throw an exception
                // throw new ArgumentException("Time must be a multiple of deltaTime.");
            }

            if (positionY.ContainsKey(time))
            {

                if (absolute)
                    positionY[time] = value;
                else
                {
                    positionY[time] += progressed;
                }

            }
            else
            {

                float lastValue = getLastY(time);

                if (absolute)
                    positionY.Add(time, value);
                else
                    positionY.Add(time, lastValue + value);

            }

        }

        private float getLastX(double currentTime)
        {
            if (positionX.Count == 0)
            {
                return 0; // Or your default value
            }

            var keys = positionX.Keys.ToList();
            int left = 0;
            int right = keys.Count - 1;
            double lastKey = -1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (keys[mid] < currentTime)
                {
                    lastKey = keys[mid];
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return lastKey != -1 ? positionX[lastKey] : 0;
        }


        private float getLastY(double currentTime)
        {
            if (positionY.Count == 0)
            {
                return 0; // Or your default value
            }

            var keys = positionY.Keys.ToList();
            int left = 0;
            int right = keys.Count - 1;
            double lastKey = -1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (keys[mid] < currentTime)
                {
                    lastKey = keys[mid];
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return lastKey != -1 ? positionY[lastKey] : 0;
        }

        private void easeProgressAbsolute(OsbEasing ease, double start, double end, Vector2 startPos, Vector2 endPos)
        {

            double duration = Math.Max(end - start, 0); // Ensure non-negative duration
            double deltaTime = 0;
            Vector2 lastPos = startPos; // Keep track of the last position to calculate the delta

            double progress = 0;
            do
            {
                deltaTime += deltaIncrement; // Increment time by deltaIncrement
                progress = deltaTime / duration; // Normalized time [0, 1]
                progress = Math.Min(progress, 1);       // Clamp progress to 1 to avoid overshooting

                float t = (float)ease.Ease(progress);   // Apply easing function

                Vector2 newPos = Vector2.Lerp(startPos, endPos, t); // Interpolated position
                Vector2 movement = newPos - lastPos;               // Delta movement

                // Apply the delta movement
                AddXValue(start + deltaTime, movement.X, newPos.X);
                AddYValue(start + deltaTime, movement.Y, newPos.Y);


                lastPos = newPos;   // Update lastPos for the next iteration
            } while (progress < 1);

        }

        private void easeProgressRelative(OsbEasing ease, double start, double end, Vector2 offset)
        {
            Vector2 startPos = new Vector2(0, 0); // Assuming starting at origin; replace with actual start if different
            Vector2 endPos = startPos + offset;   // The final desired position

            double duration = Math.Max(end - start + 1, 0); // Ensure non-negative duration
            double deltaTime = 0;
            Vector2 lastPos = startPos; // Keep track of the last position to calculate the delta

            double progress = 0;
            do
            {
                deltaTime += deltaIncrement; // Increment time by deltaIncrement
                progress = deltaTime / duration; // Normalized time [0, 1]
                progress = Math.Min(progress, 1);       // Clamp progress to 1 to avoid overshooting

                float t = (float)ease.Ease(progress);   // Apply easing function

                Vector2 newPos = Vector2.Lerp(startPos, endPos, t); // Interpolated position
                Vector2 movement = newPos - lastPos;               // Delta movement

                // Apply the delta movement
                if (offset.X != 0)
                    AddXValue(start + deltaTime, movement.X, newPos.X);
                if (offset.Y != 0)
                    AddYValue(start + deltaTime, movement.Y, newPos.Y);


                lastPos = newPos;   // Update lastPos for the next iteration
            } while (progress < 1);

        }


        public Vector2 PositionAt(double time)
        {
            return new Vector2(getLastX(time), getLastY(time));
        }


    }
}