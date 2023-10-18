using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding.CommandValues;

namespace StorybrewScripts
{

    public class NoteOrigin
    {

        public string receptorSpritePath = "";
        public Vector2 position = new Vector2(0, 0);
        public StoryboardLayer layer;
        public OsbSprite originSprite;

        public double bpmOffset;
        public double bpm;

        public OsbSprite debug;

        // Rotation in radiants
        public double rotation = 0f;

        public NoteOrigin(String receptorSpritePath, double rotation, StoryboardLayer layer, CommandScale scale, double starttime)
        {

            OsbSprite receptor = layer.CreateSprite("sb/transparent.png", OsbOrigin.Centre);
            receptor.Rotate(starttime, rotation);
            receptor.ScaleVec(starttime, scale);


            this.receptorSpritePath = receptorSpritePath;
            this.rotation = rotation;
            this.layer = layer;
            this.originSprite = receptor;

        }

        public void Render(double starttime, double endTime)
        {
            OsbSprite receptor = this.originSprite;

            receptor.Fade(starttime, 1);
            receptor.Fade(endTime, 0);

        }

        public void MoveOrigin(double starttime, Vector2 newPosition, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.originSprite;

            receptor.Move(ease, starttime, starttime + duration, getCurrentPosition(starttime - 1), newPosition);

            this.position = newPosition;

        }

        public void MoveOriginInstant(double starttime, Vector2 newPosition, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.originSprite;

            receptor.Move(starttime, newPosition);

            this.position = newPosition;

        }

        public void MoveOriginRelative(double starttime, Vector2 offset, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.originSprite;

            Vector2 originalPosition = getCurrentPosition(starttime);
            Vector2 newPosition = Vector2.Add(originalPosition, offset);

            receptor.Move(ease, starttime, starttime + duration, originalPosition, newPosition);

            this.position = newPosition;

        }

        public void MoveOriginRelativeX(double starttime, double value, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.originSprite;

            Vector2 originalPosition = getCurrentPosition(starttime);

            receptor.MoveX(ease, starttime, starttime + duration, originalPosition.X, originalPosition.X + value);

        }

        public void MoveOriginRelativeY(double starttime, double value, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.originSprite;

            Vector2 originalPosition = getCurrentPosition(starttime);

            receptor.MoveY(ease, starttime, starttime + duration, originalPosition.Y, originalPosition.Y + value);

        }

        public void ScaleReceptor(double starttime, Vector2 newPosition, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.originSprite;

            receptor.ScaleVec(ease, starttime, starttime + duration, getCurrentScale(starttime), newPosition);

        }

        public void RotateReceptor(double starttime, double rotation, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.originSprite;

            var newRotation = this.rotation + rotation;

            receptor.Rotate(ease, starttime, starttime + duration, this.rotation, newRotation);

            this.rotation = newRotation;

        }

        public void PivotReceptor(double starttime, double rotation, OsbEasing ease, double duration, int stepcount, Vector2 center)
        {

            this.RotateReceptor(starttime, rotation, ease, duration);

            Vector2 point = originSprite.PositionAt(starttime);

            double totalTime = starttime + duration; // Total duration in milliseconds
            double stepTime = duration / stepcount; // Step duration in milliseconds

            double endRadians = rotation; // Set the desired end radians here, 2*PI radians is a full circle
            double rotationPerIteration = endRadians / (stepcount - 1); // Rotation per iteration

            for (int i = 0; i < stepcount; i++)
            {
                var currentTime = starttime + stepTime * i;

                Vector2 rotatedPoint = PivotPoint(point, center, rotationPerIteration * i);
                this.MoveOrigin(currentTime, rotatedPoint, ease, stepTime);
            }
        }

        public void PivotAndRescaleReceptor(double starttime, double rotation, OsbEasing ease, double duration, int stepcount, Vector2 center, double targetDistance)
        {
            Vector2 initialPoint = originSprite.PositionAt(starttime);

            double stepTime = duration / stepcount;
            double rotationPerIteration = rotation / (stepcount - 1);

            // Calculate initial distance
            double initialDistance = (initialPoint - center).Length;

            for (int i = 0; i < stepcount; i++)
            {
                var currentTime = starttime + stepTime * i;

                // Rotate the point
                Vector2 rotatedPoint = Utility.PivotPoint(initialPoint, center, rotationPerIteration * i);

                // Get the direction in which we're moving (based on rotation around the center).
                Vector2 directionFromCenter = rotatedPoint - center;
                directionFromCenter.Normalize(); // Normalize to get a unit vector

                // Interpolate between initialDistance and targetDistance based on the progress
                double desiredDistance = initialDistance + (targetDistance - initialDistance) * ((double)i / stepcount);

                // Compute the new position based on the desired distance
                Vector2 newPoint = center + directionFromCenter * (float)desiredDistance;

                MoveOrigin(currentTime, newPoint, ease, stepTime);
            }
        }


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

        public Vector2 getCurrentScale(double currentTime)
        {
            CommandScale scale = this.originSprite.ScaleAt(currentTime);
            return new Vector2(scale.X, scale.Y);
        }

        public Vector2 getCurrentPosition(double currentTime)
        {
            CommandPosition position = this.originSprite.PositionAt(currentTime);
            return new Vector2(position.X, position.Y);
        }


    }
}