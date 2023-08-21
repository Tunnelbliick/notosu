using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{
    public class DrawInstance
    {

        public double starttime = 0;
        public double easetime = 0;
        public OsbEasing easing;
        public Playfield playfieldInstance;
        public double updatesPerSecond = 2;
        public bool rotateToFaceReceptor = true;
        public Dictionary<ColumnType, List<Anchor>> notePathByColumn = new Dictionary<ColumnType, List<Anchor>>();

        public DrawInstance InitializeDrawInstance(Playfield playfieldInstance, double starttime, double easetime, double updatesPerSecond, OsbEasing easing, bool rotateToFaceReceptor)
        {

            this.starttime = starttime;
            this.easetime = easetime;
            this.easing = easing;
            this.playfieldInstance = playfieldInstance;
            this.updatesPerSecond = updatesPerSecond;
            this.rotateToFaceReceptor = rotateToFaceReceptor;

            return this;

        }

        public DrawInstance(Playfield playfieldInstance, double starttime, double easetime, double updatesPerSecond, OsbEasing easing, bool rotateToFaceReceptor)
        {

            this.starttime = starttime;
            this.easetime = easetime;
            this.easing = easing;
            this.playfieldInstance = playfieldInstance;
            this.updatesPerSecond = updatesPerSecond;
            this.rotateToFaceReceptor = rotateToFaceReceptor;

        }

        public double drawNotesDefault(double duration)
        {

            double endtime = starttime + duration;

            foreach (Column column in playfieldInstance.columns.Values)
            {
                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];

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

            return endtime;
        }

        public double drawNotesByEndPosition(double duration)
        {

            double endtime = starttime + duration;

            foreach (Column column in playfieldInstance.columns.Values)
            {
                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];

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

            return endtime;
        }

        public double drawNotesBySnapshotPosition(double duration)
        {

            double endtime = starttime + duration;
            int snapShots = (int)(duration / 1000 * updatesPerSecond);
            double snapLength = easetime / snapShots;

            foreach (Column column in playfieldInstance.columns.Values)
            {
                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];

                // Get only the keys (hittimes) that fall within the specified range considering easetime
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime <= endtime).ToList();

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];
                    double noteTime = note.starttime;
                    double fadeInTime = noteTime - easetime;

                    double travelDistance = Math.Abs(column.origin.getCurrentPosition(fadeInTime).Y - column.receptor.getCurrentPosition(fadeInTime).Y);
                    double distancePerSnap = travelDistance / snapShots;
                    bool moveUpwards = column.origin.getCurrentPosition(fadeInTime).Y > column.receptor.getCurrentPosition(fadeInTime).Y;

                    Vector2 originPosition = column.origin.getCurrentPosition(fadeInTime);
                    Vector2 receptorPosition = column.receptor.getCurrentPositionForNotes(fadeInTime);

                    double currentTime = fadeInTime;

                    note.Render(fadeInTime, easetime, easing);

                    for (int i = 0; i <= snapShots; i++)
                    {

                        double snapDuration = snapLength * i;

                        Vector2 currentPosition = column.receptor.getCurrentPosition(currentTime + snapDuration);
                        double newYPosition = moveUpwards ? (originPosition.Y - i * distancePerSnap) : (originPosition.Y + i * distancePerSnap);

                        Vector2 newPosition = new Vector2(currentPosition.X, (float)newYPosition);

                        note.Move(currentTime, snapDuration, easing, originPosition, newPosition);

                        currentTime += snapDuration;
                        originPosition = newPosition;
                    }
                    note.Scale(fadeInTime, easetime, easing, column.receptor.getCurrentScale(note.starttime), column.receptor.getCurrentScale(note.starttime));
                }
            }

            return endtime;
        }

        public double drawNotesByOriginToReceptor(double duration)
        {
            double endtime = starttime + duration;
            int snapShots = (int)(duration / 1000 * updatesPerSecond);
            // This will guarantee that the total time of all snaps is exactly easetime
            double snapLength = easetime / (snapShots + 0f);


            foreach (Column column in playfieldInstance.columns.Values)
            {
                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];

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
                    note.Render(renderTime, noteOnScreanDuration, easing, 10);
                    double startRotation = note.getRotation(currentTime);

                    for (int i = 0; i < snapShots; i++)
                    {
                        double timeLeft = easetime - snapLength * i;

                        // Calculate the progress based on the remaining time
                        progress = (float)i / (snapShots - 1);

                        Vector2 originPosition = column.origin.getCurrentPosition(currentTime);
                        Vector2 receptorPosition = column.receptor.getCurrentPositionForNotes(currentTime);
                        Vector2 newPosition = Vector2.Lerp(originPosition, receptorPosition, progress);
                        Vector2 originScale = column.origin.getCurrentScale(currentTime);
                        Vector2 receptorScale = column.receptor.getCurrentScale(currentTime);
                        Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, progress);

                        double theta = 0;

                        if (progress < 0.15 && rotateToFaceReceptor)
                        {
                            Vector2 delta = receptorPosition - currentPosition;

                            // Check relative vertical positions
                            if (currentPosition.Y > receptorPosition.Y)
                            {
                                // If the receptor is above the origin, reverse the direction
                                delta = -delta;
                            }


                            theta = Math.Atan2(delta.X, delta.Y);
                        }

                        note.Move(currentTime, snapLength, easing, currentPosition, newPosition);
                        note.Scale(currentTime, snapLength, easing, column.origin.getCurrentScale(currentTime), scaleProgress);
                        note.AbsoluteRotate(currentTime, snapLength, easing, startRotation - theta);

                        // Weird spinn in issues?
                        //if (note.getRotation(currentTime) != column.receptor.getCurrentRotaion(currentTime))
                        //note.AbsoluteRotate(currentTime, snapLength, easing, column.receptor.getCurrentRotaion(currentTime));

                        currentTime += snapLength;
                        currentPosition = newPosition;

                    }

                    foreach (SliderParts parts in note.sliderPositions)
                    {
                        double sliderStartime = parts.Timestamp;
                        OsbSprite sprite = parts.Sprite;
                        double sliderCurrentTime = sliderStartime - easetime;
                        Vector2 currentSliderPositon = column.origin.getCurrentPosition(sliderCurrentTime); ;
                        float sliderProgress = 0;

                        sprite.Move(sliderCurrentTime - 1, currentSliderPositon);
                        double sliderRotation = sprite.RotationAt(sliderCurrentTime);

                        for (int i = 0; i < snapShots; i++)
                        {
                            double snapDuration = snapLength * i;
                            double timeLeft = easetime - snapLength * i;

                            sliderProgress = (float)i / (snapShots - 1);

                            Vector2 originPosition = column.origin.getCurrentPosition(sliderCurrentTime);
                            Vector2 receptorPosition = column.receptor.getCurrentPositionForNotes(sliderCurrentTime);
                            Vector2 newPosition = Vector2.Lerp(originPosition, receptorPosition, sliderProgress);
                            Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime);

                            //Vector2 originScale = column.origin.getCurrentScale(sliderCurrentTime);
                            //Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime);
                            //Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, sliderProgress);

                            double theta = 0;

                            Vector2 delta = receptorPosition - currentSliderPositon;

                            // Check relative vertical positions
                            if (currentSliderPositon.Y > receptorPosition.Y)
                            {
                                // If the receptor is above the origin, reverse the direction
                                delta = -delta;
                            }

                            theta = Math.Atan2(delta.X, delta.Y);

                            if (i == snapShots - 1)
                            {
                                note.Move(sliderCurrentTime, snapLength, easing, currentPosition, column.receptor.getCurrentPosition(sliderCurrentTime));
                                note.Scale(sliderCurrentTime, snapLength, easing, receptorScale, receptorScale);
                                currentPosition = column.receptor.getCurrentPosition(sliderCurrentTime);
                            }

                            // Weird spinn in issues?
                            //if (note.getRotation(currentTime) != column.receptor.getCurrentRotaion(currentTime))
                            //note.AbsoluteRotate(currentTime, snapLength, easing, column.receptor.getCurrentRotaion(currentTime));

                            sprite.Move(easing, sliderCurrentTime, sliderCurrentTime + snapLength, currentSliderPositon, newPosition);
                            sprite.ScaleVec(sliderCurrentTime, 0.7f / 0.5f * column.origin.getCurrentScale(sliderCurrentTime).X, 0.16f / 0.5f * column.origin.getCurrentScale(sliderCurrentTime).Y);
                            sprite.Rotate(easing, sliderCurrentTime, sliderCurrentTime + snapLength, sprite.RotationAt(sliderCurrentTime), sliderRotation - theta);

                            sliderCurrentTime += snapLength;
                            currentSliderPositon = newPosition;

                        }

                    }
                }
            }

            return endtime;
        }

        public double drawNotesByAnchors(double duration, string type = "line")
        {
            double endtime = starttime + duration;
            int snapShots = (int)(duration / 1000 * updatesPerSecond);
            double snapLength = easetime / snapShots;

            foreach (Column column in playfieldInstance.columns.Values)
            {
                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime <= endtime).ToList();

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];
                    List<Anchor> notePath = this.notePathByColumn[column.type];

                    double noteTime = note.starttime;
                    double fadeInTime = noteTime - easetime;
                    double renderTime = Math.Max(fadeInTime, starttime);
                    double noteOnScreanDuration = easetime - (renderTime - fadeInTime);

                    double currentTime = fadeInTime;
                    float totalProgress = 0;

                    Vector2 currentPosition = notePath[0].sprite.PositionAt(currentTime);

                    note.invisible(fadeInTime - 1);
                    note.Move(currentTime - 1, 0, easing, currentPosition, currentPosition);
                    note.Render(renderTime, noteOnScreanDuration, easing);
                    double startRotation = note.getRotation(currentTime);

                    // Check for anchor path type and adjust note's position accordingly
                    if (type == "line")
                    {
                        // Use direct lines between anchors
                        // Simplified version for understanding:
                        int snapShotsPerAnchor = snapShots / (notePath.Count - 1);
                        for (int n = 0; n < notePath.Count - 1; n++)
                        {
                            for (int i = 0; i < snapShotsPerAnchor; i++)
                            {

                                double totalTimeLeft = easetime - snapLength * i;
                                totalProgress = (float)totalTimeLeft / (float)easetime;

                                float progress = i * 1.0f / snapShotsPerAnchor; //(float) timePerAnchor * anchorIndex / (float) snapLength * i;

                                Vector2 startPos = notePath[n].sprite.PositionAt(currentTime);
                                Vector2 endPos = notePath[n + 1].sprite.PositionAt(currentTime);

                                Vector2 receptorPosition = column.receptor.getCurrentPosition(currentTime);
                                Vector2 newPosition = Vector2.Lerp(startPos, endPos, progress);
                                Vector2 originScale = column.origin.getCurrentScale(currentTime);
                                Vector2 receptorScale = column.receptor.getCurrentScale(currentTime);
                                Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, progress);
                                // Calculate the progress based on the remaining time

                                double theta = 0;

                                if (totalProgress > 0.15 && rotateToFaceReceptor)
                                {
                                    Vector2 delta = receptorPosition - currentPosition;

                                    // Check relative vertical positions
                                    if (currentPosition.Y > receptorPosition.Y)
                                    {
                                        // If the receptor is above the origin, reverse the direction
                                        delta = -delta;
                                    }

                                    theta = Math.Atan2(delta.X, delta.Y);
                                }

                                note.Move(currentTime, snapLength, easing, currentPosition, newPosition);
                                note.Scale(currentTime, snapLength, easing, column.origin.getCurrentScale(currentTime), scaleProgress);
                                note.AbsoluteRotate(currentTime, snapLength, easing, startRotation - theta);

                                currentTime += snapLength;
                                currentPosition = newPosition;
                            }
                        }

                        // Draw Slider body
                        List<SliderParts> reversedSliderPoints = note.sliderPositions.ToList();
                        reversedSliderPoints.Reverse();
                        foreach (SliderParts parts in reversedSliderPoints)
                        {
                            double sliderStartime = parts.Timestamp;
                            OsbSprite sprite = parts.Sprite;
                            double sliderCurrentTime = sliderStartime - easetime;
                            Vector2 currentSliderPositon = column.origin.getCurrentPosition(sliderCurrentTime); ;
                            float sliderProgress = 0;

                            sprite.Move(sliderCurrentTime - 1, currentSliderPositon);
                            double sliderRotation = sprite.RotationAt(sliderCurrentTime);

                            for (int n = 0; n < notePath.Count - 1; n++)
                            {
                                for (int i = 0; i < snapShotsPerAnchor; i++)
                                {

                                    double snapDuration = snapLength * i;
                                    double timeLeft = easetime - snapLength * i;

                                    Vector2 startPos = notePath[n].sprite.PositionAt(sliderCurrentTime);
                                    Vector2 endPos = notePath[n + 1].sprite.PositionAt(sliderCurrentTime);

                                    // Calculate the progress based on the remaining time
                                    sliderProgress = i * 1f / snapShotsPerAnchor;

                                    Vector2 receptorPosition = column.receptor.getCurrentPosition(sliderCurrentTime);
                                    Vector2 newPosition = Vector2.Lerp(startPos, endPos, sliderProgress);
                                    Vector2 originScale = column.origin.getCurrentScale(sliderCurrentTime);
                                    Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime);
                                    Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, sliderProgress);

                                    double theta = 0;

                                    Vector2 delta = endPos - currentSliderPositon;

                                    // Check relative vertical positions
                                    if (currentSliderPositon.Y > receptorPosition.Y)
                                    {
                                        // If the receptor is above the origin, reverse the direction
                                        delta = -delta;
                                    }

                                    theta = Math.Atan2(delta.X, delta.Y);

                                    if (i == snapShotsPerAnchor)
                                    {
                                        note.Move(sliderCurrentTime, snapLength, easing, currentPosition, column.receptor.getCurrentPosition(sliderCurrentTime));
                                        note.Scale(sliderCurrentTime, snapLength, easing, receptorScale, receptorScale);
                                        currentPosition = column.receptor.getCurrentPosition(sliderCurrentTime);
                                    }

                                    sprite.Move(easing, sliderCurrentTime, sliderCurrentTime + snapLength, currentSliderPositon, newPosition);
                                    sprite.ScaleVec(sliderCurrentTime, column.origin.getCurrentScale(sliderCurrentTime).X + 0.2f, 0.1525f);
                                    sprite.Rotate(easing, sliderCurrentTime, sliderCurrentTime + snapLength, sprite.RotationAt(sliderCurrentTime), sliderRotation - theta);

                                    sliderCurrentTime += snapLength;
                                    currentSliderPositon = newPosition;

                                }
                            }
                        }
                    }
                    else if (type == "bezier")
                    {
                        // Use bezier curve calculations with the anchors as control points
                        // You'd typically use a bezier library or implement the bezier calculations yourself

                        // Simplified version for understanding:
                        for (int i = 0; i < snapShots; i++)
                        {
                            double timeLeft = easetime - snapLength * i;
                            Vector2 receptorPosition = column.getReceptorPositionForNotes(currentTime);
                            Vector2 originScale = column.origin.getCurrentScale(currentTime);
                            Vector2 receptorScale = column.receptor.getCurrentScale(currentTime);
                            Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, totalProgress);

                            List<Vector2> points = new List<Vector2>();

                            foreach (Anchor noteAnchor in notePath)
                            {
                                // points.Add(noteAnchor.sprite.PositionAt(currentTime));
                                points.Add(noteAnchor.getPositionAt(currentTime));
                            }

                            totalProgress = i / (snapShots - 1.0f);

                            Vector2 newPosition = BezierCurve.CalculatePoint(points, totalProgress);

                            double theta = 0;

                            if (totalProgress > 0.15 && rotateToFaceReceptor)
                            {
                                Vector2 delta = receptorPosition - currentPosition;

                                // Check relative vertical positions
                                if (currentPosition.Y > receptorPosition.Y)
                                {
                                    // If the receptor is above the origin, reverse the direction
                                    delta = -delta;
                                }

                                theta = Math.Atan2(delta.X, delta.Y);
                            }

                            note.Move(currentTime, snapLength, easing, currentPosition, newPosition);
                            note.Scale(currentTime, snapLength, easing, column.origin.getCurrentScale(currentTime), scaleProgress);
                            note.AbsoluteRotate(currentTime, snapLength, easing, startRotation - theta);

                            // Weird spinn in issues?
                            //if (note.getRotation(currentTime) != column.receptor.getCurrentRotaion(currentTime))
                            //note.AbsoluteRotate(currentTime, snapLength, easing, column.receptor.getCurrentRotaion(currentTime));

                            currentTime += snapLength;
                            currentPosition = newPosition;
                            // Your logic to move the note to position
                        }

                        // draw slider body
                        List<SliderParts> reversedSliderPoints = note.sliderPositions.ToList();
                        reversedSliderPoints.Reverse();
                        foreach (SliderParts parts in reversedSliderPoints)
                        {
                            double sliderStartime = parts.Timestamp;
                            OsbSprite sprite = parts.Sprite;
                            double sliderCurrentTime = sliderStartime - easetime;
                            Vector2 currentSliderPositon = notePath[0].sprite.PositionAt(sliderCurrentTime);
                            float sliderProgress = 0;

                            sprite.Move(sliderCurrentTime - 1, currentSliderPositon);

                            for (int i = 0; i < snapShots; i++)
                            {

                                double sliderRotation = sprite.RotationAt(sliderCurrentTime);
                                List<Vector2> points = new List<Vector2>();

                                foreach (Anchor noteAnchor in notePath)
                                {
                                    // points.Add(noteAnchor.sprite.PositionAt(sliderCurrentTime));
                                    points.Add(noteAnchor.getPositionAt(sliderCurrentTime));
                                }

                                // Calculate the progress based on the remaining time
                                sliderProgress = i / (snapShots - 1.0f);

                                Vector2 receptorPosition = column.getReceptorPositionForNotes(sliderCurrentTime);
                                Vector2 originScale = column.origin.getCurrentScale(sliderCurrentTime);
                                Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime);
                                Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, sliderProgress);

                                Vector2 newPosition = BezierCurve.CalculatePoint(points, sliderProgress);
                                Vector2 nextPosition = BezierCurve.CalculatePoint(points, Math.Min(1, (i + 1) / (snapShots - 1.0f)));

                                double theta = 0;

                                Vector2 delta = newPosition - nextPosition;

                                theta = Math.Atan2(delta.X, delta.Y);

                                if (i == snapShots - 1 && sliderCurrentTime > note.starttime)
                                {
                                    note.Move(sliderCurrentTime, snapLength, easing, currentPosition, column.receptor.getCurrentPosition(sliderCurrentTime));
                                    note.Scale(sliderCurrentTime, snapLength, easing, receptorScale, receptorScale);
                                    currentPosition = column.receptor.getCurrentPosition(sliderCurrentTime);
                                }

                                sprite.Move(easing, sliderCurrentTime, sliderCurrentTime + snapLength, currentSliderPositon, newPosition);
                                sprite.ScaleVec(sliderCurrentTime, column.origin.getCurrentScale(sliderCurrentTime).X + 0.25f, 0.1525f);
                                sprite.Rotate(sliderCurrentTime, -theta);

                                sliderCurrentTime += snapLength;
                                currentSliderPositon = newPosition;

                            }
                        }
                    }

                    // Rest of the note logic remains the same...
                }
            }

            return endtime;
        }


        public void addAnchor(ColumnType column, Vector2 position, bool debug, StoryboardLayer debugLayer)
        {

            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    if (notePathByColumn.ContainsKey(currentColumn) == false)
                    {

                        notePathByColumn.Add(currentColumn, new List<Anchor>());

                    }

                    List<Anchor> notePath = notePathByColumn[currentColumn];

                    //if debug add a sprite for the position of the vector
                    Anchor pathPoint = new Anchor(0, currentColumn, position, position, debug, debugLayer);
                    notePath.Add(pathPoint);

                    notePathByColumn[currentColumn] = notePath;

                }
            }
            else
            {

                if (notePathByColumn.ContainsKey(column) == false)
                {

                    notePathByColumn.Add(column, new List<Anchor>());

                }

                List<Anchor> notePath = notePathByColumn[column];

                //if debug add a sprite for the position of the vector
                Anchor pathPoint = new Anchor(0, column, position, position, debug, debugLayer);
                notePath.Add(pathPoint);

                notePathByColumn[column] = notePath;
            }
        }

        // This adds an anchor relative to the current position of the column
        public String addRelativeAnchor(ColumnType column, double starttime, Vector2 relativeOffset, bool debug, StoryboardLayer debugLayer)
        {

            String debugstring = "";

            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumnType in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumnType == ColumnType.all)
                        continue;

                    debugstring = addRelative(currentColumnType, starttime, relativeOffset, debug, debugLayer);
                }
            }
            else
            {
                debugstring = addRelative(column, starttime, relativeOffset, debug, debugLayer);
            }

            return debugstring;
        }

        public String addRelativeAnchorList(ColumnType column, double starttime, List<Vector2> relativeOffset, bool debug, StoryboardLayer debugLayer)
        {

            String debugstring = "";

            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumnType in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumnType == ColumnType.all)
                        continue;

                    debugstring = addListRelative(currentColumnType, starttime, relativeOffset, debug, debugLayer);
                }
            }
            else
            {
                debugstring = addListRelative(column, starttime, relativeOffset, debug, debugLayer);
            }

            return debugstring;
        }

        private String addRelative(ColumnType column, double starttime, Vector2 relativeOffset, bool debug, StoryboardLayer debugLayer)
        {

            String debugString = "";

            if (this.notePathByColumn.ContainsKey(column) == false)
            {
                this.notePathByColumn.Add(column, new List<Anchor>());
            }

            List<Anchor> notePath = this.notePathByColumn[column];
            Column currentColumn = this.playfieldInstance.columns[column];
            Vector2 originPosition = currentColumn.getOriginPosition(starttime);
            Vector2 receptorPosition = currentColumn.getReceptorPositionForNotes(starttime);
            int index = 0;

            float blend = 1.0f / (notePath.Count + 1);

            foreach (Anchor noteAnchor in notePath)
            {

                float currentBlend = blend * index;
                debugString += $"{blend}, {index}, {currentBlend}, {notePath.Count}\n";

                Vector2 offsetPosition = noteAnchor.position;
                Vector2 lerpPosition = Vector2.Lerp(originPosition, receptorPosition, currentBlend);
                offsetPosition.Y = lerpPosition.Y;

                index++;

                noteAnchor.MoveAnchor(starttime, offsetPosition);

            }

            Vector2 lerpPositionForNewAnchor = Vector2.Lerp(originPosition, receptorPosition, 1);
            Vector2 offsetPositionForNewAnchor = Vector2.Add(lerpPositionForNewAnchor, relativeOffset);

            //if debug add a sprite for the position of the vector
            Anchor pathPoint = new Anchor(0, column, offsetPositionForNewAnchor, relativeOffset, debug, debugLayer);
            notePath.Add(pathPoint);

            this.notePathByColumn[column] = notePath;

            return debugString;
        }

        private String addListRelative(ColumnType column, double starttime, List<Vector2> relativeOffsets, bool debug, StoryboardLayer debugLayer)
        {

            String debugString = "";

            if (this.notePathByColumn.ContainsKey(column) == false)
            {
                this.notePathByColumn.Add(column, new List<Anchor>());
            }

            List<Anchor> notePath = this.notePathByColumn[column];
            Column currentColumn = this.playfieldInstance.columns[column];
            Vector2 originPosition = currentColumn.getOriginPosition(starttime);
            Vector2 receptorPosition = currentColumn.getReceptorPositionForNotes(starttime);
            int index = 0;

            float blend = 1.0f / (notePath.Count + relativeOffsets.Count - 1);

            foreach (Anchor noteAnchor in notePath)
            {

                float currentBlend = blend * index;
                debugString += $"{blend}, {index}, {currentBlend}, {notePath.Count}\n";

                Vector2 offsetPosition = noteAnchor.position;
                Vector2 lerpPosition = Vector2.Lerp(originPosition, receptorPosition, currentBlend);
                offsetPosition.Y = lerpPosition.Y;

                index++;

                noteAnchor.MoveAnchor(starttime, offsetPosition);

            }

            foreach (Vector2 offset in relativeOffsets)
            {

                float currentBlend = blend * index;

                Vector2 lerpPositionForNewAnchor = Vector2.Lerp(originPosition, receptorPosition, currentBlend);
                Vector2 offsetPositionForNewAnchor = Vector2.Add(lerpPositionForNewAnchor, offset);

                //if debug add a sprite for the position of the vector
                Anchor pathPoint = new Anchor(0, column, offsetPositionForNewAnchor, offset, debug, debugLayer);
                notePath.Add(pathPoint);

                index++;
            }

            this.notePathByColumn[column] = notePath;

            return debugString;
        }

        public void updateAnchors(double starttime, double duration, ColumnType column, double updatesPerSecond)
        {

            int precision = (int)(duration / 1000 * updatesPerSecond);
            double instanceLength = duration / precision;

            for (int i = 0; i <= precision; i++)
            {
                double currentTime = starttime + instanceLength * i;

                if (column == ColumnType.all)
                {

                    foreach (ColumnType type in Enum.GetValues(typeof(ColumnType)))
                    {


                        if (type == ColumnType.all)
                            continue;

                        List<Anchor> notePath = this.notePathByColumn[type];
                        Column currentColumn = this.playfieldInstance.columns[type];
                        Vector2 originPosition = currentColumn.getOriginPosition(currentTime);
                        Vector2 receptorPosition = currentColumn.getReceptorPositionForNotes(currentTime);

                        int index = 0;
                        float blend = 1.0f / (notePath.Count - 1);

                        Vector2 direction = Vector2.Normalize(receptorPosition - originPosition); // Direction from origin to receptor
                        Vector2 perpendicular = new Vector2(-direction.Y, direction.X); // Perpendicular to the direction

                        foreach (Anchor noteAnchor in notePath)
                        {
                            float currentBlend = blend * index;

                            // Determine the position along the path
                            Vector2 pathPosition = Vector2.Lerp(originPosition, receptorPosition, currentBlend);

                            // Apply the offset relative to the path's direction
                            Vector2 offsetPosition = noteAnchor.offset;
                            Vector2 adjustedPosition = pathPosition - (offsetPosition.X * perpendicular) + (offsetPosition.Y * direction);

                            noteAnchor.ManipulatePosition(currentTime, instanceLength, OsbEasing.None, adjustedPosition);

                            index++;
                        }
                    }
                }

            }

        }


        public void manipulateAnchor(int index, ColumnType column, double starttime, double transitionTime, Vector2 newPosition, OsbEasing easing)
        {

            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    List<Anchor> notePath = notePathByColumn[currentColumn];

                    Anchor pathPoint = notePath[index];

                    pathPoint = pathPoint.ManipulatePosition(starttime, transitionTime, easing, newPosition);

                    notePath[index] = pathPoint;

                }
            }
            else
            {

                List<Anchor> notePath = notePathByColumn[column];

                Anchor pathPoint = notePath[index];

                pathPoint = pathPoint.ManipulatePosition(starttime, transitionTime, easing, newPosition);

                notePath[index] = pathPoint;

            }

        }

        public String debugBezier(double starttime, StoryboardLayer layer)
        {

            String debug = "";

            foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
            {

                if (currentColumn == ColumnType.all)
                    continue;

                List<Anchor> notePath = notePathByColumn[currentColumn];
                List<Vector2> points = new List<Vector2>();

                foreach (Anchor noteAnchor in notePath)
                {
                    // points.Add(noteAnchor.sprite.PositionAt(starttime));
                    points.Add(noteAnchor.getPositionAt(starttime));
                }

                const int resolution = 50;
                for (float t = 0; t <= 1; t += 1f / (resolution - 1))
                {
                    Vector2 pointOnCurve = BezierCurve.CalculatePoint(points, t);
                    // Draw or store the point as required
                    debug += $"{pointOnCurve}";
                    OsbSprite sprite = layer.CreateSprite("sb/white1x.png", OsbOrigin.Centre, pointOnCurve);
                    sprite.Fade(starttime, 1);
                    sprite.Fade(starttime + 50000, 0);

                }
            }

            return debug;

        }
    }
}