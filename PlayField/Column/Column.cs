using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using storyboard.scriptslibrary.maniaModCharts.utility;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding.CommandValues;

namespace StorybrewScripts
{
    public enum ColumnType
    {
        one, two, three, four, all
    };

    public class Column
    {

        public ColumnType type;

        // note x cordinates for this column
        public double offset = 0f;

        public CommandScale scale;

        public Receptor receptor;
        public NoteOrigin origin;

        public double bpmOffset;
        public double bpm;


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

            this.receptor = new Receptor(receptorSpritePath, rotation, columnLayer, scale, starttime, this.type);
            this.origin = new NoteOrigin(receptorSpritePath, rotation, columnLayer, scale, starttime);

        }

        // This methods sets the bpm for the receptor glint on full beats
        public void setBPM(double bpm, double bpmOffset)
        {

            this.bpm = bpm;
            this.bpmOffset = bpmOffset;

            receptor.bpm = bpm;
            receptor.bpmOffset = bpmOffset;

            origin.bpm = bpm;
            origin.bpmOffset = bpmOffset;

        }

        public double MoveColumn(double starttime, double duration, Vector2 newColumnPosition, Vector2 newOriginPosition, OsbEasing easing)
        {
            this.receptor.MoveReceptor(starttime, newColumnPosition, easing, duration);
            this.origin.MoveOrigin(starttime, newOriginPosition, easing, duration);

            return starttime + duration;
        }

        public double MoveColumnRelative(double starttime, double duration, Vector2 offset, OsbEasing easing)
        {
            this.receptor.MoveReceptorRelative(starttime, offset, easing, duration);
            this.origin.MoveOriginRelative(starttime, offset, easing, duration);

            return starttime + duration;
        }

        public double MoveColumnRelativeX(double starttime, double duration, double value, OsbEasing easing)
        {
            this.receptor.MoveReceptorRelativeX(starttime, value, easing, duration);
            this.origin.MoveOriginRelativeX(starttime, value, easing, duration);

            return starttime + duration;
        }

        public double MoveColumnRelativeY(double starttime, double duration, double value, OsbEasing easing)
        {
            this.receptor.MoveReceptorRelativeY(starttime, value, easing, duration);
            this.origin.MoveOriginRelativeY(starttime, value, easing, duration);

            return starttime + duration;
        }

        public double MoveReceptor(double starttime, double duration, Vector2 newReceptorPosition, OsbEasing easing)
        {

            this.receptor.MoveReceptor(starttime, newReceptorPosition, easing, duration);

            return starttime + duration;
        }

        public double MoveReceptorRelative(double starttime, double duration, Vector2 offset, OsbEasing easing)
        {

            this.receptor.MoveReceptorRelative(starttime, offset, easing, duration);

            return starttime + duration;
        }


        public double RotateReceptorRelative(double starttime, double duration, OsbEasing easing, double rotation)
        {

            this.receptor.RotateReceptor(starttime, duration, easing, rotation);

            return starttime + duration;
        }

        public double RotateReceptor(double starttime, double duration, OsbEasing easing, double rotation)
        {

            this.receptor.RotateReceptorAbsolute(starttime, duration, easing, rotation);

            return starttime + duration;
        }

        public double MoveOrigin(double starttime, double duration, Vector2 newOriginPosition, OsbEasing easing)
        {

            this.origin.MoveOrigin(starttime, newOriginPosition, easing, duration);

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

        public Vector2 getReceptorPositionForNotes(double starttime)
        {
            return this.receptor.getCurrentPositionForNotes(starttime);
        }

        public double getReceptorRotation(double starttime)
        {
            return this.receptor.getCurrentRotaion(starttime);
        }

        public List<Operation> executeKeyFrames()
        {
            return receptor.executeOperations();
        }

    }
}