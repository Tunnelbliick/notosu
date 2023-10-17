using System;
using System.Drawing;
using System.IO;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Collections.Generic;
using AForge;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTK;

namespace StorybrewScripts
{
    public static class ImageManipulator
    {

        public static Bitmap transformImage(Bitmap input, Vector2 topLeft, Vector2 topRight, Vector2 bottomRight, Vector2 bottomLeft)
        {
            Bitmap paddedImage = new Bitmap(input.Width * 2 + 100, input.Height * 2 + 100);
            using (Graphics g = Graphics.FromImage(paddedImage))
            {
                // Explicitly set graphics properties
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(input, new Rectangle(input.Width / 2 + 50, input.Height / 2 + 50, input.Width, input.Height));
            }

            IntPoint topLeftInt = new IntPoint((int)topLeft.X, (int)topLeft.Y);
            IntPoint topRightInt = new IntPoint((int)topRight.X, (int)topRight.Y);
            IntPoint bottomRightInt = new IntPoint((int)bottomRight.X, (int)bottomRight.Y);
            IntPoint bottomLeftInt = new IntPoint((int)bottomLeft.X, (int)bottomLeft.Y);

            List<IntPoint> sourcePoints = new List<IntPoint>
            {
                new IntPoint(topLeftInt.X, topLeftInt.Y),
                new IntPoint(input.Width + topRightInt.X, topRightInt.Y),
                new IntPoint(input.Width + bottomRightInt.X, input.Height + bottomRightInt.Y),
                new IntPoint(bottomLeftInt.X, input.Height + bottomLeftInt.Y)
            };

            // Calculate scale factors based on the dimensions of the padded image and the original image
            double xScaleFactor = (double)paddedImage.Width / input.Width;
            double yScaleFactor = (double)paddedImage.Height / input.Height;

            List<IntPoint> destinationPoints = new List<IntPoint>
            {
                new IntPoint((int)(topLeftInt.X * xScaleFactor), (int)(topLeftInt.Y * yScaleFactor)),
                new IntPoint((int)((input.Width + topRightInt.X) * xScaleFactor), (int)(topRightInt.Y * yScaleFactor)),
                new IntPoint((int)((input.Width + bottomRightInt.X) * xScaleFactor), (int)((input.Height + bottomRightInt.Y) * yScaleFactor)),
                new IntPoint((int)(bottomLeftInt.X * xScaleFactor), (int)((input.Height + bottomLeftInt.Y) * yScaleFactor))
            };

            int transformedWidth = paddedImage.Width;
            int transformedHeight = paddedImage.Height;

            QuadrilateralTransformation filter = new QuadrilateralTransformation(destinationPoints, transformedWidth, transformedHeight);
            Bitmap transformedImage = filter.Apply(paddedImage);

            Bitmap adjustedImage = AdjustImageToFit(transformedImage, input);

            using (Graphics g = Graphics.FromImage(adjustedImage))
            {
                foreach (IntPoint p in sourcePoints)
                {
                    int x = Math.Max(0, Math.Min(adjustedImage.Width - 2, p.X));
                    int y = Math.Max(0, Math.Min(adjustedImage.Height - 2, p.Y));

                    g.DrawRectangle(Pens.Red, x, y, 2, 2);
                }
            }


            // Save the adjusted image
            return adjustedImage;
        }

        private static Bitmap AdjustImageToFit(Bitmap inputImage, Bitmap sourceImage)
        {
            if (inputImage == null)
                throw new ArgumentException("The provided input image is null.", nameof(inputImage));

            if (sourceImage == null)
                throw new ArgumentException("The provided source image is null.", nameof(sourceImage));

            int centerX = inputImage.Width / 2;
            int centerY = inputImage.Height / 2;

            double desiredDistanceSquared = Math.Pow(Math.Min(sourceImage.Width, sourceImage.Height) / 2.0, 2);
            double scaleFactor = 1;

            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);
            var bmpData = inputImage.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, inputImage.PixelFormat);
            int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(inputImage.PixelFormat) / 8;
            byte[] bytes = new byte[inputImage.Width * inputImage.Height * bytesPerPixel];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, bytes, 0, bytes.Length);
            inputImage.UnlockBits(bmpData);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    int bytePosition = y * bmpData.Stride + x * bytesPerPixel;
                    byte alpha = bytes[bytePosition + 3];

                    if (alpha > 0)
                    {
                        double distanceSquared = (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY);
                        if (distanceSquared > desiredDistanceSquared)
                        {
                            double requiredScaleFactor = desiredDistanceSquared / distanceSquared;
                            if (requiredScaleFactor < scaleFactor)
                                scaleFactor = requiredScaleFactor;
                        }
                    }
                }
            }

            int newWidth = (int)(inputImage.Width * Math.Sqrt(scaleFactor));
            int newHeight = (int)(inputImage.Height * Math.Sqrt(scaleFactor));
            Bitmap scaledImage = new Bitmap(inputImage, newWidth, newHeight);

            int xOffset = (sourceImage.Width - newWidth) / 2;
            int yOffset = (sourceImage.Height - newHeight) / 2;

            Bitmap finalImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            using (Graphics g = Graphics.FromImage(finalImage))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(scaledImage, new System.Drawing.Point(xOffset, yOffset));
            }

            return finalImage;
        }

        public static Bitmap RotateBitmap(Bitmap input, double radians)
        {
            Bitmap rotated = new Bitmap(input.Width, input.Height);

            float angleInDegrees = (float)(radians * (180.0 / Math.PI));

            using (Graphics g = Graphics.FromImage(rotated))
            {
                // Explicitly set graphics properties
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                g.TranslateTransform((float)input.Width / 2, (float)input.Height / 2);
                g.RotateTransform(angleInDegrees);
                g.TranslateTransform(-(float)input.Width / 2, -(float)input.Height / 2);
                g.DrawImage(input, new Rectangle(0, 0, rotated.Width, rotated.Height));
            }

            return rotated;

        }


    }
}
