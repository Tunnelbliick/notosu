using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using storyboard.scriptslibrary.maniaModCharts.effects;
using StorybrewCommon.Animations;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{

    public class EquationParameters
    {
        public Vector2 position;
        public double time;
        public float progress;
        public OsbSprite sprite;

        public SliderParts part;
        public Note note;
        public Column column;
        public bool isHoldBody = false;
    }

    public delegate Vector2 EquationFunction(EquationParameters parameters);

    public class ByEquation
    {

        public static string drawViaEquation(DrawInstance instance, double duration, EquationFunction equation, bool renderReceptor = true)
        {
            Playfield playfieldInstance = instance.playfieldInstance;
            bool hideNormalNotes = instance.hideNormalNotes;
            bool hideHolds = instance.hideHolds;
            bool rotateToFaceReceptor = instance.rotateToFaceReceptor;
            double starttime = instance.starttime;
            double endtime = starttime + duration;
            double easetime = instance.easetime;
            OsbEasing easing = instance.easing;
            double fadeInTime = instance.fadeInTime;
            double fadeOutTime = instance.fadeOutTime;
            string debug = "";


            foreach (Column column in playfieldInstance.columns.Values)
            {
                if (renderReceptor)
                    RenderReceptor.Render(instance, column, duration);

                RenderOrigin.Render(instance, column);

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

                    double localIterationRate = instance.findCurrentUpdateRate(note.starttime - easetime);

                    double currentTime = note.starttime - easetime - localIterationRate;
                    double renderStartTime = Math.Max(currentTime, starttime);
                    double renderEndTime = Math.Min(note.endtime, endtime);
                    Vector2 currentPosition = column.origin.PositionAt(currentTime);
                    float progress = 0f;
                    double iteratedTime = 0;
                    float initialFade = 1;
                    note.invisible(currentTime - 1);
                    if (playfieldInstance.isColored)
                        note.Color(currentTime - 1, instance.color);

                    FadeEffect noteFade = instance.findFadeAtTime(currentTime);
                    if (noteFade != null)
                    {
                        initialFade = noteFade.value;
                    }

                    var currentEffect = instance.findEffectByReferenceTime(currentTime);
                    
                    // currently defunc
                    /*if (currentEffect.Value != null && currentEffect.Value.effektType == EffectType.TransformPlayfield3D)
                    {
                        note.RenderTransformed(renderStartTime, renderEndTime, currentEffect.Value.reference, fadeInTime, fadeOutTime);
                    }*/
                    if (currentEffect.Value == null || (currentEffect.Value != null && currentEffect.Value.effektType != EffectType.RenderPlayFieldFrom))
                    {
                        note.Render(renderStartTime, renderEndTime, easing, initialFade, fadeInTime, fadeOutTime);
                    }

                    note.Scale(currentTime, currentTime, OsbEasing.None, column.origin.ScaleAt(currentTime), column.origin.ScaleAt(currentTime));

                    double startRotation = note.getRotation(currentTime);

                    do
                    {

                        if (currentTime > endtime)
                        {
                            break;
                        }

                        noteFade = instance.findFadeAtTime(currentTime + localIterationRate);
                        if (noteFade != null)
                        {
                            note.Fade(currentTime, currentTime, noteFade.easing, noteFade.value);
                        }

                        double timeleft = note.starttime - currentTime;
                        double elapsedTime = currentTime - note.starttime;

                        currentEffect = instance.findEffectByReferenceTime(currentTime + localIterationRate);

                        // currently defunc
                        /*if (currentEffect.Value != null && currentEffect.Value.effektType == EffectType.TransformPlayfield3D)
                        {
                            note.UpdateTransformed(currentTime, currentTime + localIterationRate, currentEffect.Value.reference, 10);
                        }*/

                        progress = Math.Min((float)(iteratedTime / easetime), 1);

                        // Apply easing to the progress
                        float easedProgress = (float)easing.Ease(progress);

                        if (currentEffect.Value != null && currentEffect.Value.effektType == EffectType.RenderPlayFieldFrom)
                        {
                            if (easedProgress < currentEffect.Value.value)
                            {

                                Vector2 originPositionLocal = column.origin.PositionAt(currentTime + localIterationRate);
                                Vector2 receptorPositionLocal = column.receptor.PositionAt(currentTime + localIterationRate);
                                Vector2 newPositionLocal = Vector2.Lerp(originPositionLocal, receptorPositionLocal, easedProgress);

                                note.Fade(currentTime, currentTime, OsbEasing.None, 0);
                                iteratedTime += localIterationRate;
                                currentTime += localIterationRate;
                                currentPosition = newPositionLocal;
                                continue;
                            }
                            else
                            {
                                float fade = 1;
                                if (noteFade != null)
                                {
                                    fade = noteFade.value;
                                }
                                note.Fade(currentTime + localIterationRate, currentTime + localIterationRate + fadeInTime, OsbEasing.None, fade);
                            }
                        }

                        if (currentEffect.Value != null && currentEffect.Value.effektType == EffectType.RenderPlayFieldUntil)
                        {
                            if (currentEffect.Value.value == 0)
                            {
                                note.Fade(currentTime, currentTime, OsbEasing.None, 0);
                            }
                            if (easedProgress > currentEffect.Value.value)
                            {
                                note.Fade(currentTime, currentTime + fadeOutTime, OsbEasing.None, 0);
                            }
                        }

                        Vector2 originPosition = column.origin.PositionAt(currentTime + localIterationRate);
                        Vector2 receptorPosition = column.receptor.PositionAt(currentTime + localIterationRate);
                        Vector2 newPosition = Vector2.Lerp(originPosition, receptorPosition, easedProgress);

                        var par = new EquationParameters();
                        par.position = newPosition;
                        par.time = currentTime;
                        par.progress = easedProgress;
                        par.sprite = note.noteSprite;
                        par.note = note;
                        par.column = column;

                        newPosition = equation(par);
                        Vector2 originScale = column.origin.ScaleAt(currentTime + localIterationRate);
                        Vector2 receptorScale = column.receptor.ScaleAt(currentTime + localIterationRate);
                        Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, easedProgress);
                        debug += receptorScale;
                        startRotation = column.receptor.RotationAt(currentTime + localIterationRate);

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

                        double sliderIterationLenght = instance.findCurrentUpdateRate(part.Timestamp - easetime);

                        double sliderStartime = part.Timestamp;
                        OsbSprite sprite = part.Sprite;
                        double sliderCurrentTime = sliderStartime - easetime - sliderIterationLenght;
                        Vector2 currentSliderPositon = column.origin.PositionAt(sliderCurrentTime);
                        double sliderRenderStartTime = Math.Max(sliderStartime - easetime, starttime);
                        double sliderRenderEndTime = Math.Min(sliderStartime, endtime);
                        float sliderProgress = 0;
                        double sliderIteratedTime = 0;
                        sprite.Fade(sliderCurrentTime, 0);

                        FadeEffect sliderFade = instance.findFadeAtTime(sliderRenderStartTime);
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

                        Vector2 newScale = new Vector2(defaultScaleX * column.origin.ScaleAt(sliderCurrentTime).X, defaultScaleY * column.origin.ScaleAt(sliderCurrentTime).Y);

                        SliderMovement.Add(sliderCurrentTime, currentSliderPositon, EasingFunctions.ToEasingFunction(easing));
                        SliderScale.Add(sliderCurrentTime, newScale, EasingFunctions.ToEasingFunction(easing));

                        bool hasStartedRendering = false;
                        double prevTheta = 0;  // You need to store the previous theta between calls

                        do
                        {

                            if (sliderCurrentTime > endtime)
                            {
                                break;
                            }

                            sliderFade = instance.findFadeAtTime(sliderCurrentTime);
                            if (sliderFade != null)
                            {
                                if (sprite.OpacityAt(sliderCurrentTime) != sliderFade.value)
                                    sprite.Fade(sliderFade.easing, sliderCurrentTime, sliderCurrentTime, sprite.OpacityAt(sliderCurrentTime), sliderFade.value);
                            }

                            double timeleft = sliderStartime - sliderCurrentTime;
                            sliderProgress = Math.Min((float)(sliderIteratedTime / easetime), 1);
                            float sliderProgressNext = Math.Min((float)((sliderIteratedTime + sliderIterationLenght) / easetime), 1);

                            debug += sliderProgressNext;

                            // Apply easing to the progress
                            float easedProgress = (float)easing.Ease(sliderProgress);
                            float easedNextProgress = (float)easing.Ease(sliderProgressNext);

                            if (currentEffect.Value != null && currentEffect.Value.effektType == EffectType.RenderPlayFieldFrom)
                            {
                                if (easedProgress < currentEffect.Value.value)
                                {

                                    Vector2 originPositionLocal = column.origin.PositionAt(sliderCurrentTime + sliderIterationLenght);
                                    Vector2 receptorPositionLocal = column.receptor.PositionAt(sliderCurrentTime + sliderIterationLenght);
                                    Vector2 newPositionLocal = Vector2.Lerp(originPositionLocal, receptorPositionLocal, easedProgress);

                                    sprite.Fade(sliderCurrentTime, 0);
                                    sliderIteratedTime += sliderIterationLenght;
                                    sliderCurrentTime += sliderIterationLenght;
                                    currentSliderPositon = newPositionLocal;
                                }
                                else if (hasStartedRendering == false)
                                {
                                    hasStartedRendering = true;
                                    float fade = 1;
                                    if (noteFade != null)
                                    {
                                        fade = noteFade.value;
                                    }
                                    sprite.Fade(sliderCurrentTime + sliderIterationLenght, sliderCurrentTime + sliderIterationLenght + fadeInTime, 0, fade);
                                }
                            }

                            if (currentEffect.Value != null && currentEffect.Value.effektType == EffectType.RenderPlayFieldUntil)
                            {
                                if (easedProgress > currentEffect.Value.value)
                                {
                                    sprite.Fade(sliderCurrentTime, sliderCurrentTime + fadeOutTime, sprite.OpacityAt(currentTime), 0);
                                    break;
                                }
                            }

                            Vector2 originPosition = column.origin.PositionAt(sliderCurrentTime + sliderIterationLenght);
                            Vector2 receptorPosition = column.receptor.PositionAt(sliderCurrentTime + sliderIterationLenght);
                            Vector2 newPosition = Vector2.Lerp(originPosition, receptorPosition, easedProgress);
                            Vector2 nextPosition = Vector2.Lerp(originPosition, receptorPosition, easedNextProgress);

                            var par = new EquationParameters();
                            par.position = newPosition;
                            par.time = sliderCurrentTime;
                            par.progress = easedProgress;
                            par.sprite = sprite;
                            par.note = note;
                            par.part = part;
                            par.column = column;
                            par.isHoldBody = true;

                            var parNext = new EquationParameters();
                            parNext.position = nextPosition;
                            parNext.time = sliderCurrentTime;
                            parNext.progress = easedNextProgress;
                            parNext.sprite = null;
                            parNext.note = note;
                            parNext.column = column;
                            parNext.isHoldBody = true;

                            newPosition = equation(par);
                            nextPosition = equation(parNext);
                            Vector2 receptorScale = column.receptor.ScaleAt(sliderCurrentTime + sliderIterationLenght);
                            Vector2 renderedReceptorPosition = column.receptor.renderedSprite.PositionAt(sliderCurrentTime);

                            double theta = 0;

                            // Calculate the current theta
                            Vector2 delta = new Vector2(nextPosition.X - newPosition.X, nextPosition.Y - newPosition.Y);
                            double newTheta = Math.Atan2(delta.X, delta.Y);

                            // Check if the difference between the new theta and the previous theta exceeds 180°
                            double difference = newTheta - prevTheta;
                            if (difference > Math.PI)
                            {
                                newTheta -= 2 * Math.PI;
                            }
                            else if (difference < -Math.PI)
                            {
                                newTheta += 2 * Math.PI;
                            }

                            theta = newTheta;
                            prevTheta = newTheta;

                            // If the note is already done
                            if (sliderCurrentTime > note.starttime)
                            {
                                Vector2 newNotePosition = column.receptor.PositionAt(sliderCurrentTime + sliderIterationLenght);
                                movement.Add(sliderCurrentTime + sliderIterationLenght, newNotePosition, EasingFunctions.ToEasingFunction(easing));
                                scale.Add(sliderCurrentTime + sliderIterationLenght, receptorScale, EasingFunctions.ToEasingFunction(easing));

                                currentPosition = newNotePosition;
                            }

                            newScale = new Vector2(defaultScaleX * column.origin.ScaleAt(sliderCurrentTime).X, defaultScaleY * column.origin.ScaleAt(sliderCurrentTime).Y);

                            SliderMovement.Add(sliderCurrentTime + sliderIterationLenght, newPosition);
                            SliderScale.Add(sliderCurrentTime + sliderIterationLenght, newScale);
                            SliderRotation.Add(sliderCurrentTime + sliderIterationLenght, -theta);

                            sliderIteratedTime += sliderIterationLenght;
                            sliderCurrentTime += sliderIterationLenght;
                            currentSliderPositon = newPosition;

                        } while (sliderProgress < 1);

                        // Render out Slider keyframes
                        SliderMovement.Simplify2dKeyframes(instance.HoldMovementPrecision, v => v);
                        SliderScale.Simplify2dKeyframes(instance.HoldScalePrecision, v => v);
                        SliderRotation.Simplify1dKeyframes(instance.HoldRotationPrecision, v => (float)v);
                        SliderMovement.ForEachPair((start, end) => sprite.Move(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));
                        SliderScale.ForEachPair((start, end) => sprite.ScaleVec(start.Time, end.Time, start.Value.X, start.Value.Y, end.Value.X, end.Value.Y));
                        SliderRotation.ForEachPair((start, end) => sprite.Rotate(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));
                    }


                    // Render out Note keyframes
                    movement.Simplify2dKeyframes(instance.NoteMovementPrecision, v => v);
                    scale.Simplify2dKeyframes(instance.NoteScalePrecision, v => v);
                    rotation.Simplify1dKeyframes(instance.NoteRotationPrecision, v => (float)v);
                    movement.ForEachPair((start, end) => note.Move(start.Time, end.Time - start.Time, OsbEasing.None, start.Value, end.Value));
                    scale.ForEachPair((start, end) => note.Scale(start.Time, end.Time, OsbEasing.None, start.Value, end.Value));
                    rotation.ForEachPair((start, end) => note.Rotate(start.Time, end.Time - start.Time, OsbEasing.None, start.Value, end.Value));

                    if (progress == 1 && renderReceptor)
                    {
                        note.ApplyHitLightingToNote(note.starttime, note.endtime, fadeOutTime, column.receptor, localIterationRate);
                    }

                }
            }

            return debug;
        }
    }
}