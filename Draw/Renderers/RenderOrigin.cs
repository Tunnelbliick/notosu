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

            double relativeTime = playfieldInstance.starttime + 1;

            var pos = origin.PositionAt(relativeTime);

            float lastX = pos.X;
            float lastY = pos.Y;

            while (relativeTime <= playfieldInstance.endtime)
            {
                float x = lastX;
                float y = lastY;

                if (origin.positionX.ContainsKey(relativeTime))
                {
                    x = origin.positionX[relativeTime];
                    lastX = x;
                }

                if (origin.positionY.ContainsKey(relativeTime))
                {
                    y = origin.positionY[relativeTime];
                    lastX = y;
                }

                movement.Add(relativeTime, new Vector2(x, y));

                relativeTime += playfieldInstance.delta;
            }

            movement.Simplify2dKeyframes(1, v => v);
            movement.ForEachPair((start, end) => origin.originSprite.Move(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));

        }

    }
}