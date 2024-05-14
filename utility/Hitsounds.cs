using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{
    public static class HitSound
    {

        public static double worstTiming(Playfield field)
        {
            return 151 - (3 * field.od);
        }

        public static void AddHitSound(Playfield field, Column column, List<double> keysInRange, Dictionary<double, Note> notes)
        {

            var marv = 16;
            var great = 64 - (3 * field.od);
            var good = 97 - (3 * field.od);
            var ok = 127 - (3 * field.od);
            var bad = 151 - (3 * field.od);

            OsbSprite light = column.receptor.light;
            OsbSprite hit = column.receptor.hit;

            string trigger = "HitSound0";

            switch (column.type)
            {
                case ColumnType.one:
                    trigger = "HitSoundNormalNormal";
                    break;
                case ColumnType.two:
                    trigger = "HitSoundNormalSoft";
                    break;
                case ColumnType.three:
                    trigger = "HitSoundSoftNormal";
                    break;
                case ColumnType.four:
                    trigger = "HitSoundSoftSoft";
                    break;
            }

            var fadeOut = 151;

            foreach (var key in keysInRange)
            {
                Note note = notes[key];

                // Trigger for 300+ (±16ms)
                light.StartTriggerGroup(trigger, note.endtime - marv, note.endtime + marv);
                light.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                light.Color(0, new Color4(104, 186, 207, 0));
                light.EndGroup();

                // Trigger for 300 (±46ms, excludes 300+ range)
                light.StartTriggerGroup(trigger, note.endtime - great, note.endtime - marv + 1);
                light.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                light.Color(0, new Color4(223, 181, 96, 0));
                light.EndGroup();

                light.StartTriggerGroup(trigger, note.endtime + marv + 1, note.endtime + great);
                light.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                light.Color(0, new Color4(223, 181, 96, 0));
                light.EndGroup();

                // Trigger for 200 (±79ms, excludes 300 range)
                light.StartTriggerGroup(trigger, note.endtime - good, note.endtime - great + 1);
                light.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                light.Color(0, new Color4(95, 204, 95, 0));
                light.EndGroup();

                light.StartTriggerGroup(trigger, note.endtime + great + 1, note.endtime + good);
                light.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                light.Color(0, new Color4(95, 204, 95, 0));
                light.EndGroup();

                // Trigger for 100 (±109ms, excludes 200 range)
                light.StartTriggerGroup(trigger, note.endtime - ok, note.endtime - good + 1);
                light.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                light.Color(0, new Color4(206, 128, 224, 0));
                light.EndGroup();

                light.StartTriggerGroup(trigger, note.endtime + good + 1, note.endtime + ok);
                light.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                light.Color(0, new Color4(206, 128, 224, 0));
                light.EndGroup();

                // Trigger for 50 (±133ms, excludes 100 range)
                light.StartTriggerGroup(trigger, note.endtime - bad, note.endtime - ok + 1);
                light.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                light.Color(0, new Color4(198, 86, 37, 0));
                light.EndGroup();

                light.StartTriggerGroup(trigger, note.endtime + ok + 1, note.endtime + bad);
                light.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                light.Color(0, new Color4(198, 86, 37, 0));
                light.EndGroup();




                // Trigger for 300+ (±16ms)
                hit.StartTriggerGroup(trigger, note.endtime - marv, note.endtime + marv);
                hit.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                hit.EndGroup();

                // Trigger for 300 (±46ms, excludes 300+ range)
                hit.StartTriggerGroup(trigger, note.endtime - great, note.endtime - marv + 1);
                hit.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                hit.EndGroup();

                hit.StartTriggerGroup(trigger, note.endtime + marv + 1, note.endtime + great);
                hit.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                hit.EndGroup();

                // Trigger for 200 (±79ms, excludes 300 range)
                hit.StartTriggerGroup(trigger, note.endtime - good, note.endtime - great + 1);
                hit.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                hit.EndGroup();

                hit.StartTriggerGroup(trigger, note.endtime + great + 1, note.endtime + good);
                hit.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                hit.EndGroup();

                // Trigger for 100 (±109ms, excludes 200 range)
                hit.StartTriggerGroup(trigger, note.endtime - ok, note.endtime - good + 1);
                hit.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                hit.EndGroup();

                hit.StartTriggerGroup(trigger, note.endtime + good + 1, note.endtime + ok);
                hit.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                hit.EndGroup();

                // Trigger for 50 (±133ms, excludes 100 range)
                hit.StartTriggerGroup(trigger, note.endtime - bad, note.endtime - ok + 1);
                hit.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                hit.EndGroup();

                hit.StartTriggerGroup(trigger, note.endtime + ok + 1, note.endtime + bad);
                hit.Fade(OsbEasing.InExpo, 0, fadeOut, 1, 0);
                hit.EndGroup();
            }

        }


    }
}