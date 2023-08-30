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
        public float width = 250f;
        public float height = 480f;

        public float receptorHeightOffset = 0f;
        public float noteHeightOffset = 0f;

        private double rotation = 0f;

        private CommandScale receptorScale = new CommandScale(0.5);
        private string receptorSpritePath = "sb/receiver.png";

        private OsbSprite bg;

        private double starttime;
        private double endtime;

        // Reference for active Columns;
        public Dictionary<ColumnType, Column> columns = new Dictionary<ColumnType, Column>();

        // Notes Per Column
        public Dictionary<ColumnType, Dictionary<double, Note>> columnNotes = new Dictionary<ColumnType, Dictionary<double, Note>>();

        public void initilizePlayField(StoryboardLayer receptors, StoryboardLayer notes, double starttime, double endtime, float receportWidth, float receptorHeightOffset, float noteHeightOffset)
        {

            var bg = notes.CreateSprite("sb/transparent.png");
            bg.ScaleVec(0, new Vector2(width, height));
            bg.Fade(starttime, 1);
            bg.Fade(endtime, 0);

            this.bg = bg;
            this.starttime = starttime;
            this.endtime = endtime;

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
                origin.MoveOrigin(starttime, new Vector2(x, 240 + (240 - height + noteHeightOffset)), OsbEasing.None, 0);

                position += getColumnWidth();
            }

            this.noteHeightOffset = noteHeightOffset;
            this.receptorHeightOffset = receptorHeightOffset;

        }

        public void SetWidth(float width) {
            this.width = width;
        }

        public void SetHeight(float height) {
            this.height = height;
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
                        continue;

                    if (hitobject.StartTime <= this.starttime && hitobject.EndTime >= this.endtime)
                        continue;

                    Note currentNote = new Note(noteLayer, hitobject, column, bpm, offset);

                    notes.Add(hitobject.StartTime, currentNote);

                }

                columnNotes.Add(column.type, notes);

            }
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



        public double ScalePlayField(double starttime, double duration, OsbEasing easing, float width, float height)
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
                origin.MoveOrigin(starttime, newOpposit, easing, duration);

                position += getColumnWidth();
            }

            return starttime + duration;

        }

        public double Zoom(double starttime, double duration, OsbEasing easing, double zoomAmount, Boolean keepPosition)
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

                if (!keepPosition)
                {
                    Vector2 relativeReceptorPosition = Vector2.Subtract(receptorPosition, center);
                    Vector2 relativeOriginPosition = Vector2.Subtract(originPosition, center);

                    Vector2 zoomedReceptorPosition = Vector2.Multiply(relativeReceptorPosition, (float)zoomAmount);
                    Vector2 zoomedOriginPosition = Vector2.Multiply(relativeOriginPosition, (float)zoomAmount);

                    receptorTarget = Vector2.Add(zoomedReceptorPosition, center);
                    originTarget = Vector2.Add(zoomedOriginPosition, center);

                    receptor.MoveReceptor(starttime, receptorTarget, easing, duration);
                    origin.MoveOrigin(starttime, originTarget, easing, duration);
                }

            }

            return endtime;
        }
        public String ZoomAndMove(int starttime, int duration, OsbEasing easing, Vector2 absoluteScale, Vector2 newPosition)
        {
            double endtime = starttime + duration;

            String debug = "";

            Vector2 center = calculatePlayFieldCenter(starttime);

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;
                float xOffset = 0;
                float yOffset = 0;

                Vector2 receptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 originPosition = origin.getCurrentPosition(starttime);

                Vector2 currentScale = receptor.getCurrentScale(starttime);

                Vector2 scaleDifference = new Vector2(absoluteScale.X / currentScale.X, absoluteScale.Y / currentScale.Y);

                if (absoluteScale != currentScale)
                {
                    // This needs to be calculated differently to match scale changes above 1 since 1 doesnt change anything but the sprite scale changes with 1
                    xOffset = (receptorPosition.X - center.X) * scaleDifference.X - (receptorPosition.X - center.X);
                    yOffset = (receptorPosition.Y - center.Y) * scaleDifference.Y - (receptorPosition.Y - center.Y);
                }

                receptorPosition.X += xOffset;
                originPosition.X += xOffset;

                receptorPosition.Y += yOffset;
                originPosition.Y += -yOffset;

                // Apparently subtract returns a whole number
                Vector2 movement = new Vector2(newPosition.X - center.X, newPosition.Y - center.Y);

                receptorPosition = Vector2.Add(receptorPosition, movement);
                originPosition = Vector2.Add(originPosition, movement);

                // debug += $"({receptorPosition.X} - {center.X}) * {scaleDifference.X} - ({receptorPosition.X} - {center.X})\n";
                // debug += $"{movement} = {newPosition} - {center}\n";
                // debug += $"({receptorPosition} = {receptorPosition} + {movement}\n";
                // debug += "\n";

                // Move and scale the receptors and origins
                receptor.MoveReceptor(starttime, receptorPosition, easing, duration);
                origin.MoveOrigin(starttime, originPosition, easing, duration);

                receptor.ScaleReceptor(starttime, absoluteScale, easing, duration);
                origin.ScaleReceptor(starttime, absoluteScale, easing, duration);
            }

            return debug;
        }
        public Vector2 calculatePlayFieldCenter(double currentTime)
        {
            Vector2 center;

            Vector2 topLeft = new Vector2(0, 0);
            Vector2 bottomRight = new Vector2(0, 0);

            foreach (Column column in columns.Values)
            {

                Vector2 receptor = column.getReceptorPosition(currentTime);
                Vector2 origin = column.getOriginPosition(currentTime);

                if (topLeft == new Vector2(0, 0))
                {
                    topLeft = receptor;
                    bottomRight = origin;
                }

                if (receptor.X < topLeft.X)
                {
                    topLeft.X = receptor.X;
                }
                if (origin.X < topLeft.X)
                {
                    topLeft.X = origin.X;
                }

                if (receptor.Y < topLeft.Y)
                {
                    topLeft.Y = receptor.X;
                }
                if (origin.Y < topLeft.Y)
                {
                    topLeft.Y = origin.Y;
                }

                if (receptor.X > bottomRight.X)
                {
                    bottomRight.X = receptor.X;
                }
                if (origin.X > bottomRight.X)
                {
                    bottomRight.X = origin.X;
                }

                if (receptor.Y > bottomRight.Y)
                {
                    bottomRight.Y = receptor.X;
                }
                if (origin.Y > bottomRight.Y)
                {
                    bottomRight.Y = origin.Y;
                }

            }

            center.X = (topLeft.X + bottomRight.X) / 2;
            center.Y = (topLeft.Y + bottomRight.Y) / 2;

            return center;
        }

        public double MoveReceptorAbsolute(double starttime, double duration, OsbEasing easing, ColumnType column, Vector2 position)
        {

            Column currentColumn = columns[column];

            currentColumn.MoveReceptor(starttime, duration, position, easing);

            return starttime + duration;

        }

        public double RotateReceptorRelative(double starttime, double duration, OsbEasing easing, ColumnType column, double rotation)
        {
            if (column == ColumnType.all)
            {
                foreach (Column currentColumn in columns.Values)
                {

                    currentColumn.RotateReceptorRelative(starttime, duration, easing, rotation);

                }
            }
            else
            {

                Column currentColumn = columns[column];

                currentColumn.RotateReceptorRelative(starttime, duration, easing, rotation);
            }

            return starttime + duration;

        }

        public double RotateReceptorAbsolute(double starttime, double duration, OsbEasing easing, ColumnType column, double rotation)
        {
            if (column == ColumnType.all)
            {
                foreach (Column currentColumn in columns.Values)
                {

                    currentColumn.RotateReceptor(starttime, duration, easing, rotation);

                }
            }
            else
            {

                Column currentColumn = columns[column];

                currentColumn.RotateReceptor(starttime, duration, easing, rotation);
            }

            return starttime + duration;

        }

        public double MoveOriginAbsolute(double starttime, double duration, OsbEasing easing, ColumnType column, Vector2 position)
        {

            Column currentColumn = columns[column];

            currentColumn.MoveOrigin(starttime, duration, position, easing);

            return starttime + duration;

        }

        // This will rotate the Playfield but keep the Receptors in a static position
        public void RotatePlayFieldStatic(int starttime, int duration, OsbEasing easing, double radians)
        {

            // bg.Rotate(easing, starttime, starttime + duration, this.rotation, radians);

            this.rotation = radians;

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;

                receptor.RotateReceptor(starttime, duration, easing, radians);
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

                receptor.PivotReceptor(starttime, radians, easing, duration, sampleCount, calculatePlayFieldCenter(starttime));
                origin.PivotReceptor(starttime, radians, easing, duration, sampleCount, calculatePlayFieldCenter(starttime));
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
                origin.MoveOrigin(starttime, currentOriginPosition, easing, half);

                receptor.MoveReceptor(starttime + half, originaleReceptorPosition, easing, half);
                origin.MoveOrigin(starttime + half, originalOriginPosition, easing, half);
            }

        }

        public double moveFieldX(double starttime, double duration, OsbEasing easing, float amount)
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
                origin.MoveOrigin(starttime, currentOriginPosition, easing, duration);
            }

            return starttime + duration;
        }

        public double moveField(double starttime, double duration, OsbEasing easing, float amountX, float amountY)
        {
            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                Vector2 currentReceptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 currentOriginPosition = origin.getCurrentPosition(starttime);

                currentReceptorPosition.X += amountX;
                currentOriginPosition.X += amountX;

                currentReceptorPosition.Y += amountY;
                currentOriginPosition.Y += amountY;

                receptor.MoveReceptor(starttime, currentReceptorPosition, easing, duration);
                origin.MoveOrigin(starttime, currentOriginPosition, easing, duration);
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