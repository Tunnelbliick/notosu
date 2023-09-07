using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace storyboard.scriptslibrary.maniaModCharts.effects
{
    public class EffectInfo
    {
        public double starttime { get; private set; }
        public double endtime { get; private set; }
        public double duration { get; private set; }
        public EffectType effektType { get; private set; }
        public string reference { get; private set; }

        public EffectInfo(double starttime, double endtime, EffectType type, string reference)
        {
            this.starttime = starttime;
            this.endtime = endtime;
            this.duration = endtime - starttime;
            this.effektType = type;
            this.reference = reference;
        }
    }
}