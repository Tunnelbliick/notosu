using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using storyboard.scriptslibrary.maniaModCharts.effects;
using storyboard.scriptslibrary.maniaModCharts.utility;
using StorybrewCommon.Mapset;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding.CommandValues;

namespace StorybrewScripts
{

    public class Playfield : IDisposable
    {

        // Default SB height / width

        private float absoluteWidth = 640f;
        public float width = 250f;
        public float height = 480f;

        public float receptorHeightOffset = 0f;
        public float noteHeightOffset = 0f;

        private double rotation = 0f;

        private CommandScale receptorScale = new CommandScale(0.5);
        private string receptorSpritePath = "sb/sprites/receiver.png";

        public double starttime;
        public double endtime;

        public double noteStart;
        public double noteEnd;

        // Reference for active Columns;
        public Dictionary<ColumnType, Column> columns = new Dictionary<ColumnType, Column>();
        public Dictionary<double, EffectInfo> effectReferenceByStartTime = new Dictionary<double, EffectInfo>();

        // Notes Per Column
        public Dictionary<ColumnType, Dictionary<double, Note>> columnNotes = new Dictionary<ColumnType, Dictionary<double, Note>>();

        public Dictionary<double, FadeEffect> fadeAtTime = new Dictionary<double, FadeEffect>();

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Set managed objects to null
                    columns = null;
                    effectReferenceByStartTime = null;
                    columnNotes = null;
                }

                // Nullify any unmanaged resources here, if any

