using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Threading.Tasks;
using OpenTK;
using storyboard.scriptslibrary.maniaModCharts.Draw;
using storyboard.scriptslibrary.maniaModCharts.effects;
using StorybrewCommon.Animations;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{

    public enum PathType
    {
        line,
        bezier
    }

    public class DrawInstance : Drawer
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

        public bool hideHolds = false;
        public bool hideNormalNotes = false;
        public Dictionary<ColumnType, List<OsbSprite>> pathWaySprites = new Dictionary<ColumnType, List<OsbSprite>>();

        public Dictionary<double, double> updatesPerSecondDictionary = new Dictionary<double, double>();


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
            this.changeUpdateRate(starttime, updatesPerSecond);

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
            this.changeUpdateRate(starttime, updatesPerSecond);

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
            this.changeUpdateRate(starttime, updatesPerSecond);
        }

        /*public double drawNotesDefault(double duration)
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
        }*/

        /*public double drawNotesByEndPosition(double duration)
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
        }*/

        public string drawNotesStutteredByOriginToReceptor(double duration, bool renderReceptor = true)
        {
            double endtime = starttime + duration;
            string debug = "";

            foreach (Column column in playfieldInstance.columns.Values)
            {
                if (renderReceptor)
                    RenderReceptor(column, duration);

                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime - easetime <= endtime).ToList();

                foreach (var key in keysInRange)
                {

                    KeyframedValue<Vector2> movement = new KeyframedValue<Vector2>(null);
                    KeyframedValue<Vector2> scale = new KeyframedValue<Vector2>(null);
                    KeyframedValue<double> rotation = new KeyframedValue<double>(null);

                    Note note = notes[key];

                    if (note.isSlider == false && hideNormalNotes)
                    {
                        continue;
                    }

                    if (note.isSlider == true && hideHolds)
                    {
                        continue;
                    }

                    double totalDuration = easetime;

                    double currentTime = note.starttime - easetime;

                    double renderStartTime = Math.Max(currentTime, starttime);
                    double renderEndTime = Math.Min(note.endtime, endtime);
                    Vector2 currentPosition = column.origin.getCurrentPosition(currentTime);
                    float progress = 0f;
                    double iteratedTime = 0;
                    float initialFade = 1;
                    note.invisible(currentTime - 1);

                    FadeEffect noteFade = findFadeAtTime(currentTime);
                    if (noteFade != null)
                    {
                        initialFade = noteFade.value;
                    }

                    var currentEffect = findEffectByReferenceTime(currentTime);

                    if (currentEffect.Value != null)
                    {
                        note.RenderTransformed(renderStartTime, renderEndTime, currentEffect.Value.reference, fadeInTime, fadeOutTime);
                    }
                    else
                    {
                        note.Render(renderStartTime, renderEndTime, easing, initialFade, fadeInTime, fadeOutTime);
                    }

                    double startRotation = note.getRotation(currentTime);

                    do
                    {
                        double nextNoteTime = nextNoteToHit(currentTime, hideNormalNotes, hideHolds);

                        if (nextNoteTime == 0)
                        {
                            nextNoteTime = currentTime;
                        }

                        noteFade = findFadeAtTime(currentTime);
                        if (noteFade != null)
                        {
                            note.Fade(currentTime, currentTime, noteFade.easing, noteFade.value);
                        }

                        double timeleft = note.starttime - currentTime;
                        double elapsedTime = currentTime - note.starttime;

                        currentEffect = findEffectByReferenceTime(currentTime);
                        if (currentEffect.Value != null)
                        {
                            note.UpdateTransformed(currentTime, nextNoteTime, currentEffect.Value.reference, 10);
                        }

                        progress = Math.Min((float)(iteratedTime / easetime), 1);
                        Vector2 originPosition = column.origin.getCurrentPosition(nextNoteTime);
                        Vector2 receptorPosition = column.receptor.getCurrentPosition(nextNoteTime);
                        Vector2 newPosition = Vector2.Lerp(originPosition, receptorPosition, progress);
                        Vector2 originScale = column.origin.getCurrentScale(nextNoteTime);
                        Vector2 receptorScale = column.receptor.getCurrentScale(nextNoteTime);
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


                        note.Move(currentTime, 0, OsbEasing.None, currentPosition, newPosition);

                        //movement.Add(nextNoteTime, newPosition, EasingFunctions.ToEasingFunction(easing));
                        scale.Add(nextNoteTime, scaleProgress, EasingFunctions.ToEasingFunction(easing));
                        rotation.Add(nextNoteTime, startRotation - theta, EasingFunctions.ToEasingFunction(easing));

                        iteratedTime += Math.Max(nextNoteTime - currentTime, 0);
                        currentTime += Math.Max(nextNoteTime - currentTime, 0);
                        currentPosition = newPosition;

                    } while (progress < 1);

                    foreach (SliderParts part in note.sliderPositions)
                    {

                        KeyframedValue<Vector2> SliderMovement = new KeyframedValue<Vector2>(null);
                        KeyframedValue<Vector2> SliderScale = new KeyframedValue<Vector2>(null);
                        KeyframedValue<double> SliderRotation = new KeyframedValue<double>(null);

                        double sliderIterationLenght = findCurrentUpdateRate(part.Timestamp - this.easetime);

                        double sliderStartime = part.Timestamp;
                        OsbSprite sprite = part.Sprite;
                        double sliderCurrentTime = sliderStartime - easetime + part.Duration / 2;
                        Vector2 currentSliderPositon = column.origin.getCurrentPosition(sliderCurrentTime);
                        double sliderRenderStartTime = Math.Max(sliderStartime - easetime, starttime);
                        double sliderRenderEndTime = Math.Min(sliderStartime, endtime);
                        float sliderProgress = 0;
                        double sliderIteratedTime = 0;
                        //sprite.Fade(sliderCurrentTime, 0);


                        FadeEffect sliderFade = findFadeAtTime(sliderRenderStartTime);
                        if (sliderFade != null)
                        {
                            sprite.Fade(sliderRenderStartTime, sliderFade.value);
                        }
                        else
                        {
                            sprite.Fade(sliderRenderStartTime, 1);
                        }

                        sprite.Fade(sliderRenderEndTime, 0);
                        double sliderRotation = sprite.RotationAt(sliderCurrentTime);

                        float defaultScaleX = 0.7f / 0.5f;
                        float defaultScaleY = 0.14f / 0.5f * ((float)part.Duration / 20f); // This scaled was based on 20ms long sliderParts

                        Vector2 newScale = new Vector2(defaultScaleX * column.origin.getCurrentScale(sliderCurrentTime).X, defaultScaleY * column.origin.getCurrentScale(sliderCurrentTime).Y);

                        SliderScale.Add(sliderCurrentTime, newScale, EasingFunctions.ToEasingFunction(easing));
                        SliderRotation.Add(sliderCurrentTime, sliderRotation, EasingFunctions.ToEasingFunction(easing));

                        do
                        {

                            double nextNoteTime = nextNoteToHit(sliderCurrentTime, hideNormalNotes, hideHolds);

                            if (nextNoteTime == 0)
                            {
                                nextNoteTime = sliderCurrentTime;
                            }

                            sliderFade = findFadeAtTime(sliderCurrentTime);
                            if (sliderFade != null)
                            {
                                if (sprite.OpacityAt(sliderCurrentTime) != sliderFade.value)
                                    sprite.Fade(sliderFade.easing, sliderCurrentTime, sliderCurrentTime, sprite.OpacityAt(sliderCurrentTime), sliderFade.value);
                            }

                            double timeleft = sliderStartime - sliderCurrentTime;
                            sliderProgress = Math.Min((float)(sliderIteratedTime / easetime), 1);

                            Vector2 originPosition = column.origin.getCurrentPosition(nextNoteTime);
                            Vector2 receptorPosition = column.receptor.getCurrentPosition(nextNoteTime);
                            Vector2 newPosition = Vector2.Lerp(originPosition, receptorPosition, sliderProgress);
                            Vector2 receptorScale = column.receptor.getCurrentScale(nextNoteTime);
                            Vector2 renderedReceptorPosition = column.receptor.renderedSprite.PositionAt(sliderCurrentTime);

                            double theta = 0;
                            Vector2 delta = renderedReceptorPosition - currentSliderPositon;

                            if (currentSliderPositon.Y > renderedReceptorPosition.Y)
                            {
                                delta = -delta;
                            }

                            theta = Math.Atan2(delta.X, delta.Y);

                            newScale = new Vector2(defaultScaleX * column.origin.getCurrentScale(sliderCurrentTime).X, defaultScaleY * column.origin.getCurrentScale(sliderCurrentTime).Y);

                            sprite.Move(sliderCurrentTime, sliderCurrentTime, currentSliderPositon, newPosition);

                            SliderScale.Add(sliderCurrentTime, newScale, EasingFunctions.ToEasingFunction(easing));
                            SliderRotation.Add(sliderCurrentTime, sliderRotation - theta, EasingFunctions.ToEasingFunction(easing));

                            // If the note is already done
                            if (sliderCurrentTime >= note.starttime)
                            {
                                Vector2 newNotePosition = column.receptor.getCurrentPosition(sliderCurrentTime);
                                scale.Add(sliderCurrentTime, receptorScale, EasingFunctions.ToEasingFunction(easing));

                                currentPosition = newNotePosition;
                                sliderIteratedTime += sliderIterationLenght;
                                sliderCurrentTime += sliderIterationLenght;
                            }
                            else
                            {
                                sliderIteratedTime += Math.Max(nextNoteTime - sliderCurrentTime, 0);
                                sliderCurrentTime += Math.Max(nextNoteTime - sliderCurrentTime, 0);
                            }
                            currentSliderPositon = newPosition;

                        } while (sliderProgress < 1);

                        // Render out Slider keyframes
                        SliderScale.Simplify2dKeyframes(HoldScalePrecision, v => v);
                        SliderRotation.Simplify1dKeyframes(HoldRotationPrecision, v => (float)v);
                        SliderScale.ForEachPair((start, end) => sprite.ScaleVec(start.Time, end.Time, start.Value.X, start.Value.Y, end.Value.X, end.Value.Y));
                        SliderRotation.ForEachPair((start, end) => sprite.Rotate(start.Time, start.Value));

                    }

                    // Render out Note keyframes
                    scale.Simplify2dKeyframes(NoteScalePrecision, v => v);
                    rotation.Simplify1dKeyframes(NoteRotationPrecision, v => (float)v);
                    scale.ForEachPair((start, end) => note.Scale(start.Time, end.Time - start.Time, OsbEasing.None, start.Value, end.Value));
                    rotation.ForEachPair((start, end) => note.AbsoluteRotate(start.Time, end.Time - start.Time, OsbEasing.None, end.Value));

                    if (progress == 1)
                    {
                        note.ApplyHitLightingToNote(note.starttime, note.endtime, fadeOutTime, column.receptor);
                    }

                }
            }

            return debug;
        }

        public string drawNotesByOriginToReceptor(double duration, bool renderReceptor = true)
        {
            double endtime = starttime + duration;
            string debug = "";

            foreach (Column column in playfieldInstance.columns.Values)
            {
                if (renderReceptor)
                    RenderReceptor(column, duration);

                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime - easetime <= endtime).ToList();

                foreach (var key in keysInRange)
                {



                    KeyframedValue<Vector2> movement = new KeyframedValue<Vector2>(null);
                    KeyframedValue<Vector2> scale = new KeyframedValue<Vector2>(null);
                    KeyframedValue<double> rotation = new KeyframedValue<double>(null);

                    Note note = notes[key];

                    if (note.isSlider == false && hideNormalNotes)
                    {
                        continue;
                    }

                    if (note.isSlider == true && hideHolds)
                    {
                        continue;
                    }

                    double totalDuration = easetime;

                    double localIterationRate = findCurrentUpdateRate(note.starttime - easetime);

                    double currentTime = note.starttime - easetime - localIterationRate;
                    double renderStartTime = Math.Max(currentTime, starttime);
                    double renderEndTime = Math.Min(note.endtime, endtime);
                    Vector2 currentPosition = column.origin.getCurrentPosition(currentTime);
                    float progress = 0f;
                    double iteratedTime = 0;
                    float initialFade = 1;
                    note.invisible(currentTime - 1);

                    FadeEffect noteFade = findFadeAtTime(currentTime);
                    if (noteFade != null)
                    {
                        initialFade = noteFade.value;
                    }

                    var currentEffect = findEffectByReferenceTime(currentTime);

                    if (currentEffect.Value != null)
                    {
                        note.RenderTransformed(renderStartTime, renderEndTime, currentEffect.Value.reference, fadeInTime, fadeOutTime);
                    }
                    else
                    {
                        note.Render(renderStartTime, renderEndTime, easing, initialFade, fadeInTime, fadeOutTime);
                    }

                    double startRotation = note.getRotation(currentTime);

                    do
                    {

                        if (currentTime > endtime)
                        {
                            break;
                        }

                        noteFade = findFadeAtTime(currentTime);
                        if (noteFade != null)
                        {
                            note.Fade(currentTime, currentTime, noteFade.easing, noteFade.value);
                        }

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

                        movement.Add(currentTime + localIterationRate, newPosition, EasingFunctions.ToEasingFunction(easing));
                        scale.Add(currentTime + localIterationRate, scaleProgress, EasingFunctions.ToEasingFunction(easing));
                        rotation.Add(currentTime + localIterationRate, startRotation - theta, EasingFunctions.ToEasingFunction(easing));

                        iteratedTime += localIterationRate;
                        currentTime += localIterationRate;
                        currentPosition = newPosition;

                    } while (progress < 1);

                    foreach (SliderParts part in note.sliderPositions)
                    {

                        KeyframedValue<Vector2> SliderMovement = new KeyframedValue<Vector2>(null);
                        KeyframedValue<Vector2> SliderScale = new KeyframedValue<Vector2>(null);
                        KeyframedValue<double> SliderRotation = new KeyframedValue<double>(null);

                        double sliderIterationLenght = findCurrentUpdateRate(part.Timestamp - this.easetime);

                        double sliderStartime = part.Timestamp;
                        OsbSprite sprite = part.Sprite;
                        double sliderCurrentTime = sliderStartime - easetime - sliderIterationLenght;
                        Vector2 currentSliderPositon = column.origin.getCurrentPosition(sliderCurrentTime);
                        double sliderRenderStartTime = Math.Max(sliderStartime - easetime, starttime);
                        double sliderRenderEndTime = Math.Min(sliderStartime, endtime);
                        float sliderProgress = 0;
                        double sliderIteratedTime = 0;
                        sprite.Fade(sliderCurrentTime, 0);


                        FadeEffect sliderFade = findFadeAtTime(sliderRenderStartTime);
                        if (sliderFade != null)
                        {
                            sprite.Fade(sliderRenderStartTime, sliderFade.value);
                        }
                        else
                        {
                            sprite.Fade(sliderRenderStartTime, 1);
                        }

                        sprite.Fade(sliderRenderEndTime, 0);
                        double sliderRotation = sprite.RotationAt(sliderCurrentTime);

                        float defaultScaleX = 0.7f / 0.5f;
                        float defaultScaleY = 0.14f / 0.5f * ((float)part.Duration / 20f); // This scaled was based on 20ms long sliderParts

                        Vector2 newScale = new Vector2(defaultScaleX * column.origin.getCurrentScale(sliderCurrentTime).X, defaultScaleY * column.origin.getCurrentScale(sliderCurrentTime).Y);

                        SliderMovement.Add(sliderCurrentTime, currentSliderPositon, EasingFunctions.ToEasingFunction(easing));
                        SliderScale.Add(sliderCurrentTime, newScale, EasingFunctions.ToEasingFunction(easing));
                        SliderRotation.Add(sliderCurrentTime, sliderRotation, EasingFunctions.ToEasingFunction(easing));

                        do
                        {

                            if (sliderCurrentTime > endtime)
                            {
                                break;
                            }

                            sliderFade = findFadeAtTime(sliderCurrentTime);
                            if (sliderFade != null)
                            {
                                if (sprite.OpacityAt(sliderCurrentTime) != sliderFade.value)
                                    sprite.Fade(sliderFade.easing, sliderCurrentTime, sliderCurrentTime, sprite.OpacityAt(sliderCurrentTime), sliderFade.value);
                            }

                            double timeleft = sliderStartime - sliderCurrentTime;
                            sliderProgress = Math.Min((float)(sliderIteratedTime / easetime), 1);

                            Vector2 originPosition = column.origin.getCurrentPosition(sliderCurrentTime + sliderIterationLenght);
                            Vector2 receptorPosition = column.receptor.getCurrentPosition(sliderCurrentTime + sliderIterationLenght);
                            Vector2 newPosition = Vector2.Lerp(originPosition, receptorPosition, sliderProgress);
                            Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime + sliderIterationLenght);
                            Vector2 renderedReceptorPosition = column.receptor.renderedSprite.PositionAt(sliderCurrentTime);

                            double theta = 0;
                            Vector2 delta = renderedReceptorPosition - currentSliderPositon;

                            if (currentSliderPositon.Y > renderedReceptorPosition.Y)
                            {
                                delta = -delta;
                            }

                            theta = Math.Atan2(delta.X, delta.Y);

                            // If the note is already done
                            if (sliderCurrentTime > note.starttime)
                            {
                                Vector2 newNotePosition = column.receptor.getCurrentPosition(sliderCurrentTime + sliderIterationLenght);
                                movement.Add(sliderCurrentTime + sliderIterationLenght, newNotePosition, EasingFunctions.ToEasingFunction(easing));
                                scale.Add(sliderCurrentTime + sliderIterationLenght, receptorScale, EasingFunctions.ToEasingFunction(easing));

                                currentPosition = newNotePosition;
                            }

                            newScale = new Vector2(defaultScaleX * column.origin.getCurrentScale(sliderCurrentTime).X, defaultScaleY * column.origin.getCurrentScale(sliderCurrentTime).Y);

                            SliderMovement.Add(sliderCurrentTime + sliderIterationLenght, newPosition, EasingFunctions.ToEasingFunction(easing));
                            SliderScale.Add(sliderCurrentTime + sliderIterationLenght, newScale, EasingFunctions.ToEasingFunction(easing));
                            SliderRotation.Add(sliderCurrentTime + sliderIterationLenght, sliderRotation - theta, EasingFunctions.ToEasingFunction(easing));

                            sliderIteratedTime += sliderIterationLenght;
                            sliderCurrentTime += sliderIterationLenght;
                            currentSliderPositon = newPosition;

                        } while (sliderProgress < 1);

                        // Render out Slider keyframes
                        SliderMovement.Simplify2dKeyframes(HoldMovementPrecision, v => v);
                        SliderScale.Simplify2dKeyframes(HoldScalePrecision, v => v);
                        SliderRotation.Simplify1dKeyframes(HoldRotationPrecision, v => (float)v);
                        SliderMovement.ForEachPair((start, end) => sprite.Move(easing, start.Time, end.Time, start.Value, end.Value));
                        SliderScale.ForEachPair((start, end) => sprite.ScaleVec(start.Time, end.Time, start.Value.X, start.Value.Y, end.Value.X, end.Value.Y));
                        SliderRotation.ForEachPair((start, end) => sprite.Rotate(start.Time, start.Value));

                    }


                    // Render out Note keyframes
                    movement.Simplify2dKeyframes(NoteMovementPrecision, v => v);
                    scale.Simplify2dKeyframes(NoteScalePrecision, v => v);
                    rotation.Simplify1dKeyframes(NoteRotationPrecision, v => (float)v);
                    movement.ForEachPair((start, end) => note.Move(start.Time, end.Time - start.Time, OsbEasing.None, start.Value, end.Value));
                    scale.ForEachPair((start, end) => note.Scale(start.Time, end.Time - start.Time, OsbEasing.None, start.Value, end.Value));
                    rotation.ForEachPair((start, end) => note.AbsoluteRotate(start.Time, end.Time - start.Time, OsbEasing.None, end.Value));

                    if (progress == 1 && renderReceptor)
                    {
                        note.ApplyHitLightingToNote(note.starttime, note.endtime, fadeOutTime, column.receptor, localIterationRate);
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

                foreach (var key in keysInRange)
                {

                    KeyframedValue<Vector2> movement = new KeyframedValue<Vector2>(null);
                    KeyframedValue<Vector2> scale = new KeyframedValue<Vector2>(null);
                    KeyframedValue<double> rotation = new KeyframedValue<double>(null);

                    Note note = notes[key];
                    double totalDuration = easetime;
                    double localIterationRate = findCurrentUpdateRate(note.starttime - easetime);

                    double currentTime = note.starttime - easetime - localIterationRate;
                    double renderStartTime = Math.Max(currentTime, starttime);
                    double renderEndTime = Math.Min(note.endtime, endtime);
                    Vector2 currentPosition = column.origin.getCurrentPosition(currentTime);
                    float progress = 0f;
                    double iteratedTime = 0;
                    float initialFade = 1f;
                    note.invisible(currentTime - 1);

                    FadeEffect noteFade = findFadeAtTime(currentTime);
                    if (noteFade != null)
                    {
                        initialFade = noteFade.value;
                    }

                    var currentEffect = findEffectByReferenceTime(currentTime);
                    if (currentEffect.Value != null)
                    {
                        note.RenderTransformed(renderStartTime, renderEndTime, currentEffect.Value.reference, fadeInTime, fadeOutTime);
                    }
                    else
                    {
                        note.Render(renderStartTime, renderEndTime, easing, initialFade, fadeInTime, fadeOutTime);
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

                                    noteFade = findFadeAtTime(currentTime);
                                    if (noteFade != null)
                                    {
                                        note.Fade(currentTime, currentTime, noteFade.easing, noteFade.value);
                                    }

                                    progress = (float)(iteratedTime / timePerAnchor);

                                    currentEffect = findEffectByReferenceTime(currentTime);
                                    if (currentEffect.Value != null)
                                    {
                                        note.UpdateTransformed(currentTime, currentTime + localIterationRate, currentEffect.Value.reference, 10);
                                    }

                                    Vector2 startPos = notePath[n].sprite.PositionAt(currentTime + localIterationRate);
                                    Vector2 endPos = notePath[n + 1].sprite.PositionAt(currentTime + localIterationRate);

                                    Vector2 newPosition = Vector2.Lerp(startPos, endPos, progress);
                                    Vector2 receptorPosition = column.receptor.getCurrentPosition(currentTime + localIterationRate);
                                    Vector2 originScale = column.origin.getCurrentScale(currentTime + localIterationRate);
                                    Vector2 receptorScale = column.receptor.getCurrentScale(currentTime + localIterationRate);
                                    Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, progress);

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


                                    movement.Add(currentTime + localIterationRate, newPosition, EasingFunctions.ToEasingFunction(easing));
                                    scale.Add(currentTime + localIterationRate, scaleProgress, EasingFunctions.ToEasingFunction(easing));
                                    rotation.Add(currentTime + localIterationRate, startRotation - theta, EasingFunctions.ToEasingFunction(easing));

                                    iteratedTime += localIterationRate;
                                    currentTime += localIterationRate;
                                    currentPosition = newPosition;
                                };

                                if (n == notePath.Count - 2)
                                {

                                    progress = Math.Min((float)(iteratedTime / timePerAnchor), 1);

                                    currentEffect = findEffectByReferenceTime(currentTime);
                                    if (currentEffect.Value != null)
                                    {
                                        note.UpdateTransformed(currentTime, currentTime + localIterationRate, currentEffect.Value.reference, 10);
                                    }

                                    Vector2 startPos = notePath[n].sprite.PositionAt(currentTime + localIterationRate);
                                    Vector2 endPos = notePath[n + 1].sprite.PositionAt(currentTime + localIterationRate);

                                    Vector2 newPosition = Vector2.Lerp(startPos, endPos, progress);
                                    Vector2 receptorPosition = column.receptor.getCurrentPosition(currentTime + localIterationRate);
                                    Vector2 originScale = column.origin.getCurrentScale(currentTime + localIterationRate);
                                    Vector2 receptorScale = column.receptor.getCurrentScale(currentTime + localIterationRate);
                                    Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, progress);

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

                                    movement.Add(currentTime + localIterationRate, newPosition, EasingFunctions.ToEasingFunction(easing));
                                    scale.Add(currentTime + localIterationRate, scaleProgress, EasingFunctions.ToEasingFunction(easing));
                                    rotation.Add(currentTime + localIterationRate, startRotation - theta, EasingFunctions.ToEasingFunction(easing));

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
                                Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, progress);
                                List<Vector2> points = GetPathAnchorVectors(notePath, currentTime);
                                Vector2 newPosition = BezierCurve.CalculatePoint(points, progress);

                                double theta = 0;
                                Vector2 delta = currentPosition - newPosition;
                                if (rotateToFaceReceptor)
                                    theta = Math.Atan2(delta.X, delta.Y);


                                movement.Add(currentTime + localIterationRate, newPosition, EasingFunctions.ToEasingFunction(easing));
                                scale.Add(currentTime + localIterationRate, scaleProgress, EasingFunctions.ToEasingFunction(easing));
                                rotation.Add(currentTime + localIterationRate, startRotation - theta, EasingFunctions.ToEasingFunction(easing));

                                iteratedTime += localIterationRate;
                                currentTime += localIterationRate;
                                currentPosition = newPosition;
                            }

                            break;
                    }


                    List<SliderParts> reversedSliderPoints = note.sliderPositions.ToList();
                    reversedSliderPoints.Reverse();
                    foreach (SliderParts part in note.sliderPositions)
                    {

                        KeyframedValue<Vector2> SliderMovement = new KeyframedValue<Vector2>(null);
                        KeyframedValue<Vector2> SliderScale = new KeyframedValue<Vector2>(null);
                        KeyframedValue<double> SliderRotation = new KeyframedValue<double>(null);

                        double sliderStartime = part.Timestamp;
                        OsbSprite sprite = part.Sprite;
                        double sliderCurrentTime = sliderStartime - easetime - localIterationRate;
                        Vector2 currentSliderPositon = column.origin.getCurrentPosition(sliderCurrentTime);
                        double sliderRenderStartTime = Math.Max(sliderCurrentTime, sliderStartime);
                        double sliderRenderEndTime = Math.Min(sliderStartime + 0.1f, endtime);

                        sprite.Fade(sliderCurrentTime - 1000, 0);

                        sprite.Fade(Math.Max(sliderCurrentTime, renderStartTime), 1);
                        sprite.Fade(sliderRenderEndTime, 0);
                        double sliderRotation = sprite.RotationAt(sliderCurrentTime);

                        float defaultScaleX = 0.7f / 0.5f;
                        float defaultScaleY = 0.14f / 0.5f * ((float)part.Duration / 20f); // This scaled was based on 20ms long sliderParts

                        Vector2 newScale = new Vector2(defaultScaleX * column.origin.getCurrentScale(sliderCurrentTime).X, defaultScaleY * column.origin.getCurrentScale(sliderCurrentTime).Y);
                        SliderScale.Add(sliderCurrentTime, newScale, EasingFunctions.ToEasingFunction(easing));

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

                                        currentEffect = findEffectByReferenceTime(sliderCurrentTime);
                                        if (currentEffect.Value != null)
                                        {
                                            //sprite.UpdateTransformed(sliderCurrentTime, sliderCurrentTime + movementTime, currentEffect.Value.reference, 10);
                                        }

                                        Vector2 startPos = notePath[n].sprite.PositionAt(sliderCurrentTime + localIterationRate);
                                        Vector2 endPos = notePath[n + 1].sprite.PositionAt(sliderCurrentTime + localIterationRate);

                                        Vector2 newPosition = Vector2.Lerp(startPos, endPos, sliderProgress);
                                        Vector2 receptorPosition = column.receptor.getCurrentPosition(sliderCurrentTime + localIterationRate);
                                        Vector2 originScale = column.origin.getCurrentScale(sliderCurrentTime + localIterationRate);
                                        Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime + localIterationRate);
                                        Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, sliderProgress);

                                        double theta = 0;
                                        Vector2 delta = currentSliderPositon - newPosition;
                                        if (delta.LengthSquared > 0 && rotateToFaceReceptor)
                                        {
                                            theta = Math.Atan2(delta.X, delta.Y);
                                        }

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

                                            movement.Add(sliderCurrentTime + localIterationRate, newNotePosition, EasingFunctions.ToEasingFunction(easing));
                                            scale.Add(sliderCurrentTime + localIterationRate, receptorScale, EasingFunctions.ToEasingFunction(easing));
                                            rotation.Add(sliderCurrentTime + localIterationRate, startRotation - noteTheta, EasingFunctions.ToEasingFunction(easing));
                                            currentPosition = newNotePosition;
                                        }

                                        newScale = new Vector2(defaultScaleX * scaleProgress.X, defaultScaleY * scaleProgress.Y);

                                        SliderMovement.Add(sliderCurrentTime + localIterationRate, newPosition, EasingFunctions.ToEasingFunction(easing));
                                        SliderScale.Add(sliderCurrentTime + localIterationRate, newScale, EasingFunctions.ToEasingFunction(easing));
                                        SliderRotation.Add(sliderCurrentTime + localIterationRate, sliderRotation - theta, EasingFunctions.ToEasingFunction(easing));

                                        sliderIteratedTime += localIterationRate;
                                        sliderCurrentTime += localIterationRate;
                                        currentSliderPositon = newPosition;
                                    }

                                    if (n == notePath.Count - 2)
                                    {
                                        sliderProgress = Math.Min((float)(sliderIteratedTime / timePerAnchor), 1);

                                        currentEffect = findEffectByReferenceTime(sliderCurrentTime);
                                        if (currentEffect.Value != null)
                                        {
                                            //sprite.UpdateTransformed(sliderCurrentTime, sliderCurrentTime + movementTime, currentEffect.Value.reference, 10);
                                        }

                                        Vector2 startPos = notePath[n].sprite.PositionAt(sliderCurrentTime + localIterationRate);
                                        Vector2 endPos = notePath[n + 1].sprite.PositionAt(sliderCurrentTime + localIterationRate);

                                        Vector2 newPosition = Vector2.Lerp(startPos, endPos, sliderProgress);
                                        Vector2 receptorPosition = column.receptor.getCurrentPosition(sliderCurrentTime + localIterationRate);
                                        Vector2 originScale = column.origin.getCurrentScale(sliderCurrentTime + localIterationRate);
                                        Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime + localIterationRate);
                                        Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, sliderProgress);

                                        double theta = 0;
                                        Vector2 delta = currentSliderPositon - newPosition;
                                        if (delta.LengthSquared > 0 && rotateToFaceReceptor)
                                        {
                                            theta = Math.Atan2(delta.X, delta.Y);
                                        }

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

                                            movement.Add(sliderCurrentTime + localIterationRate, newNotePosition, EasingFunctions.ToEasingFunction(easing));
                                            scale.Add(sliderCurrentTime + localIterationRate, receptorScale, EasingFunctions.ToEasingFunction(easing));
                                            rotation.Add(sliderCurrentTime + localIterationRate, startRotation - noteTheta, EasingFunctions.ToEasingFunction(easing));
                                            currentPosition = newNotePosition;
                                        }

                                        newScale = new Vector2(defaultScaleX * scaleProgress.X, defaultScaleY * scaleProgress.Y);

                                        SliderMovement.Add(sliderCurrentTime + localIterationRate, newPosition, EasingFunctions.ToEasingFunction(easing));
                                        SliderScale.Add(sliderCurrentTime + localIterationRate, newScale, EasingFunctions.ToEasingFunction(easing));
                                        SliderRotation.Add(sliderCurrentTime + localIterationRate, sliderRotation - theta, EasingFunctions.ToEasingFunction(easing));

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
                                    Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, sliderProgress);
                                    List<Vector2> points = GetPathAnchorVectors(notePath, sliderCurrentTime);

                                    Vector2 newPosition = BezierCurve.CalculatePoint(points, sliderProgress);

                                    double theta = 0;
                                    Vector2 delta = newPosition - currentSliderPositon;
                                    theta = Math.Atan2(delta.X, delta.Y);

                                    /*if (this.HoldRoationDeadzone != 0 && Math.Abs(theta) > this.HoldRoationDeadzone)
                                    {
                                        theta = 0;
                                    }*/

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

                                        movement.Add(sliderCurrentTime + localIterationRate, newNotePosition, EasingFunctions.ToEasingFunction(easing));
                                        scale.Add(sliderCurrentTime + localIterationRate, receptorScale, EasingFunctions.ToEasingFunction(easing));
                                        rotation.Add(sliderCurrentTime + localIterationRate, startRotation - noteTheta, EasingFunctions.ToEasingFunction(easing));
                                        currentPosition = newNotePosition;
                                    }


                                    newScale = new Vector2(defaultScaleX * scaleProgress.X, defaultScaleY * scaleProgress.Y);

                                    SliderMovement.Add(sliderCurrentTime + localIterationRate, newPosition, EasingFunctions.ToEasingFunction(easing));
                                    SliderScale.Add(sliderCurrentTime + localIterationRate, newScale, EasingFunctions.ToEasingFunction(easing));
                                    SliderRotation.Add(sliderCurrentTime + localIterationRate, sliderRotation - theta, EasingFunctions.ToEasingFunction(easing));

                                    sliderIteratedTime += localIterationRate;
                                    sliderCurrentTime += localIterationRate;
                                    currentSliderPositon = newPosition;
                                }
                                break;
                        }
                        // Render out Slider keyframes
                        SliderMovement.Simplify2dKeyframes(HoldMovementPrecision, v => v);
                        SliderScale.Simplify2dKeyframes(HoldScalePrecision, v => v);
                        // SliderRotation.Simplify1dKeyframes(0f, v => (float)v);
                        SliderMovement.ForEachPair((start, end) => sprite.Move(easing, start.Time, end.Time, start.Value, end.Value));
                        SliderScale.ForEachPair((start, end) => sprite.ScaleVec(start.Time, end.Time, start.Value.X, start.Value.Y, end.Value.X, end.Value.Y));
                        SliderRotation.ForEachPair((start, end) => sprite.Rotate(start.Time, end.Value));

                    }

                    // Render out Note keyframes
                    movement.Simplify2dKeyframes(NoteMovementPrecision, v => v);
                    scale.Simplify2dKeyframes(NoteScalePrecision, v => v);
                    rotation.Simplify1dKeyframes(NoteRotationPrecision, v => (float)v);
                    movement.ForEachPair((start, end) => note.Move(start.Time, end.Time - start.Time, OsbEasing.None, start.Value, end.Value));
                    scale.ForEachPair((start, end) => note.Scale(start.Time, end.Time - start.Time, OsbEasing.None, start.Value, end.Value));
                    rotation.ForEachPair((start, end) => note.AbsoluteRotate(start.Time, end.Time - start.Time, OsbEasing.None, end.Value));

                    if (progress == 1)
                    {
                        note.ApplyHitLightingToNote(note.starttime, note.endtime, fadeOutTime, column.receptor, localIterationRate);
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
                    Anchor pathPoint = new Anchor(0, starttime, currentColumn, position, position, debug, debugLayer);
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
                Anchor pathPoint = new Anchor(0, starttime, column, position, position, debug, debugLayer);
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
            Anchor pathPoint = new Anchor(0, starttime, column, offsetPositionForNewAnchor, relativeOffset, debug, debugLayer);
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
                Anchor pathPoint = new Anchor(0, starttime, column, offsetPositionForNewAnchor, offset, debug, debugLayer);
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

                    pathPoint.ManipulatePosition(starttime, transitionTime, easing, offset);

                    notePath[index] = pathPoint;

                }
            }
            else
            {

                List<Anchor> notePath = notePathByColumn[column];

                Anchor pathPoint = notePath[index];

                Vector2 offset = Vector2.Add(pathPoint.getPositionAt(starttime), newPosition);

                pathPoint.ManipulatePosition(starttime, transitionTime, easing, offset);

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


        public void ManipulateAnchorAbsolute(int index, double starttime, double transitionTime, Vector2 newPosition, OsbEasing easing, ColumnType column)
        {

            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    List<Anchor> notePath = notePathByColumn[currentColumn];

                    Anchor pathPoint = notePath[index];

                    pathPoint.ManipulatePosition(starttime, transitionTime, easing, newPosition);

                    notePath[index] = pathPoint;

                }
            }
            else
            {

                List<Anchor> notePath = notePathByColumn[column];

                Anchor pathPoint = notePath[index];

                pathPoint.ManipulatePosition(starttime, transitionTime, easing, newPosition);

                notePath[index] = pathPoint;

            }

        }

        // TODO figure this shit out!
        public String UpdateAnchors(double starttime, double duration, ColumnType column)
        {
            String debug = "";

            double endtime = starttime + duration;
            double currentTime = starttime;

            while (currentTime <= endtime)
            {
                double localIterationRate = findCurrentUpdateRate(currentTime);

                debug += $"{currentTime}, {localIterationRate}\n";

                if (column == ColumnType.all)
                {

                    foreach (ColumnType type in Enum.GetValues(typeof(ColumnType)))
                    {

                        if (type == ColumnType.all)
                            continue;

                        List<Anchor> notePath = this.notePathByColumn[type];
                        Column currentColumn = this.playfieldInstance.columns[type];
                        Vector2 originPosition = currentColumn.getOriginPosition(currentTime + localIterationRate);
                        Vector2 receptorPosition = currentColumn.getReceptorPositionForNotes(currentTime + localIterationRate);

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

                            noteAnchor.ManipulatePosition(currentTime, localIterationRate, OsbEasing.None, adjustedPosition);

                            index++;
                        }
                    }
                }

                currentTime += localIterationRate;
                currentTime = Math.Round(currentTime);

            }

            return debug;

        }

        public String DrawPath(double starttime, double endtime, StoryboardLayer layer, string spritePath, PathType type, int precision, int updatesPerSecond = 3)
        {
            String debug = "";

            Dictionary<ColumnType, List<OsbSprite>> pathSprites = this.pathWaySprites;

            var movementPerSpriteByColumn = new Dictionary<ColumnType, List<KeyframedValue<Vector2>>>();
            var scalePerSpriteByColumn = new Dictionary<ColumnType, List<KeyframedValue<Vector2>>>();
            var rotationPerSpriteByColumn = new Dictionary<ColumnType, List<KeyframedValue<double>>>();

            double currentTime = starttime;

            foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
            {

                if (currentColumn == ColumnType.all)
                    continue;

                List<Anchor> notePath = notePathByColumn[currentColumn];
                List<Vector2> points = GetPathAnchorVectors(notePath, starttime);
                List<OsbSprite> columnSprites = new List<OsbSprite>();

                var movementPerSprite = new List<KeyframedValue<Vector2>>();
                var scalePerSprite = new List<KeyframedValue<Vector2>>();
                var rotationPerSprite = new List<KeyframedValue<double>>();

                switch (type)
                {
                    case PathType.bezier:
                        float progress = 0;
                        float increment = 1f / precision;

                        while (progress < 1f)
                        {

                            var movement = new KeyframedValue<Vector2>(null);
                            var scale = new KeyframedValue<Vector2>(null);
                            var rotation = new KeyframedValue<double>(null);

                            Vector2 firstPoint = BezierCurve.CalculatePoint(points, progress);
                            Vector2 secondPoint = BezierCurve.CalculatePoint(points, progress + increment);

                            float dx = firstPoint.X - secondPoint.X;
                            float dy = firstPoint.Y - secondPoint.Y;
                            float distance = (float)Math.Sqrt(dx * dx + dy * dy) + 1f;

                            Vector2 delta = firstPoint - secondPoint;
                            double theta = Math.Atan2(delta.X, delta.Y);

                            OsbSprite sprite = layer.CreateSprite(spritePath, OsbOrigin.BottomCentre, firstPoint);
                            sprite.Fade(endtime, 0);
                            columnSprites.Add(sprite);

                            movement.Add(currentTime, firstPoint);
                            scale.Add(currentTime, new Vector2(4, distance));
                            rotation.Add(currentTime, -theta);

                            movementPerSprite.Add(movement);
                            scalePerSprite.Add(scale);
                            rotationPerSprite.Add(rotation);

                            progress += increment;
                        }
                        break;

                    case PathType.line:
                        for (int n = 0; n < notePath.Count - 1; n++)
                        {

                            var movement = new KeyframedValue<Vector2>(null);
                            var scale = new KeyframedValue<Vector2>(null);
                            var rotation = new KeyframedValue<double>(null);

                            Vector2 firstPoint = notePath[n].position;
                            Vector2 secondPoint = notePath[n + 1].position;

                            float dx = firstPoint.X - secondPoint.X;
                            float dy = firstPoint.Y - secondPoint.Y;
                            float distance = (float)Math.Sqrt(dx * dx + dy * dy) + 1f;

                            Vector2 delta = firstPoint - secondPoint;
                            double theta = Math.Atan2(delta.X, delta.Y);

                            OsbSprite sprite = layer.CreateSprite(spritePath, OsbOrigin.BottomCentre, firstPoint);
                            sprite.Fade(endtime, 0);

                            movement.Add(currentTime, firstPoint);
                            scale.Add(currentTime, new Vector2(4, distance));
                            rotation.Add(currentTime, -theta);

                            columnSprites.Add(sprite);

                            movementPerSprite.Add(movement);
                            scalePerSprite.Add(scale);
                            rotationPerSprite.Add(rotation);

                        }
                        break;
                }

                pathSprites.Add(currentColumn, columnSprites);

                movementPerSpriteByColumn.Add(currentColumn, movementPerSprite);
                scalePerSpriteByColumn.Add(currentColumn, scalePerSprite);
                rotationPerSpriteByColumn.Add(currentColumn, rotationPerSprite);
            }

            this.pathWaySprites = pathSprites;

            while (currentTime <= endtime)
            {
                double localIterationRate = 1000 / updatesPerSecond;
                currentTime += localIterationRate;

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    List<Anchor> notePath = notePathByColumn[currentColumn];
                    List<Vector2> points = GetPathAnchorVectors(notePath, currentTime);

                    var movementPerSprite = movementPerSpriteByColumn[currentColumn];
                    var scalePerSprite = scalePerSpriteByColumn[currentColumn];
                    var rotationPerSprite = rotationPerSpriteByColumn[currentColumn];

                    List<double> currentTheta = new List<double>(); ;


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

                                double priorTheta = 0f;

                                if (currentTheta.Count - 1 > i)
                                {
                                    priorTheta = currentTheta[i];
                                }

                                if (priorTheta > 0.02f && Math.Abs(Math.Abs(priorTheta) - Math.Abs(theta)) > Math.PI / 4)
                                {
                                    theta = priorTheta;
                                }

                                if (currentTheta.Count - 1 > i)
                                {
                                    currentTheta.Add(-theta);
                                }

                                rotationPerSprite[i].Add(currentTime, -theta);
                                movementPerSprite[i].Add(currentTime + localIterationRate, firstPoint);
                                scalePerSprite[i].Add(currentTime + localIterationRate, new Vector2(4, distance));

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

                                rotationPerSprite[i].Add(currentTime, -theta);
                                movementPerSprite[i].Add(currentTime + localIterationRate, firstPoint);
                                scalePerSprite[i].Add(currentTime + localIterationRate, new Vector2(4, distance));

                                i++;
                            }
                            break;
                    }
                }
            }

            foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
            {

                if (currentColumn == ColumnType.all)
                    continue;

                var movementPerSprite = movementPerSpriteByColumn[currentColumn];
                var scalePerSprite = scalePerSpriteByColumn[currentColumn];
                var rotationPerSprite = rotationPerSpriteByColumn[currentColumn];

                List<OsbSprite> sprites = pathSprites[currentColumn];

                for (int i = 0; i < movementPerSprite.Count; i++)
                {

                    OsbSprite sprite = sprites[i];

                    var movement = movementPerSprite[i];
                    var scale = scalePerSprite[i];
                    var rotation = rotationPerSprite[i];

                    movement.Simplify2dKeyframes(0.75f, v => v);
                    scale.Simplify2dKeyframes(0.25f, v => v);
                    //rotation.Simplify1dKeyframes(0.05f, v => (float)v); // this shit dont look good / dont work properly should instead do some value clamping to avoid instant 180° from pathway since that will result in shit.

                    movement.ForEachPair((start, end) => sprite.Move(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));
                    scale.ForEachPair((start, end) => sprite.ScaleVec(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));
                    rotation.ForEachPair((start, end) => sprite.Rotate(end.Time, end.Value));

                }


            }

            return debug;

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

        public void RenderReceptor(Column column, double duration)
        {

            KeyframedValue<Vector2> movement = new KeyframedValue<Vector2>(null);
            KeyframedValue<Vector2> scale = new KeyframedValue<Vector2>(null);
            KeyframedValue<double> rotation = new KeyframedValue<double>(null);

            double currentTime = starttime;
            double endTime = starttime + duration;
            double iterationLenght = 1000 / updatesPerSecond;

            Receptor receptor = column.receptor;
            Vector2 currentPosition = receptor.getCurrentPosition(currentTime);

            receptor.renderedSprite.Fade(starttime - 2500, 0);
            receptor.renderedSprite.Fade(starttime, 1);
            receptor.renderedSprite.Fade(endTime, 0);

            movement.Add(currentTime, currentPosition);

            var foundEntry = findEffectByReferenceTime(currentTime);

            if (foundEntry.Value != null)
            {
                receptor.RenderTransformed(currentTime, endTime, foundEntry.Value.reference);
            }
            else
            {
                receptor.Render(currentTime, endTime);
            }

            while (currentTime < endTime)
            {

                foundEntry = findEffectByReferenceTime(currentTime);

                if (foundEntry.Value != null)
                {
                    receptor.RenderTransformed(currentTime, endTime, foundEntry.Value.reference);
                }

                OsbSprite renderedReceptor = receptor.renderedSprite;

                FadeEffect receptorFade = findFadeAtTime(currentTime);
                if (receptorFade != null)
                {
                    if (renderedReceptor.OpacityAt(currentTime) != receptorFade.value)
                        renderedReceptor.Fade(currentTime, receptorFade.value);
                }

                Vector2 newPosition = receptor.getCurrentPosition(currentTime);

                movement.Add(currentTime, newPosition);
                scale.Add(currentTime, receptor.receptorSprite.ScaleAt(currentTime));
                rotation.Add(currentTime, receptor.receptorSprite.RotationAt(currentTime));
                currentTime += iterationLenght;
            }

            //movement.Simplify2dKeyframes(ReceptorMovementPrecision, v => v);
            //scale.Simplify2dKeyframes(ReceptorScalePrecision, v => v);
            //rotation.Simplify1dKeyframes(ReceptorRotationPrecision, v => (float)v);
            scale.ForEachPair((start, end) => receptor.renderedSprite.ScaleVec(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));
            movement.ForEachPair((start, end) => receptor.renderedSprite.Move(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));
            rotation.ForEachPair((start, end) => receptor.renderedSprite.Rotate(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));

        }

        private KeyValuePair<double, EffectInfo> findEffectByReferenceTime(double time)
        {
            KeyValuePair<double, EffectInfo> currentEffect = playfieldInstance.effectReferenceByStartTime
                   .Where(kvp => kvp.Key <= time)
                   .OrderByDescending(kvp => kvp.Key)
                   .FirstOrDefault();

            return currentEffect;
        }

        private double findCurrentUpdateRate(double time)
        {
            double iterationrate = this.iterationLength;

            KeyValuePair<double, double> currentUpdateRate = updatesPerSecondDictionary
                   .Where(kvp => kvp.Key <= time)
                   .OrderByDescending(kvp => kvp.Key)
                   .FirstOrDefault();

            if (currentUpdateRate.Value != 0)
            {
                iterationrate = 1000 / currentUpdateRate.Value;
            }

            return Math.Round(iterationrate);
        }

        private FadeEffect findFadeAtTime(double time)
        {

            KeyValuePair<double, FadeEffect> currentFadeEffect = this.playfieldInstance.fadeAtTime
                   .Where(kvp => kvp.Key <= time)
                   .OrderByDescending(kvp => kvp.Key)
                   .FirstOrDefault();

            return currentFadeEffect.Value;
        }

        public void changeUpdateRate(double time, double updatesPerSecond)
        {
            updatesPerSecondDictionary.Add(Math.Max(time - this.easetime, 0), updatesPerSecond);
        }

        public double nextNoteToHit(double referenceTime, bool hideNotes, bool hideHolds)
        {

            double earliestNote = 0;

            foreach (var currerntColumn in playfieldInstance.columnNotes.Values)
            {

                //var filteredHolds = currerntColumn.Where(kvp => hideHolds == true && kvp.Value.isSlider == false);
                //var filteredNotes = filteredHolds.Where(kvp => hideNotes == true &&  kvp.Value.isSlider == true);
                double newTime = currerntColumn.Where(kvp => kvp.Key > referenceTime).OrderBy(kvp => kvp.Key).FirstOrDefault().Key;

                if (earliestNote == 0)
                {
                    earliestNote = newTime;
                }
                else if (earliestNote > newTime)
                {
                    earliestNote = newTime;
                }

            }


            return earliestNote;

        }
    }
}