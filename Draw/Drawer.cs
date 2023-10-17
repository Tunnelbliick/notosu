using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace storyboard.scriptslibrary.maniaModCharts.Draw
{
    public class Drawer
    {

        protected float NoteMovementPrecision = 2f;
        protected float NoteScalePrecision = .01f;
        protected float NoteRotationPrecision = 1f;
        protected float NoteFadePrcision = 0f;

        protected float ReceptorMovementPrecision = 15f;
        protected float ReceptorScalePrecision = .2f;
        protected float ReceptorRotationPrecision = 1f;

        protected float HoldMovementPrecision = 2f;
        protected float HoldScalePrecision = 0f;
        protected float HoldRotationPrecision = 1f;

        protected float HoldRoationDeadzone = 0f;

        public void setReceptorPrecision(float movement, float scale, float rotation)
        {
            this.ReceptorMovementPrecision = movement;
            this.ReceptorScalePrecision = scale;
            this.ReceptorRotationPrecision = rotation;
        }

        public void setReceptorMovementPrecision(float value)
        {
            this.ReceptorMovementPrecision = value;
        }

        public void setReceptorScalePrecision(float value)
        {
            this.ReceptorScalePrecision = value;
        }

        public void setReceptorRotationPrecision(float value)
        {
            this.ReceptorRotationPrecision = value;
        }

        public void setNotePrecision(float movement, float scale, float rotation, float fade)
        {
            this.NoteMovementPrecision = movement;
            this.NoteScalePrecision = scale;
            this.NoteRotationPrecision = rotation;
            this.NoteFadePrcision = fade;
        }

        public void setNoteMovementPrecision(float value)
        {
            this.NoteMovementPrecision = value;
        }

        public void setNoteScalePrecision(float value)
        {
            this.NoteScalePrecision = value;
        }

        public void setNoteRotationPrecision(float value)
        {
            this.NoteScalePrecision = value;
        }

        public void setNoteFadePrecision(float value)
        {
            this.NoteFadePrcision = value;
        }

        public void setHoldPrecision(float movement, float scale, float rotation)
        {
            this.HoldMovementPrecision = movement;
            this.HoldScalePrecision = scale;
            this.HoldRotationPrecision = rotation;
        }

        public void setHoldMovementPrecision(float value)
        {
            this.HoldMovementPrecision = value;
        }

        public void setHoldScalePrecision(float value)
        {
            this.HoldScalePrecision = value;
        }

        public void setHoldRotationPrecision(float value)
        {
            this.HoldRotationPrecision = value;
        }

        public void setHoldRotationDeadZone(float value)
        {
            this.HoldRoationDeadzone = value;
        }
    }
}