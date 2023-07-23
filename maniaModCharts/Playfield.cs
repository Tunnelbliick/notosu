using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Mapset;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding.CommandValues;

namespace StorybrewScripts
{

    public class Playfield
    {

        // Default SB height / width

        private float absoluteWidth = 640f;
        private float width = 250f;
        private float height = 480f;

        private float receptorHeightOffset = 0f;
        private float noteHeightOffset = 0f;
        private float offsetX = 0f;
        private float offsetY = 0f;
        // Scale to dynamically scale Playfield elements effects all elements
        private float scale = 0.05f;

        private double rotation = 0f;

        private CommandScale receptorScale = new CommandScale(0.5);
        private string fullNotePath = "sb/4th.png";
        private string halfNotePath = "sb/8th.png";
        private string quarterNotePath = "sb/16th.png";
        private string sliderPath = "sb/hold_body.png";
        private string receptorSpritePath = "sb/receiver.png";

        private OsbSprite bg;

        // Reference for active Columns;
        public Dictionary<ColumnType, Column> columns = new Dictionary<ColumnType, Column>();

        // Notes Per Column
        public Dictionary<ColumnType, Dictionary<double, Note>> columnNotes = new Dictionary<ColumnType, Dictionary<double, Note>>();

        public void initilizePlayField(StoryboardLayer receptors, StoryboardLayer notes, int starttime, int endtime, float receportWidth, float receptorHeightOffset, float noteHeightOffset)
        {

            var bg = notes.CreateSprite("sb/transparent.png");
            bg.ScaleVec(0, new Vector2(width, height));
            bg.Fade(starttime, 1);
            bg.Fade(endtime, 0);

            this.bg = bg;

            Column one = new Column(128, ColumnType.one, receptorSpritePath, receptors, receptorScale, starttime);
            Column two = new Column(256, ColumnType.two, receptorSpritePath, receptors, receptorScale, starttime);
            Column three = new Column(384, ColumnType.three, receptorSpritePath, receptors, receptorScale, starttime);
            Column four = new Column(512, ColumnType.four, receptorSpritePath, receptors, receptorScale, starttime);

            columns.Add(one.type, one);
            columns.Add(two.type, two);
            columns.Add(three.type, three);
            columns.Add(four.type, four);

            Receptor receptor1 = one.receptor;

            OsbSprite receptor1Sprite = receptor1.receptorSprite;

            float position = 0f;

            foreach (Column column in columns.Values)
            {

                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                var x = calculateOffset() + position + getColumnWidth() / 2;

                receptor.Render(starttime, endtime);
                origin.Render(starttime, endtime);
                receptor.MoveReceptor(starttime, new Vector2(x, height - receptorHeightOffset), OsbEasing.None, 0);
                origin.MoveReceptor(starttime, new Vector2(x, 240 + (240 - height + noteHeightOffset)), OsbEasing.None, 0);

                position += getColumnWidth();
            }

            this.noteHeightOffset = noteHeightOffset;
            this.receptorHeightOffset = receptorHeightOffset;

        }

        public void initializeNotes(List<OsuHitObject> objects, StoryboardLayer noteLayer, double bpm, double offset)
        {


            foreach (Column column in columns.Values)
            {

                Dictionary<double, Note> notes = new Dictionary<double, Note>();

                double xOffset = column.offset;

                foreach (OsuHitObject hitobject in objects)
                {

                    if (hitobject.Position.X != xOffset)
                    {
                        continue;
                    }

                    Note currentNote = new Note(noteLayer, hitobject, column, bpm, offset);

                    notes.Add(hitobject.StartTime, currentNote);

                }

                columnNotes.Add(column.type, notes);

            }
        }


        public String drawNotesDefault(double starttime, double duration, double easetime, OsbEasing easing)
        {

            String debug = "";

            double endtime = starttime + duration;

            foreach (Column column in columns.Values)
            {
                Dictionary<double, Note> notes = columnNotes[column.type];

                // Get only the keys (hittimes) that fall within the specified range considering easetime
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime <= endtime).ToList();

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];
                    double noteTime = note.starttime;
                    double fadeInTime = noteTime - easetime;

                    note.Render(fadeInTime, easetime, easing);
                    note.Move(fadeInTime, easetime, easing, column.origin.getCurrentPosition(note.starttime), column.receptor.getCurrentPosition(note.starttime));
                    note.Scale(fadeInTime, easetime, easing, column.origin.getCurrentScale(note.starttime), column.receptor.getCurrentScale(note.starttime));
                }
            }

            return debug;
        }

        public String drawNotesByEndPosition(double starttime, double duration, double easetime, OsbEasing easing)
        {

            double endtime = starttime + duration;

            String debug = "";

            foreach (Column column in columns.Values)
            {
                Dictionary<double, Note> notes = columnNotes[column.type];

                // Get only the keys (hittimes) that fall within the specified range considering easetime
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime <= endtime).ToList();

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];
                    double noteTime = note.starttime;
                    double fadeInTime = noteTime - easetime;

                    note.Render(fadeInTime, easetime, easing);
                    note.Move(fadeInTime, easetime, easing, new Vector2(column.receptor.getCurrentPosition(note.starttime).X, column.origin.getCurrentPosition(fadeInTime).Y), column.receptor.getCurrentPosition(note.starttime));
                    note.Scale(fadeInTime, easetime, easing, column.origin.getCurrentScale(fadeInTime), column.receptor.getCurrentScale(note.starttime));
                }
            }

            return debug;
        }

        public String drawNotesBySnapshotPosition(double starttime, double duration, double easetime, OsbEasing easing, int snaps)
        {

            double endtime = starttime + duration;

            String debug = "";

            double snapLength = easetime / snaps;

            foreach (Column column in columns.Values)
            {
                Dictionary<double, Note> notes = columnNotes[column.type];

                // Get only the keys (hittimes) that fall within the specified range considering easetime
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime <= endtime).ToList();

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];
                    double noteTime = note.starttime;
                    double fadeInTime = noteTime - easetime;

                    double travelDistance = Math.Abs(column.origin.getCurrentPosition(fadeInTime).Y - column.receptor.getCurrentPosition(fadeInTime).Y);
                    double distancePerSnap = travelDistance / snaps;
                    bool moveUpwards = column.origin.getCurrentPosition(fadeInTime).Y > column.receptor.getCurrentPosition(fadeInTime).Y;

                    Vector2 originPosition = column.origin.getCurrentPosition(fadeInTime);
                    Vector2 receptorPosition = column.receptor.getCurrentPosition(fadeInTime);

                    double currentTime = fadeInTime;

                    note.Render(fadeInTime, easetime, easing);

                    for (int i = 0; i <= snaps; i++)
                    {

                        double snapDuration = snapLength * i;

                        Vector2 currentPosition = column.receptor.getCurrentPosition(currentTime + snapDuration);
                        debug = moveUpwards + "" + distancePerSnap;
                        double newYPosition = moveUpwards ? (originPosition.Y - i * distancePerSnap) : (originPosition.Y + i * distancePerSnap);

                        Vector2 newPosition = new Vector2(currentPosition.X, (float)newYPosition);

                        note.Move(currentTime, snapDuration, easing, originPosition, newPosition);

                        currentTime += snapDuration;
                        originPosition = newPosition;
                    }
                    note.Scale(fadeInTime, easetime, easing, column.receptor.getCurrentScale(note.starttime), column.receptor.getCurrentScale(note.starttime));
                }
            }

            return debug;
        }

        public String drawNotesByOriginToReceptor(double starttime, double duration, double easetime, OsbEasing easing, int snaps)
        {
            double endtime = starttime + duration;

            // This will guarantee that the total time of all snaps is exactly easetime
            double snapLength = easetime / snaps;

            String debug = "";

            foreach (Column column in columns.Values)
            {
                Dictionary<double, Note> notes = columnNotes[column.type];

                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime <= endtime).ToList();

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];
                    double noteTime = note.starttime;
                    double fadeInTime = noteTime - easetime;

                    double currentTime = fadeInTime;
                    float progress = 0;

                    Vector2 currentPosition = column.origin.getCurrentPosition(currentTime);

                    for (int i = 0; i <= snaps; i++)
                    {
                        double snapDuration = snapLength * i;
                        double timeLeft = easetime - snapLength * i; ;

                        Vector2 originPosition = column.origin.getCurrentPosition(currentTime);
                        Vector2 receptorPosition = column.receptor.getCurrentPosition(currentTime);

                        // Calculate the progress based on the remaining time
                        progress = (float)timeLeft / (float)easetime;

                        if (progress == 1)
                        {
                            continue;
                        }

                        Vector2 newPosition = Vector2.Lerp(receptorPosition, originPosition, progress);

                        debug = $"Progress: {progress}, Position: {newPosition}, SnapDuration: {snapDuration}, Currenttime: {currentTime}, timeleft: {timeLeft}  \n";

                        note.Move(currentTime, snapLength, easing, currentPosition, newPosition);

                        Vector2 originScale = column.origin.getCurrentScale(currentTime);
                        Vector2 receptorScale = column.receptor.getCurrentScale(currentTime);

                        Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, progress);

                        note.Scale(currentTime, snapLength, easing, column.origin.getCurrentScale(currentTime), scaleProgress);

                        currentTime += snapLength;

                        currentPosition = newPosition;


                    }

                    note.Render(fadeInTime, easetime, easing);
                }
            }

            return debug;
        }

        public double rotateNotes(double starttime, double duration, OsbEasing easing, double rotation)
        {
            double endtime = starttime + duration;
            double lookupTime = starttime - duration;


            foreach (Column column in columns.Values)
            {
                Dictionary<double, Note> notes = columnNotes[column.type];

                var keysInRange = notes.Keys.Where(hittime => hittime >= lookupTime && hittime <= endtime).ToList();

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];

                    note.Rotate(starttime, duration, easing, rotation);
                }
            }

            return endtime;
        }



        public void ScalePlayField(int starttime, int duration, OsbEasing easing, float width, float height)
        {

            bg.ScaleVec(easing, starttime, starttime + duration, this.width, this.height, width, height);

            this.width = width;
            this.height = height;

            float position = 0f;

            foreach (Column column in columns.Values)
            {

                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                var x = calculateOffset() + position + getColumnWidth() / 2;

                var newHeight = Math.Max(height, 0);
                var oppositHeight = Math.Max(height * -1, 0);

                if (newHeight > 240)
                {
                    newHeight -= this.receptorHeightOffset;
                    oppositHeight += this.noteHeightOffset;
                }
                else
                {
                    newHeight += this.receptorHeightOffset;
                    oppositHeight -= this.noteHeightOffset;
                }

                Vector2 newPosition = new Vector2(x, newHeight);
                Vector2 newOpposit = new Vector2(x, oppositHeight);

                receptor.MoveReceptor(starttime, newPosition, easing, duration);
                origin.MoveReceptor(starttime, newOpposit, easing, duration);

                position += getColumnWidth();
            }


        }

        public double Zoom(int starttime, int duration, OsbEasing easing, double zoomAmount, Boolean keepXPosition)
        {
            double endtime = starttime + duration;

            bg.ScaleVec(easing, starttime, starttime + duration, bg.ScaleAt(starttime), Vector2.Multiply(bg.ScaleAt(starttime), (float)zoomAmount));
            Vector2 center = bg.PositionAt(starttime);
            Vector2 newScale = bg.ScaleAt(starttime + duration);

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                Vector2 receptorScale = receptor.getCurrentScale(starttime);
                Vector2 originScale = origin.getCurrentScale(starttime);

                Vector2 receptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 originPosition = origin.getCurrentPosition(starttime);

                Vector2 scaledReceptorScale = Vector2.Multiply(receptorScale, (float)zoomAmount);
                Vector2 scaledOriginScale = Vector2.Multiply(originScale, (float)zoomAmount);

                receptor.ScaleReceptor(starttime, scaledReceptorScale, easing, duration);
                origin.ScaleReceptor(starttime, scaledOriginScale, easing, duration);

                Vector2 receptorTarget = receptorPosition;
                Vector2 originTarget = originPosition;

                if (!keepXPosition)
                {
                    Vector2 relativeReceptorPosition = Vector2.Subtract(receptorPosition, center);
                    Vector2 relativeOriginPosition = Vector2.Subtract(originPosition, center);

                    Vector2 zoomedReceptorPosition = Vector2.Multiply(relativeReceptorPosition, (float)zoomAmount);
                    Vector2 zoomedOriginPosition = Vector2.Multiply(relativeOriginPosition, (float)zoomAmount);

                    receptorTarget = Vector2.Add(zoomedReceptorPosition, center);
                    originTarget = Vector2.Add(zoomedOriginPosition, center);
                }
                else
                {
                    receptorTarget.Y *= (float)zoomAmount;
                    originTarget.Y *= (float)zoomAmount;
                }

                receptor.MoveReceptor(starttime, receptorTarget, easing, duration);
                origin.MoveReceptor(starttime, originTarget, easing, duration);
            }

            return endtime;
        }



        public double SwapColumn(int starttime, int duration, OsbEasing easing, ColumnType column1, ColumnType column2)
        {

            Column left = this.columns[column1];
            Column right = this.columns[column2];

            Vector2 leftOrigin = left.getOriginPosition(starttime);
            Vector2 leftReceptor = left.getReceptorPosition(starttime);

            Vector2 rightOrigin = right.getOriginPosition(starttime);
            Vector2 rightReceptor = right.getReceptorPosition(starttime);

            left.MoveColumn(starttime, duration, rightReceptor, rightOrigin, easing);
            right.MoveColumn(starttime, duration, leftReceptor, leftOrigin, easing);

            return starttime + duration;

        }

        public void flipPlayField(int starttime, int duration, OsbEasing easing, float width, float height, float closeScale, float fareScale)
        {

            Boolean isFlipped = false;

            // bg.ScaleVec(easing, starttime, starttime + duration, this.width, this.height, width, height);

            if (height < this.height / 2)
            {
                isFlipped = true;
            }

            this.width = width;
            this.height = height;

            float position = 0f;

            foreach (Column column in columns.Values)
            {

                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                var x = calculateOffset() + position + getColumnWidth() / 2;

                var newHeight = Math.Max(height, 0);
                var oppositHeight = Math.Max(height * -1, 0);

                if (newHeight > 240)
                {
                    newHeight -= this.receptorHeightOffset;
                    oppositHeight += this.noteHeightOffset;
                }
                else
                {
                    newHeight += this.receptorHeightOffset;
                    oppositHeight -= this.noteHeightOffset;
                }

                Vector2 newPosition = new Vector2(x, newHeight);
                Vector2 newOpposit = new Vector2(x, oppositHeight);

                Vector2 currentScale = receptor.getCurrentScale(starttime);

                receptor.MoveReceptor(starttime, newPosition, easing, duration);
                origin.MoveReceptor(starttime, newOpposit, easing, duration);


                if (isFlipped)
                {
                    receptor.ScaleReceptor(starttime, new Vector2(currentScale.X * closeScale, currentScale.Y * closeScale), easing, duration / 2);
                    receptor.ScaleReceptor(starttime + duration / 2, new Vector2(currentScale.X, currentScale.Y), easing, duration / 2);

                    origin.ScaleReceptor(starttime, new Vector2(currentScale.X * fareScale, currentScale.Y * fareScale), easing, duration / 2);
                    origin.ScaleReceptor(starttime + duration / 2, new Vector2(currentScale.X, currentScale.Y), easing, duration / 2);
                }
                else
                {
                    receptor.ScaleReceptor(starttime, new Vector2(currentScale.X * fareScale, currentScale.Y * fareScale), easing, duration / 2);
                    receptor.ScaleReceptor(starttime + duration / 2, new Vector2(currentScale.X, currentScale.Y), easing, duration / 2);

                    origin.ScaleReceptor(starttime, new Vector2(currentScale.X * closeScale, currentScale.Y * closeScale), easing, duration / 2);
                    origin.ScaleReceptor(starttime + duration / 2, new Vector2(currentScale.X, currentScale.Y), easing, duration / 2);
                }

                position += getColumnWidth();
            }


        }

        // This will rotate the Playfield but keep the Receptors in a static position
        public void RotatePlayFieldStatic(int starttime, int duration, OsbEasing easing, double radians)
        {

            // bg.Rotate(easing, starttime, starttime + duration, this.rotation, radians);

            this.rotation = radians;

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;

                receptor.RotateReceptor(starttime, radians, easing, duration);
            }


        }

        // This will rotate the Playfield but move the Receptors dynamically to adjust for the position
        public void RotatePlayField(int starttime, int duration, OsbEasing easing, double radians, int sampleCount)
        {

            double newRotation = this.rotation + radians;

            bg.Rotate(easing, starttime, starttime + duration, this.rotation, newRotation);

            this.rotation = newRotation;

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                receptor.PivotReceptor(starttime, radians, easing, duration, sampleCount, bg.PositionAt(starttime));
                origin.PivotReceptor(starttime, radians, easing, duration, sampleCount, bg.PositionAt(starttime));
            }

        }

        public void bounceLeftRight(int starttime, int duration, OsbEasing easing, float amount)
        {

            int half = duration / 2;

            Vector2 bgPosition = bg.PositionAt(starttime);

            bg.Move(easing, starttime, half, bgPosition, Vector2.Add(bgPosition, new Vector2(amount, 0)));
            bg.Move(easing, starttime + half, half, Vector2.Add(bgPosition, new Vector2(amount, 0)), bgPosition);

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                Vector2 currentReceptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 currentOriginPosition = origin.getCurrentPosition(starttime);

                Vector2 originaleReceptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 originalOriginPosition = origin.getCurrentPosition(starttime);

                currentReceptorPosition.X -= amount;
                currentOriginPosition.X += amount;

                receptor.MoveReceptor(starttime, currentReceptorPosition, easing, half);
                origin.MoveReceptor(starttime, currentOriginPosition, easing, half);

                receptor.MoveReceptor(starttime + half, originaleReceptorPosition, easing, half);
                origin.MoveReceptor(starttime + half, originalOriginPosition, easing, half);
            }

        }

        public double moveField(int starttime, int duration, OsbEasing easing, float amount)
        {
            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                Vector2 currentReceptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 currentOriginPosition = origin.getCurrentPosition(starttime);

                currentReceptorPosition.X += amount;
                currentOriginPosition.X += amount;

                receptor.MoveReceptor(starttime, currentReceptorPosition, easing, duration);
                origin.MoveReceptor(starttime, currentOriginPosition, easing, duration);
            }

            return starttime + duration;
        }

        public float getColumnWidth()
        {
            return this.width / 4;
        }

        public float calculateOffset()
        {
            return (absoluteWidth - width) / 2;
        }

    }
}