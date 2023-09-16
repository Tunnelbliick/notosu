using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StorybrewCommon.Storyboarding;

namespace storyboard.scriptslibrary.maniaModCharts.effects
{
    public class FadeEffect
    {
        public double starttime;
        public double endtime;
        public OsbEasing easing;
        public float value;

        public FadeEffect(double starttime, double endtime, OsbEasing easing, float value) {
            this.starttime = starttime;
            this.endtime = endtime;
            this.easing = easing;
            this.value = value;
        }
    }
}