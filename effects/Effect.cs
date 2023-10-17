using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{
    public class Effect
    {
        public double starttime;
        public double endtime;
        public double duration;
        public OsbEasing easing;
        public int updatesPerSecond;

        public Effect(double starttime, double endtime, OsbEasing easing, int updatesPerSecond) {
            this.starttime = starttime;
            this.endtime = endtime;
            this.duration = endtime - starttime;
            this.easing = easing;
            this.updatesPerSecond = updatesPerSecond;
        }
    }
}