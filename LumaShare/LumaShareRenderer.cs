using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace LumaShare
{
    public static class LumaSharepicRenderer
    {
        public static RenderTargetBitmap RenderSharepic(string pathToTopImage, string pathToBottomImage, Profile prof, BackgroundSettingEnum backgroundSettings, int border)
        {
            int width;
            int height;
            int leftOffset;
            int topOffset;

            BitmapImage imgProfile = GetBitmapImage(prof.ImageFileName);

            width = imgProfile.PixelWidth + (border * 2);
            height = imgProfile.PixelHeight + (border * 2);
            leftOffset = border;
            topOffset = border;

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                if (backgroundSettings != BackgroundSettingEnum.Transparent)
                {
                    string blurImagePath;

                    if (backgroundSettings == BackgroundSettingEnum.TopScreenBlurred)
                    {
                        blurImagePath = pathToTopImage;
                    }
                    else
                    {
                        blurImagePath = pathToBottomImage;
                        
                    }

                    if (File.Exists(blurImagePath))
                    {
                        BitmapImage imgBlur = GetBitmapImage(blurImagePath);

                        float scalingFactor = 0;

                        scalingFactor = (float)height / (float)imgBlur.PixelHeight;

                        float targetWidth = imgBlur.PixelWidth * scalingFactor;
                        float targetHeight = imgBlur.PixelHeight * scalingFactor;

                        Bitmap tempBitmap = ConvertBitmapImageToBitmap(imgBlur);

                        Bitmap blurBitmap = Blur(tempBitmap, new Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height), 3);

                        BitmapImage blurImage = ConvertBitmapToBitmapImage(blurBitmap);

                        dc.DrawImage(blurImage, new Rect(((targetWidth - width) / 2) * -1, 0, targetWidth, targetHeight));
                    }
                }

                dc.DrawImage(imgProfile, new Rect(leftOffset, topOffset, imgProfile.PixelWidth, imgProfile.PixelHeight));

                if (File.Exists(pathToTopImage))
                {
                    BitmapImage imgTop = GetBitmapImage(pathToTopImage);
                    dc.DrawImage(imgTop, new Rect(prof.TopScreenStart.X + leftOffset, prof.TopScreenStart.Y + topOffset, prof.TopScreenSize.Width, prof.TopScreenSize.Height));
                }
                else
                {
                    dc.DrawRectangle(System.Windows.Media.Brushes.Black, null, new Rect(prof.TopScreenStart.X + leftOffset, prof.TopScreenStart.Y + topOffset, prof.TopScreenSize.Width, prof.TopScreenSize.Height));
                }

                if (File.Exists(pathToBottomImage))
                {
                    BitmapImage imgTop = GetBitmapImage(pathToBottomImage);
                    dc.DrawImage(imgTop, new Rect(prof.BottomScreenStart.X + leftOffset, prof.BottomScreenStart.Y + topOffset, prof.BottomScreenSize.Width, prof.BottomScreenSize.Height));
                }
                else
                {
                    dc.DrawRectangle(System.Windows.Media.Brushes.Black, null, new Rect(prof.BottomScreenStart.X + leftOffset, prof.BottomScreenStart.Y + topOffset, prof.BottomScreenSize.Width, prof.BottomScreenSize.Height));
                }
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);

            return rtb;
        }

        private static BitmapImage GetBitmapImage(string imageFile)
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(imageFile, UriKind.Absolute);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();

            return src;
        }

        private static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public static System.Drawing.Bitmap ConvertBitmapImageToBitmap(BitmapImage bmpImg)
        {
            if (bmpImg == null)
                return null;

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bmpImg));
                enc.Save(outStream);
                return new System.Drawing.Bitmap(outStream);
            }
        }

        private static Bitmap Blur(Bitmap image, Rectangle rectangle, Int32 blurSize)
        {
            Bitmap blurred = new Bitmap(image.Width, image.Height);

            // make an exact copy of the bitmap provided
            using (Graphics graphics = Graphics.FromImage(blurred))
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

            // look at every pixel in the blur rectangle
            for (Int32 xx = rectangle.X; xx < rectangle.X + rectangle.Width; xx++)
            {
                for (Int32 yy = rectangle.Y; yy < rectangle.Y + rectangle.Height; yy++)
                {
                    Int32 avgR = 0, avgG = 0, avgB = 0;
                    Int32 blurPixelCount = 0;

                    // average the color of the red, green and blue for each pixel in the
                    // blur size while making sure you don't go outside the image bounds
                    for (Int32 x = xx; (x < xx + blurSize && x < image.Width); x++)
                    {
                        for (Int32 y = yy; (y < yy + blurSize && y < image.Height); y++)
                        {
                            System.Drawing.Color pixel = blurred.GetPixel(x, y);

                            avgR += pixel.R;
                            avgG += pixel.G;
                            avgB += pixel.B;

                            blurPixelCount++;
                        }
                    }

                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;

                    // now that we know the average for the blur size, set each pixel to that color
                    for (Int32 x = xx; x < xx + blurSize && x < image.Width && x < rectangle.Width; x++)
                        for (Int32 y = yy; y < yy + blurSize && y < image.Height && y < rectangle.Height; y++)
                            blurred.SetPixel(x, y, System.Drawing.Color.FromArgb(avgR, avgG, avgB));
                }
            }

            return blurred;
        }
    }

    public class Profile
    {
        public string ProfileName { get; set; }
        public string ImageFileName;
        public System.Windows.Point TopScreenStart;
        public System.Windows.Size TopScreenSize;
        public System.Windows.Point BottomScreenStart;
        public System.Windows.Size BottomScreenSize;

        public Profile(string profileXmlFile)
        {
            XmlDocument docProfile = new XmlDocument();
            docProfile.Load(profileXmlFile);

            XmlNode rootNode = docProfile.ChildNodes.Item(0);
            ProfileName = rootNode.Attributes.GetNamedItem("Name").InnerText;

            string directory = Path.GetDirectoryName(profileXmlFile);
            ImageFileName = Path.Combine(directory, rootNode.Attributes.GetNamedItem("ImageFileName").InnerText);

            XmlNode nodeTopScreen = rootNode.ChildNodes.Item(0);
            string[] topScreenStart = nodeTopScreen.Attributes.GetNamedItem("Start").InnerText.Split(',');
            TopScreenStart = new System.Windows.Point(Convert.ToInt32(topScreenStart[0]), Convert.ToInt32(topScreenStart[1]));

            string[] topScreenSize = nodeTopScreen.Attributes.GetNamedItem("Size").InnerText.Split(',');
            TopScreenSize = new System.Windows.Size(Convert.ToInt32(topScreenSize[0]), Convert.ToInt32(topScreenSize[1]));

            XmlNode nodeBottomScreen = rootNode.ChildNodes.Item(1);
            string[] bottomScreenStart = nodeBottomScreen.Attributes.GetNamedItem("Start").InnerText.Split(',');
            BottomScreenStart = new System.Windows.Point(Convert.ToInt32(bottomScreenStart[0]), Convert.ToInt32(bottomScreenStart[1]));

            string[] bottomScreenSize = nodeBottomScreen.Attributes.GetNamedItem("Size").InnerText.Split(',');
            BottomScreenSize = new System.Windows.Size(Convert.ToInt32(bottomScreenSize[0]), Convert.ToInt32(bottomScreenSize[1]));
        }
    }

    public enum BackgroundSettingEnum
    {
        Transparent,
        TopScreenBlurred,
        BottomScreenBlurred
    }
}
