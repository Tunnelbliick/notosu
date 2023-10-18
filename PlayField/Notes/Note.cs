
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Mapset;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{

    public class Note
    {
        public int noteType = 0;
        public StoryboardLayer layer;
        public OsbSprite noteSprite;
        public OsuHitObject hitObject;
        public double renderStart = 0;
        public double renderEnd = 0;
        public double renderDuration = 0;
        public bool isSlider = false;
        public double starttime;
        public double endtime;
        public ColumnType columnType;
        public List<SliderParts> sliderPositions = new List<SliderParts>();
        public int sliderParts = 1;
        public string appliedTransformation = "";

        static Random _random = new Random();

        public Note(StoryboardLayer layer, OsuHitObject hitObject, Column column, double bpm, double offset, double msPerPart = 40)
        {

            this.layer = layer;
            this.hitObject = hitObject;
            this.starttime = hitObject.StartTime;
            this.endtime = hitObject.EndTime;

            // calculate duration of one beat
            double beatDuration = 60000f / bpm;

            if (this.starttime != this.endtime)
            {

                this.isSlider = true;
                var sliderDuration = this.endtime - this.starttime;

                for (double ellapsedTime = 0; sliderDuration > ellapsedTime; ellapsedTime += msPerPart)
                {
                    var endtime = ellapsedTime + msPerPart;
                    var duration = msPerPart;

                    OsbSprite body = this.layer.CreateSprite("sb/sprites/hold_body.png");

                    SliderParts sliderPositon = new SliderParts(new Vector2(10, 10), this.starttime + ellapsedTime, duration, body);
                    this.sliderPositions.Add(sliderPositon);

                }
            }

            // Adjust the StartTime by the offset and calculate its position in the cycle

            int cycle = (int)Math.Floor((hitObject.StartTime - offset) / beatDuration);

            int adjustedTime = (int)Math.Round(hitObject.StartTime - offset - (cycle * beatDuration));

            if (IsCloseTo(adjustedTime, beatDuration, 32))  // 1/3 rhythms
            {
                this.noteType = 16;  // Whole tick
            }
            if (IsCloseTo(adjustedTime, beatDuration, 24))  // 1/3 rhythms
            {
                this.noteType = 12;  // Whole tick
            }
            if (IsCloseTo(adjustedTime, beatDuration, 16))  // 1/3 rhythms
            {
                this.noteType = 16;  // Whole tick
            }
            if (IsCloseTo(adjustedTime, beatDuration, 12))  // 1/3 rhythms
            {
                this.noteType = 12;  // Whole tick
            }
            if (IsCloseTo(adjustedTime, beatDuration, 4))  // 1/3 rhythms
            {
                this.noteType = 4;  // Whole tick
            }
            if (IsCloseTo(adjustedTime, beatDuration, 3))  // 1/3 rhythms
            {
                this.noteType = 3;  // Whole tick
            }
            if (IsCloseTo(adjustedTime, beatDuration, 2))  // 1/3 rhythms
            {
                this.noteType = 2;  // Whole tick
            }
            if (IsCloseTo(adjustedTime, beatDuration, 1))  // 1/3 rhythms
            {
                this.noteType = 1;  // Whole tick
            }

            OsbSprite sprite = null;

            switch (this.noteType)
            {
                case 1:
                    sprite = layer.CreateSprite("sb/sprites/1.png");
                    break;
                case 2:
                    sprite = layer.CreateSprite("sb/sprites/2.png");
                    break;
                case 3:
                    sprite = layer.CreateSprite("sb/sprites/3.png");
                    break;
                case 4:
                    sprite = layer.CreateSprite("sb/sprites/4.png");
                    break;
                case 12:
                    sprite = layer.CreateSprite("sb/sprites/12.png");
                    break;
                case 16:
                    sprite = layer.CreateSprite("sb/sprites/16.png");
                    break;
                default:
                    this.noteType = 1;
                    sprite = layer.CreateSprite("sb/sprites/1.png");
                    break;

            }

            this.columnType = column.type;
            this.noteSprite = sprite;

        }

        public void Render(double starttime, double endtime, OsbEasing easeing, double initialFade, double fadeInTime = 50, double fadeOutTime = 10)
        {

            renderDuration = endtime - starttime;

            if (this.appliedTransformation != "")
            {
                return;
            }

            OsbSprite note = this.noteSprite;

            switch (this.columnType)
            {
                case ColumnType.one:
                    note.Rotate(starttime - 1, 1 * Math.PI / 2);
                    break;
                case ColumnType.two:
                    note.Rotate(starttime - 1, 0 * Math.PI / 2);
                    break;
                case ColumnType.three:
                    note.Rotate(starttime - 1, 2 * Math.PI / 2);
                    break;
                case ColumnType.four:
                    note.Rotate(starttime - 1, 3 * Math.PI / 2);
                    break;
            }

            if (this.isSlider)
            {

                /*foreach (SliderParts sliderBody in sliderPositions)
                {

                    OsbSprite sprite = sliderBody.Sprite;

                    sprite.Fade(sliderBody.Timestamp - renderDuration - fadeInTime, sliderBody.Timestamp - renderDuration + fadeInTime, 0, 1);
                    sprite.Fade(Math.Min(sliderBody.Timestamp + fadeOutTime, this.endtime), 0);

                }*/

                note.Fade(starttime, starttime + fadeInTime, 0, initialFade);
                note.Fade(endtime, 0);
                renderEnd = endtime;
            }
            else
            {
                note.Fade(starttime - 1, starttime + fadeInTime, 0, initialFade);
                note.Fade(endtime + fadeOutTime + 1, 0);
                renderEnd = endtime + fadeOutTime;
            }

            renderStart = starttime;
            renderDuration = endtime - starttime;

        }

        public void ApplyHitLightingToNote(double startime, double endtime, double fadeOutTime, Receptor currentReceptor, double iterationRate = 10)
        {

            double renderStart = startime;
            double renderEnd = endtime + fadeOutTime;

            var originalScale = new Vector2(1f, 1f);
            var baseNoteScale = new Vector2(0.5f, 0.5f);

            var scaleRatio = Vector2.Divide(currentReceptor.renderedSprite.ScaleAt(renderStart), baseNoteScale);
            var currentScale = originalScale * scaleRatio;

            Vector2 currentPosition = currentReceptor.renderedSprite.PositionAt(renderStart);
            OsbSprite hitlighting = layer.CreateSprite($"sb/sprites/{GetRandomJudgement()}.png", OsbOrigin.Centre, currentPosition);

            float currentOpacity = currentReceptor.renderedSprite.OpacityAt(renderStart);
            double currentRotation = currentReceptor.renderedSprite.RotationAt(renderStart);

            OsbSprite hold;

            double localCurrentTime = renderStart;

            if (isSlider == false)
            {
                // Handle non hold
                while (localCurrentTime < renderEnd)
                {
                    double localTime = localCurrentTime + iterationRate;

                    scaleRatio = Vector2.Divide(currentReceptor.renderedSprite.ScaleAt(localTime), baseNoteScale);
                    var newScale = originalScale * scaleRatio;

                    Vector2 nexPosition = currentReceptor.renderedSprite.PositionAt(localTime);
                    float newOpactiy = currentReceptor.renderedSprite.OpacityAt(localCurrentTime);
                    double newRotation = currentReceptor.renderedSprite.RotationAt(localTime);

                    hitlighting.Move(localCurrentTime, localTime, currentPosition, nexPosition);
                    hitlighting.ScaleVec(localCurrentTime, localTime, currentScale, newScale);
                    hitlighting.Fade(localCurrentTime, newOpactiy);
                    hitlighting.Rotate(localCurrentTime, localTime, currentRotation, newRotation);

                    currentScale = newScale;
                    currentRotation = newRotation;
                    currentPosition = nexPosition;
                    currentOpacity = newOpactiy;
                    localCurrentTime += iterationRate;
                }

                hitlighting.Fade(renderEnd, 0);
            }

            if (isSlider == true)
            {
                // Handle hold
                Vector2 currentHoldPosition = currentReceptor.renderedSprite.PositionAt(renderStart);
                hold = layer.CreateSprite("sb/sprites/hold.png", OsbOrigin.Centre, currentHoldPosition);

                float currentHoldOpacity = currentReceptor.renderedSprite.OpacityAt(renderStart);
                double currentHoldRotation = currentReceptor.renderedSprite.RotationAt(renderStart);

                scaleRatio = Vector2.Divide(currentReceptor.renderedSprite.ScaleAt(renderStart), baseNoteScale);
                var currentHoldScale = originalScale * scaleRatio;

                while (localCurrentTime < Math.Min(renderStart + fadeOutTime, this.endtime))
                {
                    double localTime = localCurrentTime + iterationRate;

                    scaleRatio = Vector2.Divide(currentReceptor.renderedSprite.ScaleAt(localTime), baseNoteScale);
                    var newScale = originalScale * scaleRatio;

                    Vector2 nexPosition = currentReceptor.renderedSprite.PositionAt(localTime);
                    float newOpactiy = currentReceptor.renderedSprite.OpacityAt(localTime);
                    double newRotation = currentReceptor.renderedSprite.RotationAt(localTime);

                    hitlighting.Move(localCurrentTime, localTime, currentPosition, nexPosition);
                    hitlighting.ScaleVec(localCurrentTime, localTime, currentScale, newScale);
                    hitlighting.Fade(localCurrentTime, newOpactiy);
                    hitlighting.Rotate(localCurrentTime, localTime, currentRotation, newRotation);

                    currentScale = newScale;
                    currentRotation = newRotation;
                    currentPosition = nexPosition;
                    currentOpacity = newOpactiy;
                    localCurrentTime += iterationRate;
                }

                hitlighting.Fade(renderStart + fadeOutTime, 0);

                // render out hold sprite

                localCurrentTime = renderStart;

                while (localCurrentTime < this.endtime)
                {

                    double localTime = localCurrentTime + iterationRate;

                    scaleRatio = Vector2.Divide(currentReceptor.renderedSprite.ScaleAt(localTime), baseNoteScale);
                    var newScale = originalScale * scaleRatio;

                    Vector2 nexPosition = currentReceptor.renderedSprite.PositionAt(localTime);
                    float newOpactiy = currentReceptor.renderedSprite.OpacityAt(localTime);
                    double newRotation = currentReceptor.renderedSprite.RotationAt(localTime);

                    hold.Move(localCurrentTime, localTime, currentHoldPosition, nexPosition);
                    hold.ScaleVec(localCurrentTime, localTime, currentHoldScale, newScale);
                    hold.Fade(localCurrentTime, localTime, currentHoldOpacity, newOpactiy);
                    hold.Rotate(localCurrentTime, localTime, currentHoldRotation, newRotation);

                    currentHoldScale = newScale;
                    currentHoldRotation = newRotation;
                    currentHoldPosition = nexPosition;
                    currentHoldOpacity = newOpactiy;
                    localCurrentTime += iterationRate;
                }

                hold.Fade(renderEnd, 0);

            }

        }

        public void RenderTransformed(double starttime, double endTime, string reference, double fadeInTime = 50, double fadeOutTime = 0)
        {

            if (this.appliedTransformation == reference)
            {
                return;
            }

            renderDuration = endTime - starttime;

            OsbSprite oldSprite = this.noteSprite;

            this.appliedTransformation = reference;
            oldSprite.Rotate(starttime, 0);
            oldSprite.Fade(starttime, 0);
            this.noteSprite = layer.CreateSprite(Path.Combine("sb", "transformation", reference, this.columnType.ToString(), noteType.ToString(), noteType.ToString() + ".png"), OsbOrigin.Centre, oldSprite.PositionAt(starttime));
            layer.Discard(oldSprite);

            OsbSprite note = this.noteSprite;

            if (this.isSlider)
            {

                /*foreach (SliderParts sliderBody in sliderPositions)
                {

                    OsbSprite sprite = sliderBody.Sprite;

                    sprite.Fade(sliderBody.Timestamp - renderDuration, sliderBody.Timestamp - renderDuration + fadeInTime, 0, 1);
                    sprite.Fade(Math.Min(sliderBody.Timestamp + fadeOutTime, this.endtime), 0);

                }*/

                note.Fade(starttime, starttime + fadeInTime, 0, 1);
                note.Fade(endTime, 0);
            }
            else
            {
                note.Fade(starttime, starttime + fadeInTime, 0, 1);
                note.Fade(endTime + fadeOutTime, 0);
            }

        }

        public void UpdateTransformed(double starttime, double endtime, string reference, double fadeOutTime = 0)
        {

            if (this.appliedTransformation == reference)
            {
                return;
            }

            renderDuration = endtime - starttime;

            OsbSprite oldSprite = this.noteSprite;

            this.appliedTransformation = reference;
            oldSprite.Fade(starttime, 0);
            this.noteSprite = layer.CreateSprite(Path.Combine("sb", "transformation", reference, this.columnType.ToString(), noteType.ToString(), noteType.ToString() + ".png"), OsbOrigin.Centre, oldSprite.PositionAt(starttime));
            // layer.Discard(oldSprite);

            OsbSprite note = this.noteSprite;

            if (this.isSlider)
            {

                /*foreach (SliderParts sliderBody in sliderPositions)
                {

                    OsbSprite sprite = sliderBody.Sprite;

                    sprite.Fade(sliderBody.Timestamp - renderDuration, sliderBody.Timestamp - renderDuration, 0, 1);
                    sprite.Fade(Math.Min(sliderBody.Timestamp + fadeOutTime, this.endtime), 0);

                }*/

                note.Fade(starttime, 1);
                note.Fade(endtime, 0);
            }
            else
            {
                note.Fade(starttime, 1);
                note.Fade(endtime + fadeOutTime, 0);
            }

        }

        public string update(double currentTime, string reference, double easetime, double fadeOutTime = 0)
        {

            if (currentTime < renderStart)
            {
                return "deb";
            }

            if (currentTime > renderEnd)
            {
                return "deb";
            }

            this.appliedTransformation = reference;
            this.noteSprite.Fade(currentTime, 0);
            this.noteSprite = layer.CreateSprite(Path.Combine("sb", "transformation", reference, this.columnType.ToString(), noteType.ToString(), noteType.ToString() + ".png"), OsbOrigin.Centre, this.noteSprite.PositionAt(currentTime));

            OsbSprite note = this.noteSprite;
            note.Rotate(currentTime, 0);

            if (this.isSlider)
            {

                foreach (SliderParts sliderBody in sliderPositions)
                {

                    OsbSprite sprite = sliderBody.Sprite;

                    sprite.Fade(sliderBody.Timestamp - renderDuration, 1);
                    sprite.Fade(renderEnd, 0);

                }

                note.Fade(currentTime, 1);
                note.Fade(renderEnd, 0);
            }
            else
            {
                note.Fade(currentTime, 1);
                note.Fade(renderEnd, 0);
            }

            return "deb";

        }

        bool IsCloseTo(int adjusted, double beatDuration, int divisions, int margin = 2)
        {
            double baseTick = beatDuration / divisions;
            for (int multiplier = 1; multiplier <= divisions; multiplier++)
            {
                if (Math.Abs(adjusted - (baseTick * multiplier)) <= margin)
                    return true;
            }
            return false;
        }

        public void Fade(double starttime, double endtime, OsbEasing easing, float value)
        {
            if (this.noteSprite.OpacityAt(starttime) != value)
                this.noteSprite.Fade(easing, starttime, endtime, this.noteSprite.OpacityAt(starttime), value);
        }


        public void invisible(double time)
        {
            OsbSprite note = this.noteSprite;

            note.Fade(time, 0);
        }

        public void Move(double starttime, double duration, OsbEasing easeing, Vector2 startposition, Vector2 endposition)
        {
            OsbSprite note = this.noteSprite;
            note.Move(easeing, starttime, starttime + duration, startposition, endposition);
        }

        public void MoveAbsolute(double starttime, double endTime, OsbEasing easeing, Vector2 startposition, Vector2 endposition)
        {
            OsbSprite note = this.noteSprite;

            note.Move(easeing, starttime, endTime, startposition, endposition);
        }

        public void Rotate(double starttime, double duration, OsbEasing easing, double rotation)
        {
            OsbSprite note = this.noteSprite;

            note.Rotate(easing, starttime, starttime + duration, getRotation(starttime), getRotation(starttime) + rotation);
        }

        public void AbsoluteRotate(double starttime, double duration, OsbEasing easing, double rotation)
        {
            OsbSprite note = this.noteSprite;

            note.Rotate(easing, starttime, starttime + duration, getRotation(starttime), rotation);
        }

        public void Scale(double starttime, double duration, OsbEasing easeing, Vector2 before, Vector2 after)
        {
            OsbSprite note = this.noteSprite;
            note.ScaleVec(easeing, starttime, starttime + duration, before, after);
        }

        public void ScaleDirect(double starttime, double duration, OsbEasing easeing, Vector2 before, Vector2 after)
        {
            OsbSprite note = this.noteSprite;
            note.ScaleVec(easeing, starttime, starttime + duration, before, after);
        }

        public double getRotation(double starttime)
        {
            OsbSprite note = this.noteSprite;

            return note.RotationAt(starttime);
        }

        public float OpacityAt(double currentTime)
        {
            OsbSprite note = this.noteSprite;


            return note.OpacityAt(currentTime);
        }

        static string GetRandomJudgement()
        {
            int randomNumber = _random.Next(1, 101); // Generates a random number between 1 and 100

            if (randomNumber <= 1)    // 1% chance
                return "wayof";
            if (randomNumber <= 3)    // 2% chance
                return "decent";
            if (randomNumber <= 8)    // 5% chance
                return "great";
            if (randomNumber <= 18)   // 20% chance
                return "fantastic";

            // 72% chance
            return "excelent";
        }



    }
}