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

            //float x = pos.X;
            //float y = pos.Y;

            while (relativeTime <= playfieldInstance.endtime)
            {
                Vector2 position = origin.PositionAt(relativeTime);



                movement.Add(relativeTime, position);


                relativeTime += playfieldInstance.delta;
            }

            movement.Simplify(1);
            movement.ForEachPair((start, end) =>
            {
                origin.originSprite.Move(OsbEasing.None, start.Time, end.Time, start.Value, end.Value);
            });

        }

    }
}