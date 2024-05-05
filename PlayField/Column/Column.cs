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
        all = 0,
        one = 1,
        two = 2,
        three = 3,
        four = 4
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


        public Column(double offset, ColumnType type, String receptorSpritePath, StoryboardLayer columnLayer, CommandScale scale, double starttime, double delta)
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

            receptor = new Receptor(receptorSpritePath, rotation, columnLayer, scale, starttime, this.type, delta);
            origin = new NoteOrigin(receptorSpritePath, rotation, columnLayer, scale, starttime, delta);

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

        public void MoveColumn(OsbEasing easing, double starttime, double endtime, Vector2 from, Vector2 to)
        {
            receptor.MoveReceptorAbsolute(easing, starttime, endtime, from, to);
            origin.MoveOriginAbsolute(easing, starttime, endtime, from, to);
        }

        public void MoveColumnRelative(OsbEasing easing, double starttime, double endtime, Vector2 offset)
        {
            receptor.MoveReceptorRelative(easing, starttime, endtime, offset);
            origin.MoveOriginRelative(easing, starttime, endtime, offset);
        }

        public void MoveColumnRelativeX(OsbEasing easing, double starttime, double endtime, float value)
        {
            receptor.MoveReceptorRelativeX(easing, starttime, endtime, value);
            origin.MoveOriginRelativeX(easing, starttime, endtime, value);
        }

        public void MoveColumnRelativeY(OsbEasing easing, double starttime, double endtime, float value)
        {
            receptor.MoveReceptorRelativeY(easing, starttime, endtime, value);
            origin.MoveOriginRelativeY(easing, starttime, endtime, value);
        }

        public void MoveReceptorAbsolute(double starttime, Vector2 newReceptorPosition)
        {
            receptor.MoveReceptorAbsolute(starttime, newReceptorPosition);
        }

        public void MoveReceptorAbsolute(OsbEasing ease, double starttime, double endtime, Vector2 startPos, Vector2 endPos)
        {
            receptor.MoveReceptorAbsolute(ease, starttime, endtime, startPos, endPos);
        }

        public void MoveReceptorRelative(OsbEasing easing, double starttime, double endtime, Vector2 offset)
        {
            receptor.MoveReceptorRelative(easing, starttime, endtime, offset);
        }

        public void RotateReceptorRelative(OsbEasing easing, double starttime, double endtime, double rotation)
        {
            receptor.RotateReceptor(easing, starttime, endtime, rotation);
        }

        public void RotateReceptor(OsbEasing easing, double starttime, double endtime, double rotation)
        {
            receptor.RotateReceptorAbsolute(easing, starttime, endtime, rotation);
        }

        public void MoveOriginAbsoluite(double starttime, Vector2 newOriginPosition)
        {

            origin.MoveOriginAbsolute(starttime, newOriginPosition);
        }

        public void MoveOriginAbsoluite(OsbEasing ease, double starttime, double endtime, Vector2 startPos, Vector2 endPos)
        {

            origin.MoveOriginAbsolute(ease, starttime, endtime, startPos, endPos);
        }

        public void MoveOriginRelative(OsbEasing ease, double starttime, double endtime, Vector2 offset)
        {

            origin.MoveOriginRelative(ease, starttime, endtime, offset);
        }

        public Vector2 OriginPositionAt(double starttime)
        {
            return origin.PositionAt(starttime);
        }

        public float OriginRotationAt(double starttime)
        {
            return origin.RotationAt(starttime);
        }

        public Vector2 OriginScaleAt(double starttime)
        {
            return origin.ScaleAt(starttime);
        }

        public Vector2 ReceptorPositionAt(double starttime)
        {
            return receptor.PositionAt(starttime);
        }

        public double ReceptorRotationAt(double starttime)
        {
            return receptor.RotationAt(starttime);
        }

        public Vector2 ReceptorScaleAt(double starttime)
        {
            return receptor.ScaleAt(starttime);
        }


    }
}