                disposed = true;
            }
        }

        ~Playfield()
        {
            Dispose(false);
        }

        public void initilizePlayField(StoryboardLayer receptors, StoryboardLayer notes, double starttime, double endtime, float receportWidth, float receptorHeightOffset, float noteHeightOffset)
        {
            this.starttime = starttime;
            this.endtime = endtime;

            this.noteStart = starttime;
            this.noteEnd = endtime;

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

        public void SetWidth(float width)
        {
            this.width = width;
        }

        public void SetHeight(float height)
        {
            this.height = height;
        }

        public void initializeNotes(List<OsuHitObject> objects, StoryboardLayer noteLayer, double bpm, double offset, double msPerPart = 30)
        {
            foreach (Column column in columns.Values)
            {

                column.setBPM(bpm, offset);

                Dictionary<double, Note> notes = new Dictionary<double, Note>();
                double xOffset = column.offset;

                foreach (OsuHitObject hitobject in objects)
                {
                    if (hitobject.Position.X != xOffset)
                        continue;

                    // Check for overlapping times
                    if (hitobject.StartTime <= noteEnd && hitobject.EndTime >= noteStart)
                    {
                        Note currentNote = new Note(noteLayer, hitobject, column, bpm, offset, msPerPart);
                        notes.Add(hitobject.StartTime, currentNote);
                    }
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

        public double ScalePlayFieldRelative(double starttime, double duration, OsbEasing easing, float width, float height)
        {

            this.width = width;
            this.height = height;

            float position = 0f;

            foreach (Column column in columns.Values)
            {

                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                var x = position + getColumnWidth() / 2;

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

        public double ScalePlayField(double starttime, double duration, OsbEasing easing, float width, float height)
        {
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

        public double ScaleOrigin(double starttime, double duration, OsbEasing easing, Vector2 scale, ColumnType type)
        {

            if (type == ColumnType.all)
            {
                foreach (Column column in columns.Values)
                {
                    column.origin.ScaleReceptor(starttime, scale, easing, duration);
                }
            }
            else
            {
                columns[type].origin.ScaleReceptor(starttime, scale, easing, duration);
            }
            return starttime + duration;
        }

        public double ScaleReceptor(double starttime, double duration, OsbEasing easing, Vector2 scale, ColumnType type)
        {

            if (type == ColumnType.all)
            {
                foreach (Column column in columns.Values)
                {
                    column.receptor.ScaleReceptor(starttime, scale, easing, duration);
                }
            }
            else
            {
                columns[type].receptor.ScaleReceptor(starttime, scale, easing, duration);
            }
            return starttime + duration;
        }


        public double MoveColumnRelative(double starttime, double duration, OsbEasing easing, Vector2 offset, ColumnType type)
        {

            if (type == ColumnType.all)
            {
                foreach (Column column in columns.Values)
                {
                    column.MoveColumnRelative(starttime, duration, offset, easing);
                }
            }
            else
            {
                columns[type].MoveColumnRelative(starttime, duration, offset, easing);
            }
            return starttime + duration;
        }

        public double MoveColumnRelativeX(double starttime, double duration, OsbEasing easing, double value, ColumnType type)
        {

            if (type == ColumnType.all)
            {
                foreach (Column column in columns.Values)
                {
                    column.MoveColumnRelativeX(starttime, duration, value, easing);
                }
            }
            else
            {
                columns[type].MoveColumnRelativeX(starttime, duration, value, easing);
            }
            return starttime + duration;
        }

        public double MoveColumnRelativeY(double starttime, double duration, OsbEasing easing, double value, ColumnType type)
        {

            if (type == ColumnType.all)
            {
                foreach (Column column in columns.Values)
                {
                    column.MoveColumnRelativeY(starttime, duration, value, easing);
                }
            }
            else
            {
                columns[type].MoveColumnRelativeY(starttime, duration, value, easing);
            }
            return starttime + duration;
        }

        public double Zoom(double starttime, double duration, OsbEasing easing, Vector2 newScale, bool keepPosition, CenterType centerType = CenterType.playfield)
        {
            double endtime = starttime + duration;

            Vector2 center = calculatePlayFieldCenter(starttime);

            if (centerType == CenterType.playfield)
            {
                center = calculatePlayFieldCenter(starttime);
            }
            else if (centerType == CenterType.middle)
            {
                center = new Vector2(320, 240);
            }
            else if (centerType == CenterType.receptor)
            {

                Vector2 mostLeft = new Vector2(0, 0);
                Vector2 mostRight = new Vector2(0, 0);

                foreach (Column column in columns.Values)
                {
                    Vector2 receptorPosition = column.receptor.getCurrentPosition(starttime);

                    // Check for most left position based on x-coordinate
                    if (receptorPosition.X < mostLeft.X)
                    {
                        mostLeft = receptorPosition;
                    }

                    // Check for most right position based on x-coordinate
                    if (receptorPosition.X > mostRight.X)
                    {
                        mostRight = receptorPosition;
                    }

                }

                // Calculate center between most left and most right receptors
                center = new Vector2(
                    320,
                    (mostLeft.Y + mostRight.Y) / 2
                );
            }

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                Vector2 receptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 originPosition = origin.getCurrentPosition(starttime);

                Vector2 receptorScale = receptor.getCurrentScale(starttime);
                Vector2 originScale = origin.getCurrentScale(starttime);

                receptor.ScaleReceptor(starttime, newScale, easing, duration);
                origin.ScaleReceptor(starttime, newScale, easing, duration);

                if (!keepPosition)
                {
                    // Directional vectors for the new coordinate system.
                    Vector2 newBaseX = new Vector2(1, 0); // always horizontal
                    Vector2 newBaseY = new Vector2(0, 1); // always vertical

                    // Calculate the change in distance for receptor based on scale difference.
                    float receptorDistanceChangeX = (receptorPosition.X - center.X) * (newScale.X / receptorScale.X - 1);
                    float receptorDistanceChangeY = (receptorPosition.Y - center.Y) * (newScale.Y / receptorScale.Y - 1);

                    // Adjust using the new coordinate system's basis.
                    Vector2 receptorMovement = receptorDistanceChangeX * newBaseX + receptorDistanceChangeY * newBaseY;

                    // Calculate the change in distance for origin based on scale difference.
                    float originDistanceChangeX = (originPosition.X - center.X) * (newScale.X / originScale.X - 1);
                    float originDistanceChangeY = (originPosition.Y - center.Y) * (newScale.Y / originScale.Y - 1);

                    Vector2 originMovement = new Vector2(originDistanceChangeX, originDistanceChangeY);

                    // Apply this movement to get the new position.
                    Vector2 zoomedReceptorPosition = receptorPosition + receptorMovement;
                    Vector2 zoomedOriginPosition = originPosition + originMovement;

                    receptor.MoveReceptor(starttime, zoomedReceptorPosition, easing, duration);
                    origin.MoveOrigin(starttime, zoomedOriginPosition, easing, duration);
                }

            }

            return endtime;
        }

        public enum CenterType
        {
            receptor,
            middle,
            playfield
        }

        public string ZoomAndMove(double starttime, double duration, OsbEasing easing, Vector2 newScale, Vector2 offset, CenterType centerType = CenterType.playfield)
        {
            double endtime = starttime + duration;
            string debug = "";

            Vector2 center = calculatePlayFieldCenter(starttime);

            if (centerType == CenterType.playfield)
            {
                center = calculatePlayFieldCenter(starttime);
            }
            else if (centerType == CenterType.middle)
            {
                center = new Vector2(320, 240);
            }
            else if (centerType == CenterType.receptor)
            {

                Vector2 mostLeft = new Vector2(0, 0);
                Vector2 mostRight = new Vector2(0, 0);

                foreach (Column column in columns.Values)
                {
                    Vector2 receptorPosition = column.receptor.getCurrentPosition(starttime);

                    // Check for most left position based on x-coordinate
                    if (receptorPosition.X < mostLeft.X)
                    {
                        mostLeft = receptorPosition;
                    }

                    // Check for most right position based on x-coordinate
                    if (receptorPosition.X > mostRight.X)
                    {
                        mostRight = receptorPosition;
                    }

                    // Optional: If you want to also consider y-coordinate for vertical positioning, you can add similar checks for y-coordinate here.
                }

                // Calculate center between most left and most right receptors
                center = new Vector2(
                    320,
                    (mostLeft.Y + mostRight.Y) / 2
                );
            }

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                Vector2 receptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 originPosition = origin.getCurrentPosition(starttime);

                Vector2 receptorScale = receptor.getCurrentScale(starttime);
                Vector2 originScale = origin.getCurrentScale(starttime);

                receptor.ScaleReceptor(starttime, newScale, easing, duration);
                origin.ScaleReceptor(starttime, newScale, easing, duration);

                // Scale the offset based on the scale difference
                Vector2 scaledReceptorOffset = offset;
                Vector2 scaledOriginOffset = offset;

                // Calculate the change in distance for receptor based on scale difference.
                float receptorDistanceChangeX = (receptorPosition.X - center.X) * (newScale.X / receptorScale.X - 1);
                float receptorDistanceChangeY = (receptorPosition.Y - center.Y) * (newScale.Y / receptorScale.Y - 1);

                Vector2 receptorMovement = new Vector2(receptorDistanceChangeX, receptorDistanceChangeY);

                // Calculate the change in distance for origin based on scale difference.
                float originDistanceChangeX = (originPosition.X - center.X) * (newScale.X / originScale.X - 1);
                float originDistanceChangeY = (originPosition.Y - center.Y) * (newScale.Y / originScale.Y - 1);

                Vector2 originMovement = new Vector2(originDistanceChangeX, originDistanceChangeY);

                // Apply the zoom and then the scaled offset.
                Vector2 zoomedReceptorPosition = receptorPosition + receptorMovement + scaledReceptorOffset;
                Vector2 zoomedOriginPosition = originPosition + originMovement + scaledOriginOffset;

                receptor.MoveReceptor(starttime, zoomedReceptorPosition, easing, duration);
                origin.MoveOrigin(starttime, zoomedOriginPosition, easing, duration);
            }



            return debug;
        }


        public String ZoomMoveAndRotate(double starttime, double duration, OsbEasing easing, Vector2 absoluteScale, Vector2 newPosition, double radians, int stepSize)
        {
            double endtime = starttime + duration;
            String debug = "";

            Vector2 center = calculatePlayFieldCenter(starttime);
            double stepRadians = radians / stepSize;
            double stepDuration = duration / stepSize;

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                Vector2 currentScale = receptor.getCurrentScale(starttime);
                Vector2 priorScale = currentScale;

                Vector2 currentReceptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 currentOriginPosition = origin.getCurrentPosition(starttime);

                for (int i = 0; i <= stepSize; i++)
                {
                    double currentTime = starttime + i * stepDuration;
                    double lerpFactor = (double)i / (stepSize - 1); // calculate how much of the movement we've completed
                    float xOffset = 0;
                    float yOffset = 0;

                    Vector2 moveCenter = Vector2.Lerp(center, newPosition, (float)lerpFactor);
                    Vector2 scaleProgress = Vector2.Lerp(currentScale, absoluteScale, (float)lerpFactor);
                    Vector2 scaleDifference = new Vector2(scaleProgress.X / priorScale.X, scaleProgress.Y / priorScale.Y);

                    if (absoluteScale != currentScale)
                    {
                        // This needs to be calculated differently to match scale changes above 1 since 1 doesnt change anything but the sprite scale changes with 1
                        xOffset = (currentReceptorPosition.X - moveCenter.X) * scaleDifference.X - (currentReceptorPosition.X - moveCenter.X);
                        yOffset = (currentOriginPosition.Y - moveCenter.Y) * scaleDifference.Y - (currentOriginPosition.Y - moveCenter.Y);
                    }

                    currentReceptorPosition.X += xOffset;
                    currentOriginPosition.X += xOffset;

                    currentReceptorPosition.Y += yOffset;
                    currentOriginPosition.Y += yOffset;

                    Vector2 rotatedReceptorPosition = Utility.PivotPoint(currentReceptorPosition, moveCenter, stepRadians);
                    Vector2 rotatedOriginPosition = Utility.PivotPoint(currentOriginPosition, moveCenter, stepRadians);

                    receptor.MoveReceptor(currentTime, rotatedReceptorPosition, easing, stepDuration);
                    origin.MoveOrigin(currentTime, rotatedOriginPosition, easing, stepDuration);

                    priorScale = scaleProgress;
                    currentReceptorPosition = rotatedReceptorPosition;
                    currentOriginPosition = rotatedOriginPosition;

                }

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

        public double MoveReceptorAbsolute(double starttime, double duration, OsbEasing easing, Vector2 position, ColumnType column)
        {
            if (column == ColumnType.all)
            {
                foreach (Column currentColumn in columns.Values)
                {
                    currentColumn.MoveReceptor(starttime, duration, position, easing);
                }
            }
            else
            {
                Column currentColumn = columns[column];

                currentColumn.MoveReceptor(starttime, duration, position, easing);
            }

            return starttime + duration;

        }

        public void MoveReceptorRelative(double starttime, double duration, OsbEasing easing, Vector2 position, ColumnType column)
        {
            if (column == ColumnType.all)
            {
                foreach (Column currentColumn in columns.Values)
                {
                    Vector2 currentPosition = currentColumn.getReceptorPosition(starttime);

                    currentColumn.MoveReceptorRelative(starttime, duration, position, easing);
                }
            }
            else
            {
                Column currentColumn = columns[column];

                Vector2 currentPosition = currentColumn.getReceptorPosition(starttime);

                currentColumn.MoveReceptorRelative(starttime, duration, position, easing);

            }

        }

        public double RotateReceptorRelative(double starttime, double duration, OsbEasing easing, double rotation, ColumnType column)
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

        public void MoveOriginRelative(double starttime, double duration, OsbEasing easing, Vector2 position, ColumnType column)
        {
            if (column == ColumnType.all)
            {
                foreach (Column currentColumn in columns.Values)
                {
                    Vector2 currentPosition = currentColumn.getOriginPosition(starttime);

                    currentColumn.MoveOrigin(starttime, duration, Vector2.Add(currentPosition, position), easing);
                }
            }
            else
            {
                Column currentColumn = columns[column];

                Vector2 currentPosition = currentColumn.getOriginPosition(starttime);

                currentColumn.MoveOrigin(starttime, duration, Vector2.Add(currentPosition, position), easing);

            }

        }

        public double MoveOriginAbsolute(double starttime, double duration, OsbEasing easing, Vector2 position, ColumnType column)
        {

            if (column == ColumnType.all)
            {
                foreach (Column currentColumn in columns.Values)
                {
                    currentColumn.MoveOrigin(starttime, duration, position, easing);
                }
            }
            else
            {

                Column currentColumn = columns[column];

                currentColumn.MoveOrigin(starttime, duration, position, easing);
            }
            return starttime + duration;

        }

        // This will rotate the Playfield but keep the Receptors in a static position
        public void RotatePlayFieldStatic(double starttime, double duration, OsbEasing easing, double radians)
        {

            this.rotation = radians;

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;

                receptor.RotateReceptor(starttime, duration, easing, radians);
            }


        }

        // This will rotate the Playfield but move the Receptors dynamically to adjust for the position
        public void RotatePlayField(double starttime, double duration, OsbEasing easing, double radians, int sampleCount, CenterType centerType = CenterType.middle)
        {

            var center = new Vector2(320, 240);

            if (centerType == CenterType.playfield)
            {
                center = calculatePlayFieldCenter(starttime);
            }

            double newRotation = this.rotation + radians;

            this.rotation = newRotation;

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                receptor.PivotReceptor(starttime, radians, easing, duration, sampleCount, center);
                origin.PivotReceptor(starttime, radians, easing, duration, sampleCount, center);
            }

        }

        public void RotateAndRescalePlayField(double starttime, double duration, OsbEasing easing, double radians, int sampleCount, double targetDistanceReceptor, CenterType centerType = CenterType.middle)
        {
            var center = new Vector2(320, 240);

            if (centerType == CenterType.playfield)
            {
                center = calculatePlayFieldCenter(starttime);
            }

            double newRotation = this.rotation + radians;

            this.rotation = newRotation;

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                // Pivot and Rescale receptor and origin
                receptor.PivotAndRescaleReceptor(starttime, radians, easing, duration, sampleCount, center, targetDistanceReceptor);
                origin.PivotAndRescaleReceptor(starttime, radians, easing, duration, sampleCount, center, targetDistanceReceptor);
            }
        }

        public void bounceLeftRight(int starttime, int duration, OsbEasing easing, float amount)
        {

            int half = duration / 2;

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

        public double moveFieldAbsolute(double starttime, double duration, OsbEasing easing, Vector2 newCenter)
        {

            Vector2 center = calculatePlayFieldCenter(starttime);
            Vector2 difference = new Vector2(newCenter.X - center.X, newCenter.Y - center.Y);

            foreach (Column column in columns.Values)
            {
                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                Vector2 currentReceptorPosition = receptor.getCurrentPosition(starttime) + difference;
                Vector2 currentOriginPosition = origin.getCurrentPosition(starttime) + difference;

                receptor.MoveReceptor(starttime, currentReceptorPosition, easing, duration);
                origin.MoveOrigin(starttime, currentOriginPosition, easing, duration);
            }

            return starttime + duration;
        }

        public void addEffect(double starttime, double endtime, EffectType type, string reference)
        {

            EffectInfo info = new EffectInfo(starttime, endtime, type, reference);

            this.effectReferenceByStartTime.Add(starttime, info);

        }

        public void fadeAt(double time, float fade)
        {
            FadeEffect effect = new FadeEffect(time, time, OsbEasing.None, fade);
            this.fadeAtTime.Add(time, effect);
        }

        public void fadeAt(double time, double endtime, float fade)
        {
            FadeEffect effect = new FadeEffect(time, endtime, OsbEasing.None, fade);
            this.fadeAtTime.Add(time, effect);
        }

        public void fadeAt(double time, double endtime, OsbEasing easing, float fade)
        {
            FadeEffect effect = new FadeEffect(time, endtime, easing, fade);
            this.fadeAtTime.Add(time, effect);
        }

        public float findFadeAtTime(double time)
        {

            KeyValuePair<double, FadeEffect> currentFadeEffect = this.fadeAtTime
                   .Where(kvp => kvp.Key <= time)
                   .OrderByDescending(kvp => kvp.Key)
                   .FirstOrDefault();

            return currentFadeEffect.Value.value;
        }

        public float getColumnWidth()
        {
            return this.width / 4;
        }

        public float calculateOffset()
        {
            return (absoluteWidth - width) / 2;
        }

        public String executeKeyFrames()
        {
            String debug = "";
            foreach (Column column in columns.Values)
            {
                List<Operation> test = column.executeKeyFrames();

                if (column.type == ColumnType.one)
                {

                    foreach (var op in test)
                    {
                        Vector2 pos = (CommandPosition)op.value;
                        debug += $"Start: {op.starttime}, End: {op.endtime}, Type: {op.type}, Value: ({pos.X}, {pos.Y})\n";
                    }
                }
            }

            return debug;
        }
    }
}