using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding.CommandValues;

namespace StorybrewScripts
{
    public enum ColumnType
    {
        one, two, three, four
    };

    public class Column
    {

        public ColumnType type;

        // note x cordinates for this column
        public double offset = 0f;

        public CommandScale scale;

        public Receptor receptor;
        public NoteOrigin origin;


        public Column(double offset, ColumnType type, String receptorSpritePath, StoryboardLayer columnLayer, CommandScale scale, double starttime)
        {
            this.offset = offset;
            this.type = type;
            this.scale = scale;

            double rotation = 0f;

            switch (type)
            {
                case ColumnType.one:
                    rotation = Math.PI / 2;
                    break;
                case ColumnType.two:
                    rotation = 0;
                    break;
                case ColumnType.three:
                    rotation = Math.PI;
                    break;
                case ColumnType.four:
                    rotation = (Math.PI * 2) - Math.PI / 2;
                    break;
            }

            this.receptor = new Receptor(receptorSpritePath, rotation, columnLayer, scale, starttime);
            this.origin = new NoteOrigin(receptorSpritePath, rotation, columnLayer, scale, starttime);

        }

        public double MoveColumn(int starttime, int duration, Vector2 newColumnPosition, Vector2 newOriginPosition, OsbEasing easing)
        {

            this.receptor.MoveReceptor(starttime, newColumnPosition, easing, duration);
            this.origin.MoveReceptor(starttime, newOriginPosition, easing, duration);

            return starttime + duration;
        }

        public Vector2 getOriginPosition(double starttime)
        {
            return this.origin.getCurrentPosition(starttime);
        }

        public Vector2 getReceptorPosition(double starttime)
        {
            return this.receptor.getCurrentPosition(starttime);
        }

    }
}