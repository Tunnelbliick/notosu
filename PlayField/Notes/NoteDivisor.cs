using System;
using System.Collections.Generic;
using System.Linq;

namespace StorybrewScripts {
    public enum NoteDivisor {
        wholeTick = 1,
        halfTick = 2,
        tripletTick = 3,
        quarterTick = 4,
        twelfthTick = 12,
        sixteenthTick = 16,
        twentyFourthTick = 24,
        thirtySecondTick = 32
    } 

    public static class NoteDivisorExtension {
        public static int getNoteType(this NoteDivisor divisor) {
            switch (divisor) {
                case NoteDivisor.wholeTick:
                    return 1;
                case NoteDivisor.halfTick:
                    return 2;
                case NoteDivisor.tripletTick:
                    return 3;
                case NoteDivisor.quarterTick:
                    return 4;
                case NoteDivisor.twelfthTick:
                    return 12;
                case NoteDivisor.sixteenthTick:
                    return 16;
                case NoteDivisor.twentyFourthTick:
                    return 12;
                case NoteDivisor.thirtySecondTick:
                    return 16;
                default: 
                    return 1;
            }
        }
    } 
}