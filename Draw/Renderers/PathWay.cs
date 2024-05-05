using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using storyboard.scriptslibrary.maniaModCharts.effects;
using StorybrewCommon.Animations;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{
    [Obsolete("")]
    public static class PathWay
    {

        public static string DrawPath(DrawInstance instance, double starttime, double endtime, StoryboardLayer layer, string spritePath, PathType type, int precision, int updatesPerSecond = 3)
        {
            String debug = "";

            Dictionary<ColumnType, List<OsbSprite>> pathSprites = instance.pathWaySprites;

            var movementPerSpriteByColumn = new Dictionary<ColumnType, List<KeyframedValue<Vector2>>>();
            var scalePerSpriteByColumn = new Dictionary<ColumnType, List<KeyframedValue<Vector2>>>();
            var rotationPerSpriteByColumn = new Dictionary<ColumnType, List<KeyframedValue<double>>>();

            double currentTime = starttime;

            foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
            {

                if (currentColumn == ColumnType.all)
                    continue;

                List<Anchor> notePath = instance.notePathByColumn[currentColumn];
                List<Vector2> points = instance.GetPathAnchorVectors(notePath, starttime);
                List<OsbSprite> columnSprites = new List<OsbSprite>();

                var movementPerSprite = new List<KeyframedValue<Vector2>>();
                var scalePerSprite = new List<KeyframedValue<Vector2>>();
                var rotationPerSprite = new List<KeyframedValue<double>>();

                switch (type)
                {
                    case PathType.bezier:
                        float progress = 0;
                        float increment = 1f / precision;

                        while (progress < 1f)
                        {

                            var movement = new KeyframedValue<Vector2>(null);
                            var scale = new KeyframedValue<Vector2>(null);
                            var rotation = new KeyframedValue<double>(null);

                            Vector2 firstPoint = BezierCurve.CalculatePoint(points, progress);
                            Vector2 secondPoint = BezierCurve.CalculatePoint(points, progress + increment);

                            float dx = firstPoint.X - secondPoint.X;
                            float dy = firstPoint.Y - secondPoint.Y;
                            float distance = (float)Math.Sqrt(dx * dx + dy * dy) + 1f;

                            Vector2 delta = firstPoint - secondPoint;
                            double theta = Math.Atan2(delta.X, delta.Y);

                            OsbSprite sprite = layer.CreateSprite(spritePath, OsbOrigin.BottomCentre, firstPoint);
                            sprite.Fade(endtime, 0);
                            columnSprites.Add(sprite);

                            movement.Add(currentTime, firstPoint);
                            scale.Add(currentTime, new Vector2(4, distance));
                            rotation.Add(currentTime, -theta);

                            movementPerSprite.Add(movement);
                            scalePerSprite.Add(scale);
                            rotationPerSprite.Add(rotation);

                            progress += increment;
                        }
                        break;

                    case PathType.line:
                        for (int n = 0; n < notePath.Count - 1; n++)
                        {

                            var movement = new KeyframedValue<Vector2>(null);
                            var scale = new KeyframedValue<Vector2>(null);
                            var rotation = new KeyframedValue<double>(null);

                            Vector2 firstPoint = notePath[n].position;
                            Vector2 secondPoint = notePath[n + 1].position;

                            float dx = firstPoint.X - secondPoint.X;
                            float dy = firstPoint.Y - secondPoint.Y;
                            float distance = (float)Math.Sqrt(dx * dx + dy * dy) + 1f;

                            Vector2 delta = firstPoint - secondPoint;
                            double theta = Math.Atan2(delta.X, delta.Y);

                            OsbSprite sprite = layer.CreateSprite(spritePath, OsbOrigin.BottomCentre, firstPoint);
                            sprite.Fade(endtime, 0);

                            movement.Add(currentTime, firstPoint);
                            scale.Add(currentTime, new Vector2(4, distance));
                            rotation.Add(currentTime, -theta);

                            columnSprites.Add(sprite);

                            movementPerSprite.Add(movement);
                            scalePerSprite.Add(scale);
                            rotationPerSprite.Add(rotation);

                        }
                        break;
                }

                pathSprites.Add(currentColumn, columnSprites);

                movementPerSpriteByColumn.Add(currentColumn, movementPerSprite);
                scalePerSpriteByColumn.Add(currentColumn, scalePerSprite);
                rotationPerSpriteByColumn.Add(currentColumn, rotationPerSprite);
            }

            instance.pathWaySprites = pathSprites;

            while (currentTime <= endtime)
            {
                double localIterationRate = 1000 / updatesPerSecond;
                currentTime += localIterationRate;

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    List<Anchor> notePath = instance.notePathByColumn[currentColumn];
                    List<Vector2> points = instance.GetPathAnchorVectors(notePath, currentTime);

                    var movementPerSprite = movementPerSpriteByColumn[currentColumn];
                    var scalePerSprite = scalePerSpriteByColumn[currentColumn];
                    var rotationPerSprite = rotationPerSpriteByColumn[currentColumn];

                    List<double> currentTheta = new List<double>(); ;


                    int i = 0;

                    switch (type)
                    {
                        case PathType.bezier:
                            float progress = 0;
                            float increment = 1f / precision;

                            while (progress < 1f)
                            {
                                Vector2 firstPoint = BezierCurve.CalculatePoint(points, progress);
                                Vector2 secondPoint = BezierCurve.CalculatePoint(points, progress + increment);

                                float dx = firstPoint.X - secondPoint.X;
                                float dy = firstPoint.Y - secondPoint.Y;
                                float distance = (float)Math.Sqrt(dx * dx + dy * dy) + 1f;

                                Vector2 delta = firstPoint - secondPoint;
                                double theta = Math.Atan2(delta.X, delta.Y);

                                double priorTheta = 0f;

                                if (currentTheta.Count - 1 > i)
                                {
                                    priorTheta = currentTheta[i];
                                }

                                if (priorTheta > 0.02f && Math.Abs(Math.Abs(priorTheta) - Math.Abs(theta)) > Math.PI / 4)
                                {
                                    theta = priorTheta;
                                }

                                if (currentTheta.Count - 1 > i)
                                {
                                    currentTheta.Add(-theta);
                                }

                                rotationPerSprite[i].Add(currentTime, -theta);
                                movementPerSprite[i].Add(currentTime + localIterationRate, firstPoint);
                                scalePerSprite[i].Add(currentTime + localIterationRate, new Vector2(4, distance));

                                progress += increment;
                                i++;
                            }
                            break;

                        case PathType.line:
                            for (int n = 0; n < notePath.Count - 1; n++)
                            {
                                Vector2 firstPoint = notePath[n].position;
                                Vector2 secondPoint = notePath[n + 1].position;

                                float dx = firstPoint.X - secondPoint.X;
                                float dy = firstPoint.Y - secondPoint.Y;
                                float distance = (float)Math.Sqrt(dx * dx + dy * dy) + 1f;

                                Vector2 delta = firstPoint - secondPoint;
                                double theta = Math.Atan2(delta.X, delta.Y);

                                rotationPerSprite[i].Add(currentTime, -theta);
                                movementPerSprite[i].Add(currentTime + localIterationRate, firstPoint);
                                scalePerSprite[i].Add(currentTime + localIterationRate, new Vector2(4, distance));

                                i++;
                            }
                            break;
                    }
                }
            }

            foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
            {

                if (currentColumn == ColumnType.all)
                    continue;

                var movementPerSprite = movementPerSpriteByColumn[currentColumn];
                var scalePerSprite = scalePerSpriteByColumn[currentColumn];
                var rotationPerSprite = rotationPerSpriteByColumn[currentColumn];

                List<OsbSprite> sprites = pathSprites[currentColumn];

                for (int i = 0; i < movementPerSprite.Count; i++)
                {

                    OsbSprite sprite = sprites[i];

                    var movement = movementPerSprite[i];
                    var scale = scalePerSprite[i];
                    var rotation = rotationPerSprite[i];

                    movement.Simplify(0.75f);
                    scale.Simplify(0.25f);
                    //rotation.Simplify1dKeyframes(0.05f, v => (float)v); // this shit dont look good / dont work properly should instead do some value clamping to avoid instant 180° from pathway since that will result in shit.

                    movement.ForEachPair((start, end) => sprite.Move(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));
                    scale.ForEachPair((start, end) => sprite.ScaleVec(OsbEasing.None, start.Time, end.Time, start.Value, end.Value));
                    rotation.ForEachPair((start, end) => sprite.Rotate(end.Time, end.Value));

                }


            }

            return debug;

        }

    }
}