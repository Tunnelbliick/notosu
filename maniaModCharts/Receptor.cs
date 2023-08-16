
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

namespace StorybrewScripts
{
    public class Receptor
    {

        public string receptorSpritePath = "";
        public Vector2 position = new Vector2(0, 0);
        public StoryboardLayer layer;
        public OsbSprite receptorSprite;

        public OsbSprite debug;

        // Rotation in radiants
        public double rotation = 0f;

        public Receptor(String receptorSpritePath, double rotation, StoryboardLayer layer, CommandScale scale, double starttime)
        {

            OsbSprite receptor = layer.CreateSprite(receptorSpritePath, OsbOrigin.Centre);
            receptor.Rotate(starttime, rotation);
            receptor.ScaleVec(starttime, scale);


            this.receptorSpritePath = receptorSpritePath;
            this.rotation = rotation;
            this.layer = layer;
            this.receptorSprite = receptor;

        }

        public Receptor(String receptorSpritePath, double rotation, StoryboardLayer layer, Vector2 position)
        {
            OsbSprite receptor = layer.CreateSprite(receptorSpritePath);
            receptor.Rotate(0, rotation);
            receptor.Move(0, position);

            this.receptorSpritePath = receptorSpritePath;
            this.rotation = rotation;
            this.layer = layer;
            this.receptorSprite = receptor;
            this.position = position;

        }

        public void MoveReceptor(double starttime, Vector2 newPosition, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.receptorSprite;

            receptor.Move(ease, starttime, starttime + duration, getCurrentPosition(starttime - 1), newPosition);

            this.position = newPosition;

        }

        public void ScaleReceptor(double starttime, Vector2 newScale, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.receptorSprite;

            receptor.ScaleVec(ease, starttime, starttime + duration, getCurrentScale(starttime), newScale);

        }

        public void RotateReceptor(double starttime, double rotation, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.receptorSprite;

            var newRotation = this.rotation + rotation;

            receptor.Rotate(ease, starttime, starttime + duration, this.rotation, newRotation);

            this.rotation = newRotation;

        }

        public void PivotReceptor(double starttime, double rotation, OsbEasing ease, double duration, int stepcount, Vector2 center)
        {

            //this.RotateReceptor(starttime, rotation, ease, duration);

            Vector2 point = this.position;

            double totalTime = starttime + duration; // Total duration in milliseconds
            double stepTime = duration / stepcount; // Step duration in milliseconds

            double endRadians = rotation; // Set the desired end radians here, 2*PI radians is a full circle
            double rotationPerIteration = endRadians / stepcount; // Rotation per iteration

            for (int i = 0; i <= stepcount; i++)
            {
                var currentTime = starttime + stepTime * i;

                Vector2 rotatedPoint = PivotPoint(point, center, rotationPerIteration * i);
                this.MoveReceptor(currentTime, rotatedPoint, ease, stepTime);
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

        public void Render(int starttime, int endTime)
        {
            OsbSprite receptor = this.receptorSprite;

            receptor.Fade(starttime, 1);
            receptor.Fade(endTime, 0);

        }

        public Vector2 getCurrentScale(double currentTime)
        {
            CommandScale scale = this.receptorSprite.ScaleAt(currentTime);
            return new Vector2(scale.X, scale.Y);
        }

        public Vector2 getCurrentPosition(double currentTime)
        {
            Vector2 position = this.receptorSprite.PositionAt(currentTime);

            return position;
        }

        public Vector2 getCurrentPositionForNotes(double currentTime)
        {
            Vector2 position = this.receptorSprite.PositionAt(currentTime);
            Vector2 currentScale = getCurrentScale(currentTime);

            return position;
        }

        public float getCurrentRotaion(double currentTIme)
        {
            return this.receptorSprite.RotationAt(currentTIme);
        }

    }
}