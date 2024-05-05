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
    [Obsolete("")]
    public static class ByAnchors
    {

        public static string drawNotesByAnchors(DrawInstance instance, double duration, PathType type = PathType.line)
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

            Parallel.ForEach(playfieldInstance.columns.Values, column =>
            {

                RenderReceptor.Render(instance, column, duration);
                RenderOrigin.Render(instance, column);

                List<Anchor> notePath = instance.notePathByColumn[column.type];
                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime - easetime <= endtime).ToList();

                Parallel.ForEach(keysInRange, key =>
                {

                    KeyframedValue<Vector2> movement = new KeyframedValue<Vector2>(null);
                    KeyframedValue<Vector2> scale = new KeyframedValue<Vector2>(null);
                    KeyframedValue<double> rotation = new KeyframedValue<double>(null);

                    Note note = notes[key];
                    double totalDuration = easetime;
                    double localIterationRate = instance.findCurrentUpdateRate(note.starttime - easetime);

                    double currentTime = note.starttime - easetime - localIterationRate;
                    double renderStartTime = Math.Max(currentTime, starttime);
                    double renderEndTime = Math.Min(note.endtime, endtime);
                    Vector2 currentPosition = column.origin.PositionAt(currentTime);
                    float progress = 0f;
                    double iteratedTime = 0;
                    float initialFade = 1f;
                    note.invisible(currentTime - 1);

                    FadeEffect noteFade = instance.findFadeAtTime(currentTime);
                    if (noteFade != null)
                    {
                        initialFade = noteFade.value;
                    }

                    var currentEffect = instance.findEffectByReferenceTime(currentTime);
                    if (currentEffect.Value != null)
                    {
                        note.RenderTransformed(renderStartTime, renderEndTime, currentEffect.Value.reference, fadeInTime, fadeOutTime);
                    }
                    else
                    {
                        note.Render(renderStartTime, renderEndTime, easing, initialFade, fadeInTime, fadeOutTime);
                    }

                    double startRotation = note.getRotation(currentTime);
                    note.Scale(currentTime, currentTime, OsbEasing.None, column.origin.ScaleAt(currentTime), column.origin.ScaleAt(currentTime));

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

                                    noteFade = instance.findFadeAtTime(currentTime);
                                    if (noteFade != null)
                                    {
                                        note.Fade(currentTime, currentTime, noteFade.easing, noteFade.value);
                                    }

                                    progress = (float)(iteratedTime / timePerAnchor);

                                    // Apply easing to the progress
                                    float easedProgress = (float)easing.Ease(progress);

                                    currentEffect = instance.findEffectByReferenceTime(currentTime);
                                    if (currentEffect.Value != null)
                                    {
                                        note.UpdateTransformed(currentTime, currentTime + localIterationRate, currentEffect.Value.reference, 10);
                                    }

                                    Vector2 startPos = notePath[n].sprite.PositionAt(currentTime + localIterationRate);
                                    Vector2 endPos = notePath[n + 1].sprite.PositionAt(currentTime + localIterationRate);

                                    Vector2 newPosition = Vector2.Lerp(startPos, endPos, easedProgress);
                                    Vector2 receptorPosition = column.receptor.PositionAt(currentTime + localIterationRate);
                                    Vector2 originScale = column.origin.ScaleAt(currentTime + localIterationRate);
                                    Vector2 receptorScale = column.receptor.ScaleAt(currentTime + localIterationRate);
                                    Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, easedProgress);
                                    startRotation = column.receptor.RotationAt(currentTime + localIterationRate);

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

                                    // Apply easing to the progress
                                    float easedProgress = (float)easing.Ease(progress);

                                    currentEffect = instance.findEffectByReferenceTime(currentTime);
                                    if (currentEffect.Value != null)
                                    {
                                        note.UpdateTransformed(currentTime, currentTime + localIterationRate, currentEffect.Value.reference, 10);
                                    }

                                    Vector2 startPos = notePath[n].sprite.PositionAt(currentTime + localIterationRate);
                                    Vector2 endPos = notePath[n + 1].sprite.PositionAt(currentTime + localIterationRate);

                                    Vector2 newPosition = Vector2.Lerp(startPos, endPos, easedProgress);
                                    Vector2 receptorPosition = column.receptor.PositionAt(currentTime + localIterationRate);
                                    Vector2 originScale = column.origin.ScaleAt(currentTime + localIterationRate);
                                    Vector2 receptorScale = column.receptor.ScaleAt(currentTime + localIterationRate);
                                    Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, easedProgress);

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

                                // Apply easing to the progress
                                float easedProgress = (float)easing.Ease(progress);

                                Vector2 receptorPosition = column.ReceptorPositionAt(currentTime);
                                Vector2 originScale = column.origin.ScaleAt(currentTime);
                                Vector2 receptorScale = column.receptor.ScaleAt(currentTime);
                                Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, easedProgress);
                                List<Vector2> points = instance.GetPathAnchorVectors(notePath, currentTime);
                                Vector2 newPosition = BezierCurve.CalculatePoint(points, easedProgress);

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
                    Parallel.ForEach(note.sliderPositions, part =>
                         {

                             KeyframedValue<Vector2> SliderMovement = new KeyframedValue<Vector2>(null);
                             KeyframedValue<Vector2> SliderScale = new KeyframedValue<Vector2>(null);
                             KeyframedValue<double> SliderRotation = new KeyframedValue<double>(null);

                             double sliderStartime = part.Timestamp;
                             OsbSprite sprite = part.Sprite;
                             double sliderCurrentTime = sliderStartime - easetime - localIterationRate;
                             Vector2 currentSliderPositon = column.origin.PositionAt(sliderCurrentTime);
                             double sliderRenderStartTime = Math.Max(sliderCurrentTime, sliderStartime);
                             double sliderRenderEndTime = Math.Min(sliderStartime + 0.1f, endtime);

                             sprite.Fade(sliderCurrentTime - 1000, 0);

                             sprite.Fade(Math.Max(sliderCurrentTime, renderStartTime), 1);
                             sprite.Fade(sliderRenderEndTime, 0);
                             double sliderRotation = sprite.RotationAt(sliderCurrentTime);

                             float defaultScaleX = 0.7f / 0.5f;
                             float defaultScaleY = 0.14f / 0.5f * ((float)part.Duration / 20f); // This scaled was based on 20ms long sliderParts

                             Vector2 newScale = new Vector2(defaultScaleX * column.origin.ScaleAt(sliderCurrentTime).X, defaultScaleY * column.origin.ScaleAt(sliderCurrentTime).Y);
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

                                             // Apply easing to the progress
                                             float easedProgress = (float)easing.Ease(sliderProgress);

                                             currentEffect = instance.findEffectByReferenceTime(sliderCurrentTime);
                                             if (currentEffect.Value != null)
                                             {
                                                 //sprite.UpdateTransformed(sliderCurrentTime, sliderCurrentTime + movementTime, currentEffect.Value.reference, 10);
                                             }

                                             Vector2 startPos = notePath[n].sprite.PositionAt(sliderCurrentTime + localIterationRate);
                                             Vector2 endPos = notePath[n + 1].sprite.PositionAt(sliderCurrentTime + localIterationRate);

                                             Vector2 newPosition = Vector2.Lerp(startPos, endPos, easedProgress);
                                             Vector2 receptorPosition = column.receptor.PositionAt(sliderCurrentTime + localIterationRate);
                                             Vector2 originScale = column.origin.ScaleAt(sliderCurrentTime + localIterationRate);
                                             Vector2 receptorScale = column.receptor.ScaleAt(sliderCurrentTime + localIterationRate);
                                             Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, easedProgress);

                                             double theta = 0;
                                             Vector2 delta = currentSliderPositon - newPosition;
                                             if (delta.LengthSquared > 0 && rotateToFaceReceptor)
                                             {
                                                 theta = Math.Atan2(delta.X, delta.Y);
                                             }

                                             // If the note is already done
                                             if (sliderCurrentTime > note.starttime)
                                             {
                                                 Vector2 newNotePosition = column.receptor.PositionAt(sliderCurrentTime + localIterationRate);

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

                                             // Apply easing to the progress
                                             float easedProgress = (float)easing.Ease(sliderProgress);

                                             currentEffect = instance.findEffectByReferenceTime(sliderCurrentTime);
                                             if (currentEffect.Value != null)
                                             {
                                                 //sprite.UpdateTransformed(sliderCurrentTime, sliderCurrentTime + movementTime, currentEffect.Value.reference, 10);
                                             }

                                             Vector2 startPos = notePath[n].sprite.PositionAt(sliderCurrentTime + localIterationRate);
                                             Vector2 endPos = notePath[n + 1].sprite.PositionAt(sliderCurrentTime + localIterationRate);

                                             Vector2 newPosition = Vector2.Lerp(startPos, endPos, easedProgress);
                                             Vector2 receptorPosition = column.receptor.PositionAt(sliderCurrentTime + localIterationRate);
                                             Vector2 originScale = column.origin.ScaleAt(sliderCurrentTime + localIterationRate);
                                             Vector2 receptorScale = column.receptor.ScaleAt(sliderCurrentTime + localIterationRate);
                                             Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, easedProgress);

                                             double theta = 0;
                                             Vector2 delta = currentSliderPositon - newPosition;
                                             if (delta.LengthSquared > 0 && rotateToFaceReceptor)
                                             {
                                                 theta = Math.Atan2(delta.X, delta.Y);
                                             }

                                             // If the note is already done
                                             if (sliderCurrentTime > note.starttime)
                                             {
                                                 Vector2 newNotePosition = column.receptor.PositionAt(sliderCurrentTime + localIterationRate);

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

                                         // Apply easing to the progress
                                         float easedProgress = (float)easing.Ease(sliderProgress);

                                         Vector2 receptorPosition = column.ReceptorPositionAt(sliderCurrentTime + localIterationRate);
                                         Vector2 originScale = column.origin.ScaleAt(sliderCurrentTime + localIterationRate);
                                         Vector2 receptorScale = column.receptor.ScaleAt(sliderCurrentTime + localIterationRate);
                                         Vector2 scaleProgress = Vector2.Lerp(originScale, receptorScale, easedProgress);
                                         List<Vector2> points = instance.GetPathAnchorVectors(notePath, sliderCurrentTime);

                                         Vector2 newPosition = BezierCurve.CalculatePoint(points, easedProgress);

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
                                             Vector2 newNotePosition = column.receptor.PositionAt(sliderCurrentTime + localIterationRate);

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
                             SliderMovement.Simplify(instance.HoldMovementPrecision);
                             SliderScale.Simplify(instance.HoldScalePrecision);
                             SliderRotation.Simplify(instance.HoldRotationPrecision);
                             SliderMovement.ForEachPair((start, end) => sprite.Move(easing, start.Time, end.Time, start.Value, end.Value));
                             SliderScale.ForEachPair((start, end) => sprite.ScaleVec(start.Time, end.Time, start.Value.X, start.Value.Y, end.Value.X, end.Value.Y));
                             SliderRotation.ForEachPair((start, end) => sprite.Rotate(start.Time, end.Value));

                         });

                    // Render out Note keyframes
                    movement.Simplify(instance.NoteMovementPrecision);
                    scale.Simplify(instance.NoteScalePrecision);
                    rotation.Simplify(instance.NoteRotationPrecision);
                    movement.ForEachPair((start, end) => note.Move(start.Time, end.Time - start.Time, OsbEasing.None, start.Value, end.Value));
                    scale.ForEachPair((start, end) => note.Scale(start.Time, end.Time, OsbEasing.None, start.Value, end.Value));
                    rotation.ForEachPair((start, end) => note.AbsoluteRotate(start.Time, end.Time - start.Time, OsbEasing.None, end.Value));

                    if (progress == 1)
                    {
                        //note.ApplyHitLightingToNote(note.starttime, note.endtime, fadeOutTime, column.receptor, localIterationRate);
                    }
                });
            });

            return debug;
        }

    }
}