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

        private int starttime;
        private int endtime;

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
                        continue;

                    if (hitobject.StartTime - 2000 <= this.starttime && (hitobject.StartTime - 2000) >= this.endtime)
                        continue;

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
                    note.Move(fadeInTime, easetime, easing, column.origin.getCurrentPosition(note.starttime), column.receptor.getCurrentPositionForNotes(note.starttime));
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
                    note.Move(fadeInTime, easetime, easing, new Vector2(column.receptor.getCurrentPositionForNotes(note.starttime).X, column.origin.getCurrentPosition(fadeInTime).Y), column.receptor.getCurrentPositionForNotes(note.starttime));
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
                    Vector2 receptorPosition = column.receptor.getCurrentPositionForNotes(fadeInTime);

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
                    double renderTime = Math.Max(fadeInTime, starttime);
                    double noteOnScreanDuration = easetime - (renderTime - fadeInTime);

                    double currentTime = fadeInTime;
                    float progress = 0;

                    Vector2 currentPosition = column.origin.getCurrentPosition(currentTime);


                    // TODO fix render in position beeing center of screen then initial receptor position
                    // IDK why this happens but it does happen prob some weird order issue.
                    note.invisible(fadeInTime - 1);
                    note.Move(currentTime - 1, 0, easing, currentPosition, currentPosition);
                    note.Render(renderTime, noteOnScreanDuration, easing);

                    for (int i = 0; i <= snaps; i++)
                    {
                        double timeLeft = easetime - snapLength * i; ;

                        Vector2 originPosition = column.origin.getCurrentPosition(currentTime);
                        Vector2 receptorPosition = column.receptor.getCurrentPositionForNotes(currentTime);
                        Vector2 newPosition = Vector2.Lerp(receptorPosition, originPosition, progress);
                        Vector2 originScale = column.origin.getCurrentScale(currentTime);
                        Vector2 receptorScale = column.receptor.getCurrentScale(currentTime);
                        Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, progress);
                        // Calculate the progress based on the remaining time
                        progress = (float)timeLeft / (float)easetime;

                        if (progress == 1)
                            continue;

                        note.Move(currentTime, snapLength, easing, currentPosition, newPosition);
                        note.Scale(currentTime, snapLength, easing, column.origin.getCurrentScale(currentTime), scaleProgress);

                        // Weird spinn in issues?
                        //if (note.getRotation(currentTime) != column.receptor.getCurrentRotaion(currentTime))
                        //note.AbsoluteRotate(currentTime, snapLength, easing, column.receptor.getCurrentRotaion(currentTime));

                        currentTime += snapLength;
                        currentPosition = newPosition;

                    }

                    if (note.isSlider)
                    {

                        foreach (Vector2WithTimestamp parts in note.sliderPositions)
                        {
                            double sliderStartime = parts.Timestamp;
                            OsbSprite sprite = parts.Sprite;
                            double sliderCurrentTime = sliderStartime - easetime;
                            Vector2 currentSliderPositon = column.origin.getCurrentPosition(sliderCurrentTime); ;
                            float sliderProgress = 0;

                            sprite.Move(sliderCurrentTime - 1, currentSliderPositon);

                            for (int i = 0; i <= snaps; i++)
                            {

                                double snapDuration = snapLength * i;
                                double timeLeft = easetime - snapLength * i; ;

                                Vector2 originPosition = column.origin.getCurrentPosition(sliderCurrentTime);
                                Vector2 receptorPosition = column.receptor.getCurrentPositionForNotes(sliderCurrentTime);
                                Vector2 newPosition = Vector2.Lerp(receptorPosition, originPosition, sliderProgress);
                                Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime);

                                //Vector2 originScale = column.origin.getCurrentScale(sliderCurrentTime);
                                //Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime);
                                //Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, sliderProgress);

                                // Calculate the progress based on the remaining time
                                sliderProgress = (float)timeLeft / (float)easetime;

                                if (i == snaps)
                                {
                                    note.Move(sliderCurrentTime, snapLength, easing, currentPosition, column.receptor.getCurrentPosition(sliderCurrentTime));
                                    note.Scale(sliderCurrentTime, snapLength, easing, receptorScale, receptorScale);
                                    currentPosition = column.receptor.getCurrentPosition(sliderCurrentTime);
                                }

                                // Weird spinn in issues?
                                //if (note.getRotation(currentTime) != column.receptor.getCurrentRotaion(currentTime))
                                //note.AbsoluteRotate(currentTime, snapLength, easing, column.receptor.getCurrentRotaion(currentTime));

                                if (sliderProgress == 1)
                                    continue;

                                sprite.Move(easing, sliderCurrentTime, sliderCurrentTime + snapLength, currentSliderPositon, newPosition);
                                sprite.ScaleVec(sliderCurrentTime, column.origin.getCurrentScale(sliderCurrentTime).X + 0.2f, 0.125f);

                                sliderCurrentTime += snapLength;
                                currentSliderPositon = newPosition;

                            }

                        }
                    }
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

        public double Zoom(int starttime, int duration, OsbEasing easing, double zoomAmount, Boolean keepPosition)
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
                    origin.MoveReceptor(starttime, originTarget, easing, duration);
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
                origin.MoveReceptor(starttime, originPosition, easing, duration);

                receptor.ScaleReceptor(starttime, absoluteScale, easing, duration);
                origin.ScaleReceptor(starttime, absoluteScale, easing, duration);
            }

            return debug;
        }
        public Vector2 calculatePlayFieldCenter(int currentTime)
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

        public void flipPlayField(int starttime, int duration, OsbEasing easing, float width, float height, float closeScale, float farScale)
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

            Vector2 center = calculatePlayFieldCenter(starttime);

            foreach (Column column in columns.Values)
            {

                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                Vector2 receptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 currentScale = receptor.getCurrentScale(starttime);

                float closeScaleDifference = closeScale / currentScale.X;
                float farScaleDifference = farScale / currentScale.X;
                // float xDifference = fareScale / currentScale.X;

                var xOffset = (receptorPosition.X - center.X) * closeScaleDifference - (receptorPosition.X - center.X);

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

                Vector2 newPosition = new Vector2(receptorPosition.X + xOffset, 240);
                Vector2 newOpposit = new Vector2(receptorPosition.X + xOffset, 240);

                Vector2 newPositionAfter = new Vector2(receptorPosition.X, newHeight);
                Vector2 newOppositAfter = new Vector2(receptorPosition.X, oppositHeight);

                receptor.MoveReceptor(starttime - 1, newPosition, easing, duration / 2);
                origin.MoveReceptor(starttime - 1, newOpposit, easing, duration / 2);

                receptor.MoveReceptor(starttime + duration / 2, newPositionAfter, easing, duration / 2);
                origin.MoveReceptor(starttime + duration / 2, newOppositAfter, easing, duration / 2);


                if (isFlipped)
                {
                    receptor.ScaleReceptor(starttime, new Vector2(currentScale.X * closeScaleDifference, currentScale.Y * closeScaleDifference), easing, duration / 2);
                    receptor.ScaleReceptor(starttime + duration / 2, new Vector2(currentScale.X, currentScale.Y), easing, duration / 2);

                    origin.ScaleReceptor(starttime, new Vector2(currentScale.X * farScaleDifference, currentScale.Y * farScaleDifference), easing, duration / 2);
                    origin.ScaleReceptor(starttime + duration / 2, new Vector2(currentScale.X, currentScale.Y), easing, duration / 2);
                }
                else
                {
                    receptor.ScaleReceptor(starttime, new Vector2(currentScale.X * farScaleDifference, currentScale.Y * farScaleDifference), easing, duration / 2);
                    receptor.ScaleReceptor(starttime + duration / 2, new Vector2(currentScale.X, currentScale.Y), easing, duration / 2);

                    origin.ScaleReceptor(starttime, new Vector2(currentScale.X * closeScaleDifference, currentScale.Y * closeScaleDifference), easing, duration / 2);
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

        public double moveFieldX(int starttime, int duration, OsbEasing easing, float amount)
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

        public double moveField(int starttime, int duration, OsbEasing easing, float amountX, float amountY)
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
                origin.MoveReceptor(starttime, currentOriginPosition, easing, duration);
            }

            return starttime + duration;
        }


        public double movePosition(int starttime, int duration, OsbEasing easing, Vector2 position)
        {
            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                Vector2 currentReceptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 currentOriginPosition = origin.getCurrentPosition(starttime);

                var difference = Vector2.Subtract(currentReceptorPosition, currentOriginPosition);

                receptor.MoveReceptor(starttime, position, easing, duration);
                origin.MoveReceptor(starttime, new Vector2(position.X, difference.Y), easing, duration);
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