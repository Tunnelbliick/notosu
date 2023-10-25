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
    public static class RenderReceptor
    {

        public static void Render(DrawInstance instance, Column column, double duration)
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

            KeyframedValue<Vector2> movement = new KeyframedValue<Vector2>(null);
            KeyframedValue<Vector2> scale = new KeyframedValue<Vector2>(null);
            KeyframedValue<double> rotation = new KeyframedValue<double>(null);

            double currentTime = starttime;
            double endTime = starttime + duration;
            double iterationLenght = 1000 / instance.updatesPerSecond;

            Receptor receptor = column.receptor;
            Vector2 currentPosition = receptor.getCurrentPosition(currentTime);

            receptor.renderedSprite.Fade(starttime - 2500, 0);
            receptor.renderedSprite.Fade(starttime, 1);
            receptor.renderedSprite.Fade(endTime, 0);

            movement.Add(currentTime, currentPosition);

            var foundEntry = instance.findEffectByReferenceTime(currentTime);

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

                foundEntry = instance.findEffectByReferenceTime(currentTime);

                if (foundEntry.Value != null)
                {
                    receptor.RenderTransformed(currentTime, endTime, foundEntry.Value.reference);
                }

                OsbSprite renderedReceptor = receptor.renderedSprite;

                FadeEffect receptorFade = instance.findFadeAtTime(currentTime);
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

    }
}