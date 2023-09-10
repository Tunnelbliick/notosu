using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using storyboard.scriptslibrary.maniaModCharts.effects;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{

    public enum PathType
    {
        line,
        bezier
    }

    public class DrawInstance
    {

        public double starttime = 0;
        public double easetime = 0;
        public OsbEasing easing;
        public Playfield playfieldInstance;
        public double updatesPerSecond = 2;
        public double fadeInTime = 50;
        public double fadeOutTime = 10;
        public bool rotateToFaceReceptor = true;
        public double iterationLength = 1000 / 2;
        public Dictionary<ColumnType, List<OsbSprite>> pathWaySprites = new Dictionary<ColumnType, List<OsbSprite>>();

        public Dictionary<double, Double> updatesPerSecondDictionary = new Dictionary<double, Double>();


        public Dictionary<ColumnType, List<Anchor>> notePathByColumn = new Dictionary<ColumnType, List<Anchor>>();

        public DrawInstance InitializeDrawInstance(Playfield playfieldInstance, double starttime, double easetime, double updatesPerSecond, OsbEasing easing, bool rotateToFaceReceptor)
        {

            this.starttime = starttime;
            this.easetime = easetime;
            this.easing = easing;
            this.playfieldInstance = playfieldInstance;
            this.updatesPerSecond = updatesPerSecond;
            this.rotateToFaceReceptor = rotateToFaceReceptor;
            this.iterationLength = 1000 / updatesPerSecond;
            updatesPerSecondDictionary.Add(starttime, updatesPerSecond);

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
            this.iterationLength = 1000 / updatesPerSecond;
            updatesPerSecondDictionary.Add(starttime, updatesPerSecond);

        }


        public DrawInstance(Playfield playfieldInstance, double starttime, double easetime, double updatesPerSecond, OsbEasing easing, bool rotateToFaceReceptor, double fadeInTime, double fadeOutTime)
        {
            this.starttime = starttime;
            this.easetime = easetime;
            this.easing = easing;
            this.playfieldInstance = playfieldInstance;
            this.updatesPerSecond = updatesPerSecond;
            this.rotateToFaceReceptor = rotateToFaceReceptor;
            this.fadeInTime = fadeInTime;
            this.fadeOutTime = fadeOutTime;
            this.iterationLength = 1000 / updatesPerSecond;
            updatesPerSecondDictionary.Add(starttime, updatesPerSecond);
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

                RenderReceptor(column, duration);

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

        public string drawNotesByOriginToReceptor(double duration)
        {
            double endtime = starttime + duration;
            string debug = "";

            foreach (Column column in playfieldInstance.columns.Values)
            {

                RenderReceptor(column, duration);

                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];

                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime - easetime <= endtime).ToList();

                double localIterationRate = this.iterationLength;

                foreach (var key in keysInRange)
                {

                    Note note = notes[key];
                    double totalDuration = easetime;

                    var updateRate = findCurrentUpdateRate(note.starttime - easetime);

                    if (updateRate.Value != this.updatesPerSecond && updateRate.Value != 0)
                    {
                        localIterationRate = 1000 / updateRate.Value;
                    }

                    double currentTime = note.starttime - easetime - localIterationRate;
                    double renderStartTime = Math.Max(currentTime, starttime);
                    double renderEndTime = Math.Min(note.endtime, endtime);
                    Vector2 currentPosition = column.origin.getCurrentPosition(currentTime);
                    float progress = 0f;
                    double iteratedTime = 0;
                    note.invisible(currentTime - 1);

                    var currentEffect = findEffectByReferenceTime(currentTime);

                    if (currentEffect.Value != null)
                    {
                        note.RenderTransformed(renderStartTime, renderEndTime, currentEffect.Value.reference, fadeInTime, fadeOutTime);
                    }
                    else
                    {
                        note.Render(renderStartTime, renderEndTime, easing, fadeInTime, fadeOutTime);
                    }

                    double startRotation = note.getRotation(currentTime);

                    do
                    {
                        double timeleft = note.starttime - currentTime;
                        double elapsedTime = currentTime - note.starttime;

                        currentEffect = findEffectByReferenceTime(currentTime);
                        if (currentEffect.Value != null)
                        {
                            note.UpdateTransformed(currentTime, currentTime + localIterationRate, currentEffect.Value.reference, 10);
                        }

                        progress = Math.Min((float)(iteratedTime / easetime), 1);
                        Vector2 originPosition = column.origin.getCurrentPosition(currentTime + localIterationRate);
                        Vector2 receptorPosition = column.receptor.getCurrentPosition(currentTime + localIterationRate);
                        Vector2 newPosition = Vector2.Lerp(originPosition, receptorPosition, progress);
                        Vector2 originScale = column.origin.getCurrentScale(currentTime + localIterationRate);
                        Vector2 receptorScale = column.receptor.getCurrentScale(currentTime + localIterationRate);
                        Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, progress);

                        double theta = 0;
                        if (progress < 0.15 && rotateToFaceReceptor)
                        {
                            Vector2 delta = receptorPosition - currentPosition;
                            if (currentPosition.Y > receptorPosition.Y)
                            {
                                delta = -delta;
                            }
                            theta = Math.Atan2(delta.X, delta.Y);
                        }

                        note.Move(currentTime, localIterationRate, easing, currentPosition, newPosition);
                        note.Scale(currentTime, localIterationRate, easing, column.origin.getCurrentScale(currentTime), scaleProgress);
                        // note.AbsoluteRotate(currentTime, iterationLenght, easing, startRotation - theta);

                        iteratedTime += localIterationRate;
                        currentTime += localIterationRate;
                        currentPosition = newPosition;

                    } while (progress < 1);

                    foreach (SliderParts parts in note.sliderPositions)
                    {

                        double sliderIterationLenght = localIterationRate;
                        updateRate = findCurrentUpdateRate(parts.Timestamp - this.easetime);

                        if (updateRate.Value != this.updatesPerSecond && updateRate.Value != 0)
                        {
                            sliderIterationLenght = 1000 / updateRate.Value;
                        }

                        double sliderStartime = parts.Timestamp;
                        OsbSprite sprite = parts.Sprite;
                        double sliderCurrentTime = sliderStartime - easetime - sliderIterationLenght;
                        Vector2 currentSliderPositon = column.origin.getCurrentPosition(sliderCurrentTime);
                        double sliderRenderStartTime = Math.Max(sliderStartime - easetime, sliderStartime);
                        double sliderRenderEndTime = Math.Min(sliderStartime + 0.1f, endtime);
                        float sliderProgress = 0;
                        double sliderIteratedTime = 0;

                        // sprite.Fade(sliderStartime - easetime, 0);
                        sprite.Fade(sliderCurrentTime, 1);
                        sprite.Fade(sliderRenderEndTime, 0);
                        double sliderRotation = sprite.RotationAt(sliderCurrentTime);

                        do
                        {
                            double timeleft = sliderStartime - sliderCurrentTime;
                            sliderProgress = Math.Min((float)(sliderIteratedTime / easetime), 1);

                            Vector2 originPosition = column.origin.getCurrentPosition(sliderCurrentTime + sliderIterationLenght);
                            Vector2 receptorPosition = column.receptor.getCurrentPosition(sliderCurrentTime + sliderIterationLenght);
                            Vector2 newPosition = Vector2.Lerp(originPosition, receptorPosition, sliderProgress);
                            Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime + sliderIterationLenght);

                            double theta = 0;
                            Vector2 delta = receptorPosition - currentSliderPositon;

                            if (currentSliderPositon.Y > receptorPosition.Y)
                            {
                                delta = -delta;
                            }

                            theta = Math.Atan2(delta.X, delta.Y);

                            // If the note is already done
                            if (sliderCurrentTime > note.starttime)
                            {
                                Vector2 newNotePosition = column.receptor.getCurrentPosition(sliderCurrentTime + sliderIterationLenght);
                                note.Move(sliderCurrentTime, sliderIterationLenght, easing, currentPosition, newNotePosition);
                                note.Scale(sliderCurrentTime, sliderIterationLenght, easing, receptorScale, receptorScale);
                                currentPosition = newNotePosition;
                            }

                            sprite.Move(easing, sliderCurrentTime, sliderCurrentTime + sliderIterationLenght, currentSliderPositon, newPosition);
                            sprite.ScaleVec(sliderCurrentTime, 0.7f / 0.5f * column.origin.getCurrentScale(sliderCurrentTime).X, 0.16f / 0.5f * column.origin.getCurrentScale(sliderCurrentTime).Y);
                            sprite.Rotate(easing, sliderCurrentTime, sliderCurrentTime + sliderIterationLenght, sprite.RotationAt(sliderCurrentTime), sliderRotation - theta);

                            sliderIteratedTime += sliderIterationLenght;
                            sliderCurrentTime += sliderIterationLenght;
                            currentSliderPositon = newPosition;

                        } while (sliderProgress < 1);
                    }
                }
            }

            return debug;
        }

        public string drawNotesByAnchors(double duration, PathType type = PathType.line)
        {

            double endtime = starttime + duration;
            string debug = "";

            foreach (Column column in playfieldInstance.columns.Values)
            {
                RenderReceptor(column, duration);
                List<Anchor> notePath = this.notePathByColumn[column.type];
                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime - easetime <= endtime).ToList();
                double localIterationRate = this.iterationLength;

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];
                    double totalDuration = easetime;
                    var updateRate = findCurrentUpdateRate(note.starttime - easetime);
                    if (updateRate.Value != this.updatesPerSecond && updateRate.Value != 0)
                    {
                        localIterationRate = 1000 / updateRate.Value;
                    }

                    double currentTime = note.starttime - easetime - localIterationRate;
                    double renderStartTime = Math.Max(currentTime, starttime);
                    double renderEndTime = Math.Min(note.endtime, endtime);
                    Vector2 currentPosition = column.origin.getCurrentPosition(currentTime);
                    float progress = 0f;
                    double iteratedTime = 0;
                    note.invisible(currentTime - 1);

                    var currentEffect = findEffectByReferenceTime(currentTime);
                    if (currentEffect.Value != null)
                    {
                        note.RenderTransformed(renderStartTime, renderEndTime, currentEffect.Value.reference, fadeInTime, fadeOutTime);
                    }
                    else
                    {
                        note.Render(renderStartTime, renderEndTime, easing, fadeInTime, fadeOutTime);
                    }

                    double startRotation = note.getRotation(currentTime);

                    switch (type)
                    {
                        case PathType.line:
                            // Use direct lines between anchors
                            double timePerAnchor = totalDuration / (notePath.Count - 1);
                            int optimalUpdatesForAnchor = (int)(timePerAnchor / localIterationRate);
                            localIterationRate = timePerAnchor / optimalUpdatesForAnchor;

                            for (int n = 0; n < notePath.Count - 1; n++)
                            {

                                iteratedTime = 0;
                                while (iteratedTime < timePerAnchor)
                                {

                                    progress = (float)(iteratedTime / timePerAnchor);
                                    Vector2 newPosition = MoveNoteForAnchor(column, notePath, localIterationRate, note, currentTime, currentPosition, progress, n, startRotation);

                                    iteratedTime += localIterationRate;
                                    currentTime += localIterationRate;
                                    currentPosition = newPosition;
                                };

                                if (n == notePath.Count - 2)
                                {

                                    progress = Math.Min((float)(iteratedTime / timePerAnchor), 1);
                                    Vector2 newPosition = MoveNoteForAnchor(column, notePath, localIterationRate, note, currentTime, currentPosition, progress, n, startRotation);

                                }
                            }
                            break;
                        case PathType.bezier:

                            while (progress < 1)
                            {
                                progress = Math.Min((float)(iteratedTime / totalDuration), 1f);

                                Vector2 receptorPosition = column.getReceptorPositionForNotes(currentTime);
                                Vector2 originScale = column.origin.getCurrentScale(currentTime);
                                Vector2 receptorScale = column.receptor.getCurrentScale(currentTime);
                                Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, progress);
                                List<Vector2> points = GetPathAnchorVectors(notePath, currentTime);
                                Vector2 newPosition = BezierCurve.CalculatePoint(points, progress);

                                double theta = 0;
                                Vector2 delta = currentPosition - newPosition;
                                if (rotateToFaceReceptor)
                                    theta = Math.Atan2(delta.X, delta.Y);

                                note.Move(currentTime, localIterationRate, easing, currentPosition, newPosition);
                                note.Scale(currentTime, localIterationRate, easing, column.origin.getCurrentScale(currentTime), scaleProgress);
                                note.AbsoluteRotate(currentTime, localIterationRate, easing, startRotation - theta);

                                iteratedTime += localIterationRate;
                                currentTime += localIterationRate;
                                currentPosition = newPosition;
                            }
                            break;
                    }


                    List<SliderParts> reversedSliderPoints = note.sliderPositions.ToList();
                    reversedSliderPoints.Reverse();
                    foreach (SliderParts parts in note.sliderPositions)
                    {

                        double sliderStartime = parts.Timestamp;
                        OsbSprite sprite = parts.Sprite;
                        double sliderCurrentTime = sliderStartime - easetime - localIterationRate;
                        Vector2 currentSliderPositon = column.origin.getCurrentPosition(sliderCurrentTime);
                        double sliderRenderStartTime = Math.Max(sliderCurrentTime, sliderStartime);
                        double sliderRenderEndTime = Math.Min(sliderStartime + 0.1f, endtime);

                        sprite.Fade(sliderRenderStartTime, 1);
                        sprite.Fade(sliderRenderEndTime, 0);
                        double sliderRotation = sprite.RotationAt(sliderCurrentTime);

                        float sliderProgress = 0;
                        double sliderIteratedTime = 0;

                        switch (type)
                        {
                            // Use direct lines between anchors
                            case PathType.line:
                                double timePerAnchor = totalDuration / (notePath.Count - 1);
                                int optimalUpdatesForAnchor = (int)(timePerAnchor / localIterationRate);
                                localIterationRate = timePerAnchor / optimalUpdatesForAnchor;

                                for (int n = 0; n < notePath.Count - 1; n++)
                                {
                                    sliderProgress = 0;
                                    sliderIteratedTime = 0;

                                    while (sliderIteratedTime < timePerAnchor)
                                    {
                                        sliderProgress = (float)(sliderIteratedTime / timePerAnchor);
                                        Vector2 newPosition;
                                        currentPosition = MoveSliderForAnchor(column, notePath, note, currentPosition, out currentEffect, localIterationRate, sprite, sliderCurrentTime, currentSliderPositon, sliderRotation, n, sliderProgress, out newPosition, startRotation);

                                        sliderIteratedTime += localIterationRate;
                                        sliderCurrentTime += localIterationRate;
                                        currentSliderPositon = newPosition;
                                    }

                                    if (n == notePath.Count - 2)
                                    {
                                        sliderProgress = Math.Min((float)(sliderIteratedTime / timePerAnchor), 1);
                                        Vector2 newPosition;
                                        currentPosition = MoveSliderForAnchor(column, notePath, note, currentPosition, out currentEffect, localIterationRate, sprite, sliderCurrentTime, currentSliderPositon, sliderRotation, n, sliderProgress, out newPosition, startRotation);
                                    }
                                }
                                break;

                            // Use Bezier calculation for the path
                            case PathType.bezier:
                                while (sliderProgress < 1)
                                {
                                    sliderProgress = Math.Min((float)(sliderIteratedTime / totalDuration), 1f);

                                    Vector2 receptorPosition = column.getReceptorPositionForNotes(sliderCurrentTime + localIterationRate);
                                    Vector2 originScale = column.origin.getCurrentScale(sliderCurrentTime + localIterationRate);
                                    Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime + localIterationRate);
                                    Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, sliderProgress);
                                    List<Vector2> points = GetPathAnchorVectors(notePath, sliderCurrentTime);

                                    Vector2 newPosition = BezierCurve.CalculatePoint(points, sliderProgress);

                                    double theta = 0;
                                    Vector2 delta = newPosition - currentSliderPositon;
                                    theta = Math.Atan2(delta.X, delta.Y);

                                    // If the note is already done
                                    if (sliderCurrentTime > note.starttime)
                                    {

                                        Vector2 newNotePosition = column.receptor.getCurrentPosition(sliderCurrentTime + localIterationRate);

                                        double noteTheta = 0;
                                        Vector2 noteDelta = newNotePosition - currentPosition;
                                        if (rotateToFaceReceptor)
                                        {
                                            noteTheta = Math.Atan2(noteDelta.X, noteDelta.Y);
                                        }

                                        note.Move(sliderCurrentTime, localIterationRate, easing, currentPosition, newNotePosition);
                                        note.Scale(sliderCurrentTime, localIterationRate, easing, receptorScale, receptorScale);
                                        note.AbsoluteRotate(sliderCurrentTime, localIterationRate, easing, startRotation - noteTheta);
                                        currentPosition = newNotePosition;
                                    }

                                    sprite.Move(easing, sliderCurrentTime, sliderCurrentTime + localIterationRate, currentSliderPositon, newPosition);
                                    sprite.ScaleVec(sliderCurrentTime, 0.7f / 0.5f * column.origin.getCurrentScale(sliderCurrentTime).X, 0.16f / 0.5f * column.origin.getCurrentScale(sliderCurrentTime).Y);
                                    sprite.Rotate(sliderCurrentTime, -theta);

                                    sliderIteratedTime += localIterationRate;
                                    sliderCurrentTime += localIterationRate;
                                    currentSliderPositon = newPosition;
                                }
                                break;
                        }
                    }
                }
            }

            return debug;
        }

        private static List<Vector2> GetPathAnchorVectors(List<Anchor> notePath, double currentTime)
        {
            List<Vector2> points = new List<Vector2>();
            foreach (Anchor noteAnchor in notePath)
            {
                points.Add(noteAnchor.getPositionAt(currentTime));
            }

            return points;
        }

        private Vector2 MoveSliderForAnchor(Column column, List<Anchor> notePath, Note note, Vector2 currentPosition, out KeyValuePair<double, EffectInfo> currentEffect, double movementTime, OsbSprite sprite, double sliderCurrentTime, Vector2 currentSliderPositon, double sliderRotation, int n, float sliderProgress, out Vector2 newPosition, double noteStartRotation)
        {
            currentEffect = findEffectByReferenceTime(sliderCurrentTime);
            if (currentEffect.Value != null)
            {
                //sprite.UpdateTransformed(sliderCurrentTime, sliderCurrentTime + movementTime, currentEffect.Value.reference, 10);
            }

            Vector2 startPos = notePath[n].sprite.PositionAt(sliderCurrentTime + movementTime);
            Vector2 endPos = notePath[n + 1].sprite.PositionAt(sliderCurrentTime + movementTime);

            newPosition = Vector2.Lerp(startPos, endPos, sliderProgress);
            Vector2 receptorPosition = column.receptor.getCurrentPosition(sliderCurrentTime + movementTime);
            Vector2 originScale = column.origin.getCurrentScale(sliderCurrentTime + movementTime);
            Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime + movementTime);
            Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, sliderProgress);

            double theta = 0;
            Vector2 delta = currentSliderPositon - newPosition;
            if (delta.LengthSquared > 0 && rotateToFaceReceptor)
            {
                theta = Math.Atan2(delta.X, delta.Y);
            }

            // If the note is already done
            if (sliderCurrentTime > note.starttime)
            {
                Vector2 newNotePosition = column.receptor.getCurrentPosition(sliderCurrentTime + movementTime);

                double noteTheta = 0;
                Vector2 noteDelta = newNotePosition - currentPosition;
                if (rotateToFaceReceptor)
                {
                    noteTheta = Math.Atan2(noteDelta.X, noteDelta.Y);
                }

                note.Move(sliderCurrentTime, movementTime, easing, currentPosition, newNotePosition);
                note.Scale(sliderCurrentTime, movementTime, easing, receptorScale, receptorScale);
                note.AbsoluteRotate(sliderCurrentTime, movementTime, easing, noteStartRotation - noteTheta);
                currentPosition = newNotePosition;
            }

            sprite.Move(easing, sliderCurrentTime, sliderCurrentTime + movementTime, currentSliderPositon, newPosition);
            sprite.ScaleVec(sliderCurrentTime, 0.7f / 0.5f * column.origin.getCurrentScale(sliderCurrentTime).X, 0.16f / 0.5f * column.origin.getCurrentScale(sliderCurrentTime).Y);
            sprite.Rotate(easing, sliderCurrentTime, sliderCurrentTime + movementTime, sprite.RotationAt(sliderCurrentTime), sliderRotation - theta);
            return currentPosition;
        }

        private Vector2 MoveNoteForAnchor(Column column, List<Anchor> notePath, double movementTime, Note note, double currentTime, Vector2 currentPosition, float progress, int n, double startRotation)
        {

            KeyValuePair<double, EffectInfo> currentEffect = findEffectByReferenceTime(currentTime);
            if (currentEffect.Value != null)
            {
                note.UpdateTransformed(currentTime, currentTime + movementTime, currentEffect.Value.reference, 10);
            }

            Vector2 startPos = notePath[n].sprite.PositionAt(currentTime + movementTime);
            Vector2 endPos = notePath[n + 1].sprite.PositionAt(currentTime + movementTime);

            Vector2 newPosition = Vector2.Lerp(startPos, endPos, progress);
            Vector2 receptorPosition = column.receptor.getCurrentPosition(currentTime + movementTime);
            Vector2 originScale = column.origin.getCurrentScale(currentTime + movementTime);
            Vector2 receptorScale = column.receptor.getCurrentScale(currentTime + movementTime);
            Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, progress);

            double theta = 0;
            if (rotateToFaceReceptor)
            {
                Vector2 delta = receptorPosition - currentPosition;
                if (currentPosition.Y > receptorPosition.Y)
                {
                    delta = -delta;
                }
                theta = Math.Atan2(delta.X, delta.Y);
            }

            note.Move(currentTime, movementTime, easing, currentPosition, newPosition);
            note.Scale(currentTime, movementTime, easing, column.origin.getCurrentScale(currentTime), scaleProgress);
            note.AbsoluteRotate(currentTime, movementTime, easing, startRotation - theta);

            return newPosition;
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
            Vector2 receptorPosition = currentColumn.getReceptorPosition(starttime);

            int index = 0;

            float blend = 1.0f / (notePath.Count + relativeOffsets.Count - 1);

            foreach (Anchor noteAnchor in notePath)
            {

                float currentBlend = blend * index;
                //debugString += $"{blend}, {index}, {currentBlend}, {notePath.Count}\n";
                debugString += $"{currentColumn}, {originPosition}, {receptorPosition}, {notePath.Count}\n";


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
                debugString += $"{currentColumn}, {offsetPositionForNewAnchor}, {lerpPositionForNewAnchor}, {notePath.Count}\n";

                //if debug add a sprite for the position of the vector
                Anchor pathPoint = new Anchor(0, column, offsetPositionForNewAnchor, offset, debug, debugLayer);
                notePath.Add(pathPoint);

                index++;
            }

            this.notePathByColumn[column] = notePath;

            return debugString;
        }

        public void ManipulateAnchorRelative(int index, double starttime, double transitionTime, Vector2 newPosition, OsbEasing easing, ColumnType column = ColumnType.all)
        {

            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    List<Anchor> notePath = notePathByColumn[currentColumn];

                    Anchor pathPoint = notePath[index];

                    Vector2 offset = Vector2.Add(pathPoint.getPositionAt(starttime), newPosition);

                    pathPoint = pathPoint.ManipulatePosition(starttime, transitionTime, easing, offset);

                    notePath[index] = pathPoint;

                }
            }
            else
            {

                List<Anchor> notePath = notePathByColumn[column];

                Anchor pathPoint = notePath[index];

                Vector2 offset = Vector2.Add(pathPoint.getPositionAt(starttime), newPosition);

                pathPoint = pathPoint.ManipulatePosition(starttime, transitionTime, easing, offset);

                notePath[index] = pathPoint;

            }

        }

        public double ResetAnchors(double starttime, double transitionTime, OsbEasing easing, ColumnType column = ColumnType.all)
        {
            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    List<Anchor> notePath = notePathByColumn[currentColumn];

                    Column selectedColumn = this.playfieldInstance.columns[currentColumn];

                    Vector2 originPosition = selectedColumn.getOriginPosition(starttime);
                    Vector2 receptorPosition = selectedColumn.getReceptorPosition(starttime);

                    float blend = 1.0f / (notePath.Count - 1);
                    int index = 0;
                    foreach (Anchor noteAnchor in notePath)
                    {

                        float currentBlend = blend * index;

                        Vector2 lerpPosition = Vector2.Lerp(originPosition, receptorPosition, currentBlend);
                        noteAnchor.ManipulatePosition(starttime, transitionTime, easing, lerpPosition);
                        index++;

                    }

                }
            }
            else
            {

                if (column == ColumnType.all)
                    return starttime + transitionTime;

                List<Anchor> notePath = notePathByColumn[column];

                Column selectedColumn = this.playfieldInstance.columns[column];
                Vector2 originPosition = selectedColumn.getOriginPosition(starttime);
                Vector2 receptorPosition = selectedColumn.getReceptorPosition(starttime);

                float blend = 1.0f / (notePath.Count - 1);
                int index = 0;
                foreach (Anchor noteAnchor in notePath)
                {

                    float currentBlend = blend * index;

                    Vector2 lerpPosition = Vector2.Lerp(originPosition, receptorPosition, currentBlend);
                    noteAnchor.ManipulatePosition(starttime, transitionTime, easing, lerpPosition);
                    index++;

                }

            }

            return starttime + transitionTime;

        }


        public void ManipulateAnchorAbsolute(int index, ColumnType column, double starttime, double transitionTime, Vector2 newPosition, OsbEasing easing)
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

        public String DrawPath(double starttime, double endtime, StoryboardLayer layer, string spritePath, PathType type, int precision = 10)
        {
            String debug = "";

            Dictionary<ColumnType, List<OsbSprite>> pathSprites = this.pathWaySprites;
            Dictionary<ColumnType, List<Vector2>> priorTime = new Dictionary<ColumnType, List<Vector2>>();
            double currentTime = starttime;
            double localIterationRate = this.iterationLength;

            foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
            {

                if (currentColumn == ColumnType.all)
                    continue;

                List<Anchor> notePath = notePathByColumn[currentColumn];
                List<Vector2> points = GetPathAnchorVectors(notePath, starttime);
                List<OsbSprite> columnSprites = new List<OsbSprite>();

                switch (type)
                {
                    case PathType.bezier:
                        float progress = 0;
                        float increment = 1f / precision;

                        while (progress < 1f)
                        {
                            Vector2 firstPoint = BezierCurve.CalculatePoint(points, progress);
                            Vector2 secondPoint = BezierCurve.CalculatePoint(points, progress + increment);

                            float dx = firstPoint.X - secondPoint.X;
                            float dy = firstPoint.Y - secondPoint.Y;
                            float distance = (float)Math.Sqrt(dx * dx + dy * dy) + 1f;

                            Vector2 delta = firstPoint - secondPoint;
                            double theta = Math.Atan2(delta.X, delta.Y);

                            OsbSprite sprite = layer.CreateSprite(spritePath, OsbOrigin.BottomCentre, firstPoint);
                            sprite.Rotate(starttime, -theta);
                            sprite.ScaleVec(starttime, new Vector2(4, distance));
                            sprite.Fade(endtime, 0);
                            columnSprites.Add(sprite);

                            progress += increment;
                        }
                        break;

                    case PathType.line:
                        for (int n = 0; n < notePath.Count - 1; n++)
                        {
                            Vector2 firstPoint = notePath[n].position;
                            Vector2 secondPoint = notePath[n + 1].position;

                            float dx = firstPoint.X - secondPoint.X;
                            float dy = firstPoint.Y - secondPoint.Y;
                            float distance = (float)Math.Sqrt(dx * dx + dy * dy) + 1f;

                            Vector2 delta = firstPoint - secondPoint;
                            double theta = Math.Atan2(delta.X, delta.Y);

                            OsbSprite sprite = layer.CreateSprite(spritePath, OsbOrigin.BottomCentre, firstPoint);
                            sprite.Rotate(starttime, -theta);
                            sprite.ScaleVec(starttime, new Vector2(4, distance));
                            sprite.Fade(endtime, 0);
                            columnSprites.Add(sprite);

                        }
                        break;
                }

                pathSprites.Add(currentColumn, columnSprites);
            }

            this.pathWaySprites = pathSprites;

            while (currentTime <= endtime)
            {
                var updateRate = findCurrentUpdateRate(currentTime);
                if (updateRate.Value != this.updatesPerSecond && updateRate.Value != 0)
                {
                    localIterationRate = 1000 / updateRate.Value;
                }

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    List<Anchor> notePath = notePathByColumn[currentColumn];
                    List<Vector2> points = GetPathAnchorVectors(notePath, currentTime);
                    List<Vector2> lastPoints = new List<Vector2>();

                    if (priorTime.ContainsKey(currentColumn))
                    {
                        lastPoints = priorTime[currentColumn];
                    }
                    else
                    {
                        priorTime.Add(currentColumn, points);
                    }

                    if (lastPoints.SequenceEqual(points))
                    {
                        continue;
                    }

                    priorTime[currentColumn] = points;


                    List<OsbSprite> columnSprites = this.pathWaySprites[currentColumn];
                    int i = 0;

                    switch (type)
                    {
                        case PathType.bezier:
                            float progress = 0;
                            float increment = 1f / precision;

                            while (progress < 1f)
                            {
                                Vector2 firstPoint = BezierCurve.CalculatePoint(points, progress);
                                Vector2 secondPoint = BezierCurve.CalculatePoint(points, progress + increment);

                                float dx = firstPoint.X - secondPoint.X;
                                float dy = firstPoint.Y - secondPoint.Y;
                                float distance = (float)Math.Sqrt(dx * dx + dy * dy) + 1f;

                                Vector2 delta = firstPoint - secondPoint;
                                double theta = Math.Atan2(delta.X, delta.Y);

                                OsbSprite sprite = columnSprites[i];

                                Vector2 currentPosition = sprite.PositionAt(currentTime);
                                if (currentColumn == ColumnType.one && progress == 0)
                                    debug += $"{currentPosition}, {firstPoint}, {currentPosition - firstPoint}, \n";


                                sprite.Move(currentTime, currentTime + iterationLength, sprite.PositionAt(currentTime), firstPoint);
                                sprite.Rotate(currentTime, currentTime + iterationLength, sprite.RotationAt(currentTime), -theta);
                                sprite.ScaleVec(currentTime, currentTime + iterationLength, sprite.ScaleAt(currentTime), new Vector2(4, distance));


                                progress += increment;
                                i++;
                            }
                            break;

                        case PathType.line:
                            for (int n = 0; n < notePath.Count - 1; n++)
                            {
                                Vector2 firstPoint = notePath[n].position;
                                Vector2 secondPoint = notePath[n + 1].position;

                                float dx = firstPoint.X - secondPoint.X;
                                float dy = firstPoint.Y - secondPoint.Y;
                                float distance = (float)Math.Sqrt(dx * dx + dy * dy) + 1f;

                                Vector2 delta = firstPoint - secondPoint;
                                double theta = Math.Atan2(delta.X, delta.Y);

                                OsbSprite sprite = columnSprites[i];

                                Vector2 currentPosition = sprite.PositionAt(currentTime);

                                debug += $"{currentPosition}, {firstPoint}, {currentPosition - firstPoint}, \n";


                                sprite.Move(currentTime, currentTime + iterationLength, sprite.PositionAt(currentTime), firstPoint);
                                sprite.Rotate(currentTime, currentTime + iterationLength, sprite.RotationAt(currentTime), -theta);
                                sprite.ScaleVec(currentTime, currentTime + iterationLength, sprite.ScaleAt(currentTime), new Vector2(4, distance));


                                i++;
                            }
                            break;
                    }
                }

                currentTime += localIterationRate;
            }

            return debug;

        }

        private bool AreVectorsCloseEnough(Vector2 v1, Vector2 v2, float threshold = 0.5f)
        {
            float dx = v1.X - v2.X;
            float dy = v1.Y - v2.Y;
            float distanceSquared = dx * dx + dy * dy;
            return distanceSquared >= threshold * threshold;
        }


        public double FadePath(double starttime, double duration, OsbEasing easing, float fade, ColumnType column = ColumnType.all)
        {

            if (column == ColumnType.all)
            {
                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    foreach (OsbSprite path in this.pathWaySprites[currentColumn])
                    {
                        float currentFade = path.OpacityAt(starttime);
                        path.Fade(easing, starttime, starttime + duration, currentFade, fade);
                    }
                }
            }
            else
            {
                foreach (OsbSprite path in this.pathWaySprites[column])
                {
                    float currentFade = path.OpacityAt(starttime);
                    path.Fade(easing, starttime, starttime + duration, currentFade, fade);
                }
            }

            return starttime + duration;
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

        public void changeUpdateRate(double time, int updatesPerSecond)
        {
            updatesPerSecondDictionary.Add(Math.Max(time - this.easetime, 0), updatesPerSecond);
        }

        public void RenderReceptor(Column column, double duration)
        {

            double currentTime = starttime;
            double endTime = starttime + duration;
            double iterationLenght = 1000 / updatesPerSecond;

            Receptor receptor = column.receptor;
            Vector2 currentPosition = receptor.getCurrentPosition(currentTime);

            while (currentTime < endTime)
            {
                var foundEntry = findEffectByReferenceTime(currentTime);

                if (foundEntry.Value != null)
                {
                    receptor.RenderTransformed(currentTime, endTime, foundEntry.Value.reference);
                }
                else
                {
                    receptor.Render(currentTime, endTime);
                }

                OsbSprite renderedReceptor = receptor.renderedSprite;
                Vector2 newPosition = receptor.getCurrentPosition(currentTime + iterationLenght);
                renderedReceptor.Move(currentTime, currentTime + iterationLenght, currentPosition, newPosition);
                currentPosition = newPosition;
                currentTime += iterationLenght;
            }

        }

        private KeyValuePair<double, EffectInfo> findEffectByReferenceTime(double time)
        {
            KeyValuePair<double, EffectInfo> currentEffect = playfieldInstance.effectReferenceByStartTime
                   .Where(kvp => kvp.Key <= time)
                   .OrderByDescending(kvp => kvp.Key)
                   .FirstOrDefault();

            return currentEffect;
        }

        private KeyValuePair<double, double> findCurrentUpdateRate(double time)
        {
            KeyValuePair<double, double> currentUpdateRate = updatesPerSecondDictionary
                   .Where(kvp => kvp.Key <= time)
                   .OrderByDescending(kvp => kvp.Key)
                   .FirstOrDefault();

            return currentUpdateRate;
        }
    }
}