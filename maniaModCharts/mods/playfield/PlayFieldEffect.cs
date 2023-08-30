using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{
    public class PlayFieldEffect : Mod
    {

        public Playfield field;

        public PlayFieldEffect(Playfield field, double starttime, double endtime, OsbEasing easing, int updatesPerSecond) : base(starttime, endtime, easing, updatesPerSecond) // Call the base class constructor
        {
            this.field = field;
        }

        public double SwapColumn(ColumnType column1, ColumnType column2)
        {

            Column left = field.columns[column1];
            Column right = field.columns[column2];

            Vector2 leftOrigin = left.getOriginPosition(this.starttime);
            Vector2 leftReceptor = left.getReceptorPosition(this.starttime);

            Vector2 rightOrigin = right.getOriginPosition(this.starttime);
            Vector2 rightReceptor = right.getReceptorPosition(this.starttime);

            left.MoveColumn(starttime, duration, rightReceptor, rightOrigin, this.easing);
            right.MoveColumn(starttime, duration, leftReceptor, leftOrigin, this.easing);

            return this.starttime + this.duration;
        }

        public double MoveColumnRelative(ColumnType column, Vector2 relativeMovement)
        {

            Column currentColumn = field.columns[column];

            Vector2 originPosition = currentColumn.getOriginPosition(starttime);
            Vector2 receptorPosition = currentColumn.getReceptorPosition(starttime);

            field.MoveOriginAbsolute(starttime, duration, easing, column, Vector2.Add(originPosition, relativeMovement));
            field.MoveReceptorAbsolute(starttime, duration, easing, column, Vector2.Add(receptorPosition, relativeMovement));

            return starttime + duration;

        }

        public void flipPlayField(float closeScale, float farScale)
        {

            Boolean isFlipped = false;

            // bg.ScaleVec(easing, starttime, starttime + duration, this.width, this.height, width, height);

            if (field.height > 240)
            {
                field.height = 20;
                isFlipped = true;
            }

            float position = 0f;

            Vector2 center = field.calculatePlayFieldCenter(starttime);

            foreach (Column column in field.columns.Values)
            {

                Receptor receptor = column.receptor;
                NoteOrigin origin = column.origin;

                Vector2 receptorPosition = receptor.getCurrentPosition(starttime);
                Vector2 currentScale = receptor.getCurrentScale(starttime);

                float closeScaleDifference = closeScale / currentScale.X;
                float farScaleDifference = farScale / currentScale.X;
                // float xDifference = fareScale / currentScale.X;

                var xOffset = (receptorPosition.X - center.X) * closeScaleDifference - (receptorPosition.X - center.X);

                var newHeight = Math.Max(field.height, 0);
                var oppositHeight = Math.Max(field.height * -1, 0);

                if (newHeight > 240)
                {
                    newHeight -= field.receptorHeightOffset;
                    oppositHeight += field.noteHeightOffset;
                }
                else
                {
                    newHeight += field.receptorHeightOffset;
                    oppositHeight -= field.noteHeightOffset;
                }

                Vector2 newPosition = new Vector2(receptorPosition.X + xOffset, 240);
                Vector2 newOpposit = new Vector2(receptorPosition.X + xOffset, 240);

                Vector2 newPositionAfter = new Vector2(receptorPosition.X, newHeight);
                Vector2 newOppositAfter = new Vector2(receptorPosition.X, oppositHeight);

                receptor.MoveReceptor(starttime - 1, newPosition, easing, duration / 2);
                origin.MoveOrigin(starttime - 1, newOpposit, easing, duration / 2);

                receptor.MoveReceptor(starttime + duration / 2, newPositionAfter, easing, duration / 2);
                origin.MoveOrigin(starttime + duration / 2, newOppositAfter, easing, duration / 2);


                if (isFlipped)
                {
                    receptor.ScaleReceptor(starttime, new Vector2(currentScale.X * closeScaleDifference, currentScale.Y * closeScaleDifference), easing, duration / 2);
                    receptor.ScaleReceptor(starttime + duration / 2, new Vector2(currentScale.X, currentScale.Y), easing, duration / 2);

                    origin.ScaleReceptor(starttime, new Vector2(currentScale.X * farScaleDifference, currentScale.Y * farScaleDifference), easing, duration / 2);
                    origin.ScaleReceptor(starttime + duration / 2, new Vector2(currentScale.X, currentScale.Y), easing, duration / 2);
                }
                else
                {
                    receptor.ScaleReceptor(starttime, new Vector2(currentScale.X * farScaleDifference, currentScale.Y * farScaleDifference), easing, duration / 2);
                    receptor.ScaleReceptor(starttime + duration / 2, new Vector2(currentScale.X, currentScale.Y), easing, duration / 2);

                    origin.ScaleReceptor(starttime, new Vector2(currentScale.X * closeScaleDifference, currentScale.Y * closeScaleDifference), easing, duration / 2);
                    origin.ScaleReceptor(starttime + duration / 2, new Vector2(currentScale.X, currentScale.Y), easing, duration / 2);
                }

                position += field.getColumnWidth();
            }


        }

        

    }
}