using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Storyboarding.CommandValues;

namespace storyboard.scriptslibrary.maniaModCharts.utility
{
    public class Operation
    {
        public double starttime = 0;
        public double endtime = 0;
        public OperationType type;
        public CommandValue value;
        public float progress = 0;

        public Operation(double starttime, double endtime, OperationType type, CommandValue value)
        {
            this.starttime = starttime;
            this.endtime = endtime;
            this.type = type;
            this.value = value;
        }
    }

    public enum OperationType
    {
        MOVE,
        MOVEX,
        MOVEY,
        ROTATE,
        SCALE,
        SCALEVEC
    }
}