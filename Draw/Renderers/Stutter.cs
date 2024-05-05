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
    [Obsolete("Deprecated use RenderViaEquation instead")]
    public static class Stutter
    {

        static Playfield playfieldInstance;

        public static string drawNotesStutteredByOriginToReceptor(DrawInstance instance, double duration, bool renderReceptor = true)
        {
            playfieldInstance = instance.playfieldInstance;
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

                if (renderReceptor)
                    RenderReceptor.Render(instance, column, duration);

                RenderOrigin.Render(instance, column);

                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime - easetime <= endtime).ToList();

                Parallel.ForEach(keysInRange, key =>
                {

                    KeyframedValue<Vector2> movement = new KeyframedValue<Vector2>(null);
                    KeyframedValue<Vector2> scale = new KeyframedValue<Vector2>(null);
                    KeyframedValue<double> rotation = new KeyframedValue<double>(null);

                    Note note = notes[key];

                    if (note.isSlider == false && hideNormalNotes)
                    {
                        return;
                    }

                    if (note.isSlider == true && hideHolds)
                    {
                        return;
                    }

                    double totalDuration = easetime;

                    double currentTime = note.starttime - easetime;

                    double renderStartTime = Math.Max(currentTime, starttime);
                    double renderEndTime = Math.Min(note.endtime, endtime);
                    Vector2 currentPosition = column.origin.PositionAt(currentTime);
                    float progress = 0f;
                    double iteratedTime = 0;
                    float initialFade = 1;
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

                    do
                    {
                        double nextNoteTime = nextNoteToHit(currentTime, hideNormalNotes, hideHolds);

                        if (nextNoteTime == 0)
                        {
                            nextNoteTime = currentTime;
                        }

                        noteFade = instance.findFadeAtTime(currentTime);
                        if (noteFade != null)
                        {
                            note.Fade(currentTime, currentTime, noteFade.easing, noteFade.value);
                        }

                        double timeleft = note.starttime - currentTime;
                        double elapsedTime = currentTime - note.starttime;

                        currentEffect = instance.findEffectByReferenceTime(currentTime);
                        if (currentEffect.Value != null)
                        {
                            note.UpdateTransformed(currentTime, nextNoteTime, currentEffect.Value.reference, 10);
                        }

                        progress = Math.Min((float)(iteratedTime / easetime), 1);
                        Vector2 originPosition = column.origin.PositionAt(nextNoteTime);
                        Vector2 receptorPosition = column.receptor.PositionAt(nextNoteTime);
                        Vector2 newPosition = Vector2.Lerp(originPosition, receptorPosition, progress);
                        Vector2 originScale = column.origin.ScaleAt(nextNoteTime);
                        Vector2 receptorScale = column.receptor.ScaleAt(nextNoteTime);
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

                    Parallel.ForEach(note.sliderPositions, part =>
                         {

                             KeyframedValue<Vector2> SliderMovement = new KeyframedValue<Vector2>(null);
                             KeyframedValue<Vector2> SliderScale = new KeyframedValue<Vector2>(null);
                             KeyframedValue<double> SliderRotation = new KeyframedValue<double>(null);

                             double sliderIterationLenght = instance.findCurrentUpdateRate(part.Timestamp - easetime);

                             double sliderStartime = part.Timestamp;
                             OsbSprite sprite = part.Sprite;
                             double sliderCurrentTime = sliderStartime - easetime + part.Duration / 2;
                             Vector2 currentSliderPositon = column.origin.PositionAt(sliderCurrentTime);
                             double sliderRenderStartTime = Math.Max(sliderStartime - easetime, starttime);
                             double sliderRenderEndTime = Math.Min(sliderStartime, endtime);
                             float sliderProgress = 0;
                             double sliderIteratedTime = 0;
                             //sprite.Fade(sliderCurrentTime, 0);


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

                             SliderScale.Add(sliderCurrentTime, newScale, EasingFunctions.ToEasingFunction(easing));
                             SliderRotation.Add(sliderCurrentTime, sliderRotation, EasingFunctions.ToEasingFunction(easing));

                             do
                             {

                                 double nextNoteTime = nextNoteToHit(sliderCurrentTime, hideNormalNotes, hideHolds);

                                 if (nextNoteTime == 0)
                                 {
                                     nextNoteTime = sliderCurrentTime;
                                 }

                                 sliderFade = instance.findFadeAtTime(sliderCurrentTime);
                                 if (sliderFade != null)
                                 {
                                     if (sprite.OpacityAt(sliderCurrentTime) != sliderFade.value)
                                         sprite.Fade(sliderFade.easing, sliderCurrentTime, sliderCurrentTime, sprite.OpacityAt(sliderCurrentTime), sliderFade.value);
                                 }

                                 double timeleft = sliderStartime - sliderCurrentTime;
                                 sliderProgress = Math.Min((float)(sliderIteratedTime / easetime), 1);

                                 Vector2 originPosition = column.origin.PositionAt(nextNoteTime);
                                 Vector2 receptorPosition = column.receptor.PositionAt(nextNoteTime);
                                 Vector2 newPosition = Vector2.Lerp(originPosition, receptorPosition, sliderProgress);
                                 Vector2 receptorScale = column.receptor.ScaleAt(nextNoteTime);
                                 Vector2 renderedReceptorPosition = column.receptor.renderedSprite.PositionAt(sliderCurrentTime);

                                 double theta = 0;
                                 Vector2 delta = renderedReceptorPosition - currentSliderPositon;

                                 if (currentSliderPositon.Y > renderedReceptorPosition.Y)
                                 {
                                     delta = -delta;
                                 }

                                 theta = Math.Atan2(delta.X, delta.Y);

                                 newScale = new Vector2(defaultScaleX * column.origin.ScaleAt(sliderCurrentTime).X, defaultScaleY * column.origin.ScaleAt(sliderCurrentTime).Y);

                                 sprite.Move(sliderCurrentTime, sliderCurrentTime, currentSliderPositon, newPosition);

                                 SliderScale.Add(sliderCurrentTime, newScale, EasingFunctions.ToEasingFunction(easing));
                                 SliderRotation.Add(sliderCurrentTime, sliderRotation - theta, EasingFunctions.ToEasingFunction(easing));

                                 // If the note is already done
                                 if (sliderCurrentTime >= note.starttime)
                                 {
                                     Vector2 newNotePosition = column.receptor.PositionAt(sliderCurrentTime);
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
                             SliderScale.Simplify(instance.HoldScalePrecision);
                             SliderRotation.Simplify(instance.HoldRotationPrecision);
                             SliderScale.ForEachPair((start, end) => sprite.ScaleVec(start.Time, end.Time, start.Value.X, start.Value.Y, end.Value.X, end.Value.Y));
                             SliderRotation.ForEachPair((start, end) => sprite.Rotate(start.Time, start.Value));

                         });

                    // Render out Note keyframes
                    scale.Simplify(instance.NoteScalePrecision);
                    rotation.Simplify(instance.NoteRotationPrecision);
                    scale.ForEachPair((start, end) => note.Scale(start.Time, end.Time, OsbEasing.None, start.Value, end.Value));
                    rotation.ForEachPair((start, end) => note.AbsoluteRotate(start.Time, end.Time - start.Time, OsbEasing.None, end.Value));

                    if (progress == 1)
                    {
                        //note.ApplyHitLightingToNote(note.starttime, note.endtime, fadeOutTime, column.receptor);
                    }

                });
            });

            return debug;
        }

        public static double nextNoteToHit(double referenceTime, bool hideNotes, bool hideHolds)
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