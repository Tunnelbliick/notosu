
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

namespace StorybrewScripts
{
    public class Receptor
    {

        public string receptorSpritePath = "";
        public Vector2 position = new Vector2(0, 0);
        public StoryboardLayer layer;
        public OsbSprite receptorSprite;
        public OsbSprite renderedSprite;
        public Operation currentOperation;
        public OsbSprite debug;
        public string appliedTransformation = "";
        public Dictionary<Guid, Operation> operationLog = new Dictionary<Guid, Operation>();

        // Rotation in radiants
        public double rotation = 0f;
        public double startRotation = 0f;
        public ColumnType columnType;

        public Receptor(String receptorSpritePath, double rotation, StoryboardLayer layer, CommandScale scale, double starttime, ColumnType type)
        {

            OsbSprite receptor = layer.CreateSprite("sb/transparent.png", OsbOrigin.Centre);
            OsbSprite receptorSprite = layer.CreateSprite(receptorSpritePath, OsbOrigin.Centre);

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
            this.receptorSprite = receptor;

        }

        public Receptor(String receptorSpritePath, double rotation, StoryboardLayer layer, Vector2 position, ColumnType type)
        {
            OsbSprite receptor = layer.CreateSprite("sb/transparent.png", OsbOrigin.Centre);
            OsbSprite receptorSprite = layer.CreateSprite(receptorSpritePath, OsbOrigin.Centre);

            switch (type)
            {
                case ColumnType.one:
                    receptorSprite.Rotate(0 - 1, 1 * Math.PI / 2);
                    break;
                case ColumnType.two:
                    receptorSprite.Rotate(0 - 1, 0 * Math.PI / 2);
                    break;
                case ColumnType.three:
                    receptorSprite.Rotate(0 - 1, 2 * Math.PI / 2);
                    break;
                case ColumnType.four:
                    receptorSprite.Rotate(0 - 1, 3 * Math.PI / 2);
                    break;
            }

            this.columnType = type;
            this.receptorSpritePath = receptorSpritePath;
            this.renderedSprite = receptorSprite;
            this.rotation = rotation;
            this.layer = layer;
            this.receptorSprite = receptor;
            this.position = position;

        }

        public void MoveReceptor(double starttime, Vector2 newPosition, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.receptorSprite;


            Vector2 originalPostion = getCurrentPosition(starttime - 1);

            receptor.Move(ease, starttime, starttime + duration, originalPostion, newPosition);

            this.position = newPosition;

        }

        public void ScaleReceptor(double starttime, Vector2 newScale, OsbEasing ease, double duration)
        {
            OsbSprite receptor = this.receptorSprite;

            Vector2 originalScale = getCurrentScale(starttime);

            receptor.ScaleVec(ease, starttime, starttime + duration, originalScale, newScale);

        }

        public void RotateReceptorAbsolute(double starttime, double duration, OsbEasing ease, double rotation)
        {
            OsbSprite receptor = this.receptorSprite;

            receptor.Rotate(ease, starttime, starttime + duration, getCurrentRotaion(starttime), rotation);

            this.rotation = rotation;

        }

        public void RotateReceptor(double starttime, double duration, OsbEasing ease, double rotation)
        {
            OsbSprite receptor = this.receptorSprite;

            var newRotation = getCurrentRotaion(starttime) + rotation;

            receptor.Rotate(ease, starttime, starttime + duration, getCurrentRotaion(starttime), newRotation);

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

                Vector2 rotatedPoint = Utility.PivotPoint(point, center, rotationPerIteration * i);
                this.MoveReceptor(currentTime, rotatedPoint, ease, stepTime);
            }
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


            sprite.Fade(starttime, 1);
            sprite.ScaleVec(starttime, receptorSprite.ScaleAt(starttime));
            sprite.Fade(endtime, 0);

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
            OsbSprite sprite = layer.CreateSprite(Path.Combine("sb", "transformation", reference, this.columnType.ToString(), "receptor", "receptor" + ".png"), OsbOrigin.Centre, receptorSprite.PositionAt(starttime));

            sprite.Rotate(starttime, 0);
            sprite.ScaleVec(starttime, receptorSprite.ScaleAt(starttime));
            sprite.Fade(starttime, 1);
            sprite.Fade(endtime, 0);

            this.renderedSprite = sprite;

           // oldSprite = null;
        }
    }
}