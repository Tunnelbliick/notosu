
using System;
using System.Collections.Generic;
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

        public Note(StoryboardLayer layer, OsuHitObject hitObject, Column column, double bpm, double offset)
        {

            this.layer = layer;
            this.hitObject = hitObject;
            this.starttime = hitObject.StartTime;
            this.endtime = hitObject.EndTime;

            // calculate duration of one beat
            double beatDuration = 60000 / bpm;
            double tickDuration = beatDuration / 4f;
            // Calculate tick values
            int white = 0;
            double blue = tickDuration * 1;
            double red = tickDuration * 2;
            double blue2 = tickDuration * 3;
            double white2 = tickDuration * 4; // This is where the pattern repeats

            Column currentColumn = column;

            if (this.starttime != this.endtime)
            {

                this.isSlider = true;
                var sliderDuration = this.endtime - this.starttime;

                for (int i = 0; sliderDuration > i; i += 20)
                {
                    double split = 20 / sliderParts;
                    for (int count = 0; sliderParts >= count; count++)
                    {

                        OsbSprite body = this.layer.CreateSprite("sb/hold_body.png");

                        var variation = split * count;

                        SliderParts sliderPositon = new SliderParts(new Vector2(10, 10), this.starttime + i + variation, body);
                        this.sliderPositions.Add(sliderPositon);
                    }
                }
            }

            // Adjust the StartTime by the offset and calculate its position in the cycle

            int cycle = (int)Math.Floor((hitObject.StartTime - offset) / beatDuration);

            int adjustedTime = (int)Math.Round(hitObject.StartTime - offset - (cycle * beatDuration));
            int margin = 2; // Change this value to increase or decrease the margin of error

            if (Math.Abs(adjustedTime - white) <= margin) // The cycle starts with a white tick
            {
                this.noteType = 4;  // White tick
            }
            else if (Math.Abs(adjustedTime - blue) <= margin || Math.Abs(adjustedTime - blue2) <= margin)
            {
                this.noteType = 16;  // Blue tick
            }
            else if (Math.Abs(adjustedTime - red) <= margin)
            {
                this.noteType = 8;  // Red tick
            }


            var sprite = layer.CreateSprite("sb/note.png"); ;

            switch (this.noteType)
            {
                case 4:
                    sprite = layer.CreateSprite("sb/4th.png");
                    break;
                case 8:
                    sprite = layer.CreateSprite("sb/8th.png");
                    break;
                case 16:
                    sprite = layer.CreateSprite("sb/16th.png");
                    break;
                default:
                    this.noteType = 4;
                    sprite = layer.CreateSprite("sb/4th.png");
                    break;

            }

            this.columnType = column.type;
            this.noteSprite = sprite;

        }

        public void Render(double starttime, double duration, OsbEasing easeing, double fadeOutTime = 0)
        {

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

                foreach (SliderParts sliderBody in sliderPositions)
                {

                    OsbSprite sprite = sliderBody.Sprite;

                    sprite.Fade(sliderBody.Timestamp - duration, sliderBody.Timestamp - duration + 50, 0, 1);
                    sprite.Fade(Math.Min(sliderBody.Timestamp + fadeOutTime, this.endtime), 0);

                }

                note.Fade(starttime, starttime + 50, 0, 1);
                note.Fade(this.endtime, 0);
                renderEnd = this.endtime;
            }
            else
            {
                note.Fade(starttime, starttime + 50, 0, 1);
                note.Fade(starttime + duration + fadeOutTime, 0);
                renderEnd = starttime + duration + fadeOutTime;
            }

            renderStart = starttime;
            renderDuration = duration;

        }

        public void RenderTransformed(double starttime, double duration, OsbEasing easeing, double transformationTime, string reference, double fadeOutTime = 0)
        {

            if (this.appliedTransformation == reference)
            {
                return;
            }

            OsbSprite oldSprite = this.noteSprite;

            this.appliedTransformation = reference;
            oldSprite.Fade(starttime, 0);
            this.noteSprite = layer.CreateSprite(Path.Combine("sb", "transformation", reference, this.columnType.ToString(), noteType.ToString(), noteType.ToString() + ".png"), OsbOrigin.Centre, oldSprite.PositionAt(starttime));
            layer.Discard(oldSprite);

            OsbSprite note = this.noteSprite;
            note.Rotate(starttime, 0);

            if (this.isSlider)
            {

                foreach (SliderParts sliderBody in sliderPositions)
                {

                    OsbSprite sprite = sliderBody.Sprite;

                    sprite.Fade(sliderBody.Timestamp - duration, sliderBody.Timestamp - duration + 50, 0, 1);
                    sprite.Fade(Math.Min(sliderBody.Timestamp + fadeOutTime, this.endtime), 0);

                }

                note.Fade(starttime, starttime + 50, 0, 1);
                note.Fade(this.endtime, 0);
            }
            else
            { 
                note.Fade(starttime, starttime + 50, 0, 1);
                note.Fade(starttime + duration + fadeOutTime, 0);
            }

        }

        public void UpdateTransformed(double starttime, double duration, OsbEasing easeing, double transformationTime, string reference, double fadeOutTime = 0)
        {

            if (this.appliedTransformation == reference)
            {
                return;
            }

            OsbSprite oldSprite = this.noteSprite;

            this.appliedTransformation = reference;
            oldSprite.Fade(starttime, 0);
            this.noteSprite = layer.CreateSprite(Path.Combine("sb", "transformation", reference, this.columnType.ToString(), noteType.ToString(), noteType.ToString() + ".png"), OsbOrigin.Centre, oldSprite.PositionAt(starttime));

            OsbSprite note = this.noteSprite;
            note.Rotate(starttime, 0);

            if (this.isSlider)
            {

                foreach (SliderParts sliderBody in sliderPositions)
                {

                    OsbSprite sprite = sliderBody.Sprite;

                    sprite.Fade(sliderBody.Timestamp - duration, 1);
                    sprite.Fade(Math.Min(sliderBody.Timestamp + fadeOutTime, this.endtime), 0);

                }

                note.Fade(starttime, 1);
                note.Fade(this.endtime, 0);
            }
            else
            {
                note.Fade(starttime, 1);
                note.Fade(starttime + duration + fadeOutTime, 0);
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

    }
}