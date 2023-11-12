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
    public static class RenderOrigin
    {

        public static void Render(DrawInstance instance, Column column)
        {

            Playfield playfieldInstance = instance.playfieldInstance;

            KeyframedValue<Vector2> movement = new KeyframedValue<Vector2>(null);

            NoteOrigin origin = column.origin;

            double relativeTime = playfieldInstance.starttime;

            var pos = origin.PositionAt(relativeTime);
            
            while (relativeTime <= playfieldInstance.endtime)
            {
                movement.Add(relativeTime, origin.PositionAt(relativeTime));
                relativeTime += playfieldInstance.delta;
            }

            movement.Simplify2dKeyframes(1, v => v);
            movement.ForEachPair((start, end) => origin.originSprite.Move(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));

        }

    }
}