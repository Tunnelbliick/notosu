using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{
    public class DrawInstance
    {

        public double starttime = 0;
        public double easetime = 0;
        public OsbEasing easing;
        public Playfield playfieldInstance;
        public int snapShots = 25;
        public bool rotateToFaceReceptor = true;

        public DrawInstance InitializeDrawInstance(Playfield playfieldInstance, double starttime, double easetime, int snapShots, OsbEasing easing, bool rotateToFaceReceptor)
        {

            this.starttime = starttime;
            this.easetime = easetime;
            this.easing = easing;
            this.playfieldInstance = playfieldInstance;
            this.snapShots = snapShots;
            this.rotateToFaceReceptor = rotateToFaceReceptor;

            return this;

        }

        public DrawInstance(Playfield playfieldInstance, double starttime, double easetime, int snapShots, OsbEasing easing, bool rotateToFaceReceptor) {

            this.starttime = starttime;
            this.easetime = easetime;
            this.easing = easing;
            this.playfieldInstance = playfieldInstance;
            this.snapShots = snapShots;
            this.rotateToFaceReceptor = rotateToFaceReceptor;

        }

        public double drawNotesDefault(double duration)
        {

            double endtime = starttime + duration;

            foreach (Column column in playfieldInstance.columns.Values)
            {
                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];

                // Get only the keys (hittimes) that fall within the specified range considering easetime
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime <= endtime).ToList();

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];
                    double noteTime = note.starttime;
                    double fadeInTime = noteTime - easetime;

                    note.Render(fadeInTime, easetime, easing);
                    note.Move(fadeInTime, easetime, easing, column.origin.getCurrentPosition(note.starttime), column.receptor.getCurrentPositionForNotes(note.starttime));
                    note.Scale(fadeInTime, easetime, easing, column.origin.getCurrentScale(note.starttime), column.receptor.getCurrentScale(note.starttime));
                }
            }

            return endtime;
        }

        public double drawNotesByEndPosition(double duration)
        {

            double endtime = starttime + duration;

            foreach (Column column in playfieldInstance.columns.Values)
            {
                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];

                // Get only the keys (hittimes) that fall within the specified range considering easetime
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime <= endtime).ToList();

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];
                    double noteTime = note.starttime;
                    double fadeInTime = noteTime - easetime;

                    note.Render(fadeInTime, easetime, easing);
                    note.Move(fadeInTime, easetime, easing, new Vector2(column.receptor.getCurrentPositionForNotes(note.starttime).X, column.origin.getCurrentPosition(fadeInTime).Y), column.receptor.getCurrentPositionForNotes(note.starttime));
                    note.Scale(fadeInTime, easetime, easing, column.origin.getCurrentScale(fadeInTime), column.receptor.getCurrentScale(note.starttime));
                }
            }

            return endtime;
        }

        public double drawNotesBySnapshotPosition(double duration)
        {

            double endtime = starttime + duration;

            double snapLength = easetime / snapShots;

            foreach (Column column in playfieldInstance.columns.Values)
            {
                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];

                // Get only the keys (hittimes) that fall within the specified range considering easetime
                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime <= endtime).ToList();

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];
                    double noteTime = note.starttime;
                    double fadeInTime = noteTime - easetime;

                    double travelDistance = Math.Abs(column.origin.getCurrentPosition(fadeInTime).Y - column.receptor.getCurrentPosition(fadeInTime).Y);
                    double distancePerSnap = travelDistance / snapShots;
                    bool moveUpwards = column.origin.getCurrentPosition(fadeInTime).Y > column.receptor.getCurrentPosition(fadeInTime).Y;

                    Vector2 originPosition = column.origin.getCurrentPosition(fadeInTime);
                    Vector2 receptorPosition = column.receptor.getCurrentPositionForNotes(fadeInTime);

                    double currentTime = fadeInTime;

                    note.Render(fadeInTime, easetime, easing);

                    for (int i = 0; i <= snapShots; i++)
                    {

                        double snapDuration = snapLength * i;

                        Vector2 currentPosition = column.receptor.getCurrentPosition(currentTime + snapDuration);
                        double newYPosition = moveUpwards ? (originPosition.Y - i * distancePerSnap) : (originPosition.Y + i * distancePerSnap);

                        Vector2 newPosition = new Vector2(currentPosition.X, (float)newYPosition);

                        note.Move(currentTime, snapDuration, easing, originPosition, newPosition);

                        currentTime += snapDuration;
                        originPosition = newPosition;
                    }
                    note.Scale(fadeInTime, easetime, easing, column.receptor.getCurrentScale(note.starttime), column.receptor.getCurrentScale(note.starttime));
                }
            }

            return endtime;
        }

        public double drawNotesByOriginToReceptor(double duration)
        {
            double endtime = starttime + duration;

            // This will guarantee that the total time of all snaps is exactly easetime
            double snapLength = easetime / snapShots;


            foreach (Column column in playfieldInstance.columns.Values)
            {
                Dictionary<double, Note> notes = playfieldInstance.columnNotes[column.type];

                var keysInRange = notes.Keys.Where(hittime => hittime >= starttime && hittime <= endtime).ToList();

                foreach (var key in keysInRange)
                {
                    Note note = notes[key];
                    double noteTime = note.starttime;
                    double fadeInTime = noteTime - easetime;
                    double renderTime = Math.Max(fadeInTime, starttime);
                    double noteOnScreanDuration = easetime - (renderTime - fadeInTime);

                    double currentTime = fadeInTime;
                    float progress = 0;

                    Vector2 currentPosition = column.origin.getCurrentPosition(currentTime);


                    // TODO fix render in position beeing center of screen then initial receptor position
                    // IDK why this happens but it does happen prob some weird order issue.
                    note.invisible(fadeInTime - 1);
                    note.Move(currentTime - 1, 0, easing, currentPosition, currentPosition);
                    note.Render(renderTime, noteOnScreanDuration, easing);
                    double startRotation = note.getRotation(currentTime);

                    for (int i = 0; i <= snapShots; i++)
                    {
                        double timeLeft = easetime - snapLength * i; ;

                        Vector2 originPosition = column.origin.getCurrentPosition(currentTime);
                        Vector2 receptorPosition = column.receptor.getCurrentPositionForNotes(currentTime);
                        Vector2 newPosition = Vector2.Lerp(receptorPosition, originPosition, progress);
                        Vector2 originScale = column.origin.getCurrentScale(currentTime);
                        Vector2 receptorScale = column.receptor.getCurrentScale(currentTime);
                        Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, progress);
                        // Calculate the progress based on the remaining time
                        progress = (float)timeLeft / (float)easetime;

                        if (progress == 1)
                            continue;

                        double theta = 0;

                        if (progress > 0.15 && rotateToFaceReceptor)
                        {
                            Vector2 delta = receptorPosition - currentPosition;

                            // Check relative vertical positions
                            if (currentPosition.Y > receptorPosition.Y)
                            {
                                // If the receptor is above the origin, reverse the direction
                                delta = -delta;
                            }

                            theta = Math.Atan2(delta.X, delta.Y);
                        }

                        note.Move(currentTime, snapLength, easing, currentPosition, newPosition);
                        note.Scale(currentTime, snapLength, easing, column.origin.getCurrentScale(currentTime), scaleProgress);
                        note.AbsoluteRotate(currentTime, snapLength, easing, startRotation - theta);

                        // Weird spinn in issues?
                        //if (note.getRotation(currentTime) != column.receptor.getCurrentRotaion(currentTime))
                        //note.AbsoluteRotate(currentTime, snapLength, easing, column.receptor.getCurrentRotaion(currentTime));

                        currentTime += snapLength;
                        currentPosition = newPosition;

                    }

                    if (note.isSlider)
                    {

                        foreach (Vector2WithTimestamp parts in note.sliderPositions)
                        {
                            double sliderStartime = parts.Timestamp;
                            OsbSprite sprite = parts.Sprite;
                            double sliderCurrentTime = sliderStartime - easetime;
                            Vector2 currentSliderPositon = column.origin.getCurrentPosition(sliderCurrentTime); ;
                            float sliderProgress = 0;

                            sprite.Move(sliderCurrentTime - 1, currentSliderPositon);
                            double sliderRotation = sprite.RotationAt(sliderCurrentTime);

                            for (int i = 0; i <= snapShots; i++)
                            {

                                double snapDuration = snapLength * i;
                                double timeLeft = easetime - snapLength * i; ;

                                Vector2 originPosition = column.origin.getCurrentPosition(sliderCurrentTime);
                                Vector2 receptorPosition = column.receptor.getCurrentPositionForNotes(sliderCurrentTime);
                                Vector2 newPosition = Vector2.Lerp(receptorPosition, originPosition, sliderProgress);
                                Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime);

                                //Vector2 originScale = column.origin.getCurrentScale(sliderCurrentTime);
                                //Vector2 receptorScale = column.receptor.getCurrentScale(sliderCurrentTime);
                                //Vector2 scaleProgress = Vector2.Lerp(receptorScale, originScale, sliderProgress);

                                // Calculate the progress based on the remaining time
                                sliderProgress = (float)timeLeft / (float)easetime;

                                double theta = 0;

                                Vector2 delta = receptorPosition - currentSliderPositon;

                                // Check relative vertical positions
                                if (currentSliderPositon.Y > receptorPosition.Y)
                                {
                                    // If the receptor is above the origin, reverse the direction
                                    delta = -delta;
                                }

                                theta = Math.Atan2(delta.X, delta.Y);

                                if (i == snapShots)
                                {
                                    note.Move(sliderCurrentTime, snapLength, easing, currentPosition, column.receptor.getCurrentPosition(sliderCurrentTime));
                                    note.Scale(sliderCurrentTime, snapLength, easing, receptorScale, receptorScale);
                                    currentPosition = column.receptor.getCurrentPosition(sliderCurrentTime);
                                }

                                // Weird spinn in issues?
                                //if (note.getRotation(currentTime) != column.receptor.getCurrentRotaion(currentTime))
                                //note.AbsoluteRotate(currentTime, snapLength, easing, column.receptor.getCurrentRotaion(currentTime));

                                if (sliderProgress == 1)
                                    continue;

                                sprite.Move(easing, sliderCurrentTime, sliderCurrentTime + snapLength, currentSliderPositon, newPosition);
                                sprite.ScaleVec(sliderCurrentTime, column.origin.getCurrentScale(sliderCurrentTime).X + 0.2f, 0.1525f);
                                sprite.Rotate(easing, sliderCurrentTime, sliderCurrentTime + snapLength, sprite.RotationAt(sliderCurrentTime), sliderRotation - theta);

                                sliderCurrentTime += snapLength;
                                currentSliderPositon = newPosition;

                            }

                        }
                    }
                }
            }

            return endtime;
        }

    }
}