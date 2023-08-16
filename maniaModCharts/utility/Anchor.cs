using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{
    public class Anchor
    {
        public int type = 0;
        public Vector2 position;
        public Vector2 offset;
        public OsbSprite sprite;
        public bool debug = false;
        public ColumnType column;

        public Anchor(int type, ColumnType column, Vector2 initialPosition, Vector2 offset, bool debug, StoryboardLayer layer)
        {

            OsbSprite debugSprite = layer.CreateSprite("sb/white.png", OsbOrigin.Centre, position);
            if (debug)
            {
                debugSprite.Fade(0, 1);
                debugSprite.Fade(300000, 0);
            }
            else
            {
                debugSprite.Fade(0, 0);
            }

            this.sprite = debugSprite;

            this.debug = debug;
            this.position = initialPosition;
            this.offset = offset;
            this.type = type;
            this.column = column;

        }

        public Anchor ManipulatePosition(double starttime, double transitionTime, OsbEasing easing, Vector2 newPosition)
        {

            OsbSprite sprite = this.sprite;
            sprite.Move(easing, starttime, starttime + transitionTime, this.position, newPosition);


            this.position = newPosition;


            return this;

        }

        public void MoveAnchor(double time, Vector2 newPosition)
        {
            OsbSprite sprite = this.sprite;

            sprite.Move(time, newPosition);
        }
    }
}