using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Animations;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding.CommandValues;

namespace storyboard.scriptslibrary.maniaModCharts.utility
{
    public class Operation
    {
        public double starttime = 0;
        public double endtime = 0;
        public OperationType type;
        public OsbEasing easing;
        public CommandValue value;
        public float progress = 0;

        public Operation(double starttime, double endtime, OperationType type, CommandValue value)
        {
            this.starttime = starttime;
            this.endtime = endtime;
            this.type = type;
            this.value = value;
        }

        public Operation(double starttime, double endtime, OperationType type, OsbEasing easing, CommandValue value)
        {
            this.starttime = starttime;
            this.endtime = endtime;
            this.type = type;
            this.easing = easing;
            this.value = value;
        }

        public bool Overlaps(Operation other)
        {
            return starttime < other.endtime && endtime > other.starttime && type == other.type;
        }


        public double GetOverlapDuration(Operation other)
        {
            double overlapStart = Math.Max(this.starttime, other.starttime);
            double overlapEnd = Math.Min(this.endtime, other.endtime);
            return Math.Max(0, overlapEnd - overlapStart);
        }

        public static List<Operation> ResolveOverlaps(List<Operation> operations)
        {
            var mergedOperations = new List<Operation>();

            // First Pass: Split operations based on time
            operations.Sort((a, b) => a.starttime.CompareTo(b.starttime));

            // Split overlapping operations
            List<Operation> splitOperations = new List<Operation>();
            List<double> splitPoints = new List<double>();

            foreach (var operation in operations)
            {
                splitPoints.Add(operation.starttime);
                splitPoints.Add(operation.endtime);
            }

            splitPoints = splitPoints.Distinct().OrderBy(x => x).ToList();

            for (int i = 0; i < splitPoints.Count - 1; i++)
            {
                double start = splitPoints[i];
                double end = splitPoints[i + 1];

                // Check if there's any operation overlapping with the current time frame
                if (operations.Any(op => op.starttime < end && op.endtime > start))
                {
                    splitOperations.Add(new Operation(start, end, OperationType.MOVE, OsbEasing.None, new CommandPosition(0, 0)));
                }
            }

            foreach (var operation in operations)
            {
                for (int i = 0; i < splitOperations.Count; i++)
                {
                    var splitOp = splitOperations[i];

                    if (operation.starttime < splitOp.endtime && operation.endtime > splitOp.starttime)
                    {
                        splitOperations[i] = new Operation(splitOp.starttime, splitOp.endtime, operation.type, operation.easing, new CommandPosition(0, 0));
                    }
                }
            }


            // Second Pass: Distribute values among split operations
            foreach (var splitOperation in splitOperations)
            {
                CommandPosition finalValue = new CommandPosition(0, 0);

                foreach (var operation in operations)
                {
                    // Check if the operations don't overlap
                    if (splitOperation.starttime >= operation.endtime || splitOperation.endtime <= operation.starttime)
                        continue;

                    // Determine the overlapping duration
                    double overlapStart = Math.Max(operation.starttime, splitOperation.starttime);
                    double overlapEnd = Math.Min(operation.endtime, splitOperation.endtime);
                    double overlapDuration = overlapEnd - overlapStart;

                    // Find the fraction of the original operation's duration that's overlapping
                    double fractionOfOriginal = overlapDuration / (operation.endtime - operation.starttime);

                    // Use the fraction to scale the original operation's value
                    finalValue += (CommandPosition)operation.value * fractionOfOriginal;
                }

                // Create a new operation with the combined value and add to the mergedOperations list
                mergedOperations.Add(new Operation(splitOperation.starttime, splitOperation.endtime, splitOperation.type, splitOperation.easing, finalValue));
            }

            return mergedOperations;
        }
    }

    public enum OperationType
    {
        MOVE,
        MOVERELATIVE,
        MOVEX,
        MOVEY,
        ROTATE,
        ROTATEABSOLUTE,
        SCALE,
        SCALEVEC
    }
}