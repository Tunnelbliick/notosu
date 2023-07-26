
using System;
using System.Collections.Generic;
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
        public double starttime;
        public double endtime;
        public ColumnType columnType;

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

            // Adjust the StartTime by the offset and calculate its position in the cycle

            int cycle = (int)Math.Floor((hitObject.StartTime - offset) / beatDuration);

            int adjustedTime = (int)Math.Round((hitObject.StartTime - offset) - (cycle * beatDuration));
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
                    sprite = layer.CreateSprite("sb/4th.png");
                    break;

            }

            this.columnType = column.type;
            this.noteSprite = sprite;

        }

        public void Render(double starttime, double duration, OsbEasing easeing)
        {
            OsbSprite note = this.noteSprite;

            switch (this.columnType)
            {
                case ColumnType.one:
                    note.Rotate(starttime - 1, Math.PI / 2);
                    break;
                case ColumnType.two:
                    note.Rotate(starttime - 1, 0f);
                    break;
                case ColumnType.three:
                    note.Rotate(starttime - 1, Math.PI);
                    break;
                case ColumnType.four:
                    note.Rotate(starttime - 1, Math.PI * 2);
                    break;
            }

            note.Fade(starttime, 1);
            note.Fade(starttime + duration, 0);

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