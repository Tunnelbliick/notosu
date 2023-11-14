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

            KeyframedValue<float> movementX = new KeyframedValue<float>(null);
            KeyframedValue<float> movementY = new KeyframedValue<float>(null);

            NoteOrigin origin = column.origin;

            double relativeTime = playfieldInstance.starttime;

            var pos = origin.PositionAt(relativeTime);

            float x = pos.X;
            float y = pos.Y;

            while (relativeTime <= playfieldInstance.endtime)
            {
                Vector2 position = origin.PositionAt(relativeTime);

                x = position.X;
                y = position.Y;

                if (relativeTime >= 15157 && relativeTime <= 15473)
                {
                    //Utility.Log($"{relativeTime} - {position} - {x}/{y}");
                }

                movementX.Add(relativeTime, x);
                movementY.Add(relativeTime, y);

                relativeTime += playfieldInstance.delta;
            }

            movementX.Simplify1dKeyframes(1, v => v);
            movementY.Simplify1dKeyframes(1, v => v);
            movementX.ForEachPair((start, end) => origin.originSprite.MoveX(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));
            movementY.ForEachPair((start, end) => origin.originSprite.MoveY(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));

        }

    }
}