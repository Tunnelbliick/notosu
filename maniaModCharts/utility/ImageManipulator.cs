using System;
using System.Drawing;
using System.IO;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Collections.Generic;
using AForge;
using System.Linq;
using System.Runtime.InteropServices;

namespace StorybrewScripts
{
    public static class ImageManipulator
    {

        private static double previousScaleFactor = 1;
        private static PointF previousCenter = new PointF(0, 0);
        private const double smoothingFactor = 0.2; // Adjust this as needed
        private static readonly object locker = new object();
        private static System.Collections.Concurrent.ConcurrentQueue<PointF> centerBuffer = new System.Collections.Concurrent.ConcurrentQueue<PointF>();
        private const int BufferSize = 5;  // Adjust as needed
        private static object lockObj = new object();
        public static void testImage(string path, int frames = 20)
        {
            frames = 120;
            int amountPerFrame = 2;
            int currentAmount = amountPerFrame;
            // Load the image
            Bitmap sourceImage = new Bitmap(Path.Combine(path, "sb", "receiver", "default.png"));

            for (int i = 0; i < frames; i++)
            {

                // Pad the source image to 256x256
                Bitmap paddedImage = new Bitmap(sourceImage.Width * 2 + 100, sourceImage.Height * 2 + 100);
                using (Graphics g = Graphics.FromImage(paddedImage))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(sourceImage, new System.Drawing.Point(sourceImage.Width / 2 + 25, sourceImage.Height / 2 + 25)); // centered
                }

                // Define the destination points for the transformation
                List<IntPoint> sourcePoints = new List<IntPoint>
                {
                    new IntPoint(0, currentAmount), new IntPoint(sourceImage.Width, 0),
                    new IntPoint(sourceImage.Width, sourceImage.Height),
                    new IntPoint(0, sourceImage.Height - currentAmount)
                };

                // Define the destination points for the transformation
                List<IntPoint> destinationPoints = new List<IntPoint>
                {
                    new IntPoint(0, currentAmount), new IntPoint(paddedImage.Width, 0),
                    new IntPoint(paddedImage.Width, paddedImage.Height),
                    new IntPoint(0, paddedImage.Height - currentAmount)
                };

                // Create the transformation filter with desired dimensions
                int transformedWidth = paddedImage.Width;
                int transformedHeight = paddedImage.Height;
                QuadrilateralTransformation filter = new QuadrilateralTransformation(destinationPoints, transformedWidth, transformedHeight);
                Bitmap transformedImage = filter.Apply(paddedImage);

                Bitmap adjustedImage = AdjustImageToFit(transformedImage, sourceImage);

                using (Graphics g = Graphics.FromImage(adjustedImage))
                {
                    foreach (IntPoint p in sourcePoints)
                    {
                        // Ensure the points are within the boundaries of the 128x128 image
                        int x = Math.Max(0, Math.Min(adjustedImage.Width - 2, p.X));  // ensure x is within [0, 126]
                        int y = Math.Max(0, Math.Min(adjustedImage.Height - 2, p.Y)); // ensure y is within [0, 126]

                        // Draw a rectangle around each point
                        g.DrawRectangle(Pens.Red, x, y, 2, 2);
                    }
                }


                // Save the adjusted image
                adjustedImage.Save(Path.Combine(path, "sb", "receiver", "test", $"test{i}.png"));


                if (i > 59)
                {
                    currentAmount -= amountPerFrame;
                }
                else
                {
                    currentAmount += amountPerFrame;
                }
            }
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

            // Lock bits for faster pixel access
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
                    byte alpha = bytes[bytePosition + 3]; // Assuming image is 32bpp (position + 3 is the alpha channel)

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


    }
}
