using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Warhammer.Core.Abstract;
using Warhammer.Core.Models;

namespace Warhammer.Core.Concrete
{
    public class ImageProcessor : IImageProcessor
    {
        public List<ExtractedImage> GetImagesFromHtmlString(string html)
        {
            ConvertHtml convertHtml = new ConvertHtml();
            List<ExtractedImage> images = convertHtml.ExtractImagesInHtml(html);
            return images;
        }

        public Image ResizeImage(Image imgToResize, Size destinationSize)
        {
            var originalWidth = imgToResize.Width;
            var originalHeight = imgToResize.Height;

            //how many units are there to make the original length
            var hRatio = (float)originalHeight / destinationSize.Height;
            var wRatio = (float)originalWidth / destinationSize.Width;

            //get the shorter side
            var ratio = Math.Min(hRatio, wRatio);

            var hScale = Convert.ToInt32(destinationSize.Height * ratio);
            var wScale = Convert.ToInt32(destinationSize.Width * ratio);

            //start cropping from the center
            var startX = (originalWidth - wScale) / 2;
            var startY = (originalHeight - hScale) / 2;

            //crop the image from the specified location and size
            var sourceRectangle = new Rectangle(startX, startY, wScale, hScale);

            //the future size of the image
            var bitmap = new Bitmap(destinationSize.Width, destinationSize.Height);

            //fill-in the whole bitmap
            var destinationRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            //generate the new image
            using (var g = Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
            }

            return bitmap;
        }

        public byte[] GetJpegFromImage(Image image)
        {
            return ToByteArray(image, ImageFormat.Jpeg);
        }

        public Image Crop(Image image, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(image);
            Bitmap bmpCrop = bmpImage.Clone(cropArea,
            bmpImage.PixelFormat);
            return bmpCrop;
        }

        public byte[] GetPngFromImage(Image image)
        {
            return ToByteArray(image, ImageFormat.Png);
        }

        public static byte[] ToByteArray(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        public Image RoundCorners(byte[] image, Color? borderColor = null, int? cornerRadius = null, Color? backgroundColor = null)
        {
            using (MemoryStream ms = new MemoryStream(image))
            {
                Image startImage = Image.FromStream(ms);

                int radius = cornerRadius ?? startImage.Width / 2;
                radius *= 2;

                if (backgroundColor == null)
                {
                    backgroundColor = Color.Transparent;
                }

                if (borderColor == null)
                {
                    borderColor = Color.Green;
                }


                Bitmap roundedImage = new Bitmap(startImage.Width, startImage.Height);
                Graphics g = Graphics.FromImage(roundedImage);
                g.Clear(backgroundColor.Value);
                g.SmoothingMode = SmoothingMode.AntiAlias;


                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0,0,startImage.Width, startImage.Height);

                PathGradientBrush pgb = new PathGradientBrush(path);

                pgb.CenterPoint = new PointF(startImage.Width / 2,startImage.Height / 2);
                pgb.CenterColor = Color.Transparent;
                pgb.SurroundColors = new Color[] { borderColor.Value };
                pgb.FocusScales = new PointF(0f, 0f);

                Blend blnd = new Blend();
                blnd.Positions = new float[] { 0f, 0.1f, 0.2f, 1f };
                blnd.Factors = new float[] { 0f, 0f, 1f, 1f };
                pgb.Blend = blnd;
                g.FillPath(pgb, path);

                Brush brush = new TextureBrush(startImage);
                GraphicsPath gp = new GraphicsPath();
                gp.AddArc(0, 0, radius, radius, 180, 90);
                gp.AddArc(0 + roundedImage.Width - radius, 0, radius, radius, 270, 90);
                gp.AddArc(0 + roundedImage.Width - radius, 0 + roundedImage.Height - radius, radius, radius, 0, 90);
                gp.AddArc(0, 0 + roundedImage.Height - radius, radius, radius, 90, 90);
                g.FillPath(brush, gp);
                g.FillPath(pgb, path);
                return roundedImage;
            }
        }

        public Image GetImageFromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }
    }
}
