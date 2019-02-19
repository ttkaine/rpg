using System.Collections.Generic;
using System.Drawing;
using Warhammer.Core.Models;

namespace Warhammer.Core.Abstract
{
    public interface IImageProcessor
    {
        List<ExtractedImage> GetImagesFromHtmlString(string html);
        Image ResizeImage(Image imgToResize, Size destinationSize);
        byte[] GetJpegFromImage(Image image);
        Image Crop(Image image, Rectangle cropArea);
        byte[] GetPngFromImage(Image image);
        Image RoundCorners(byte[] imageData, Color? borderColor = null, int? cornerRadius = null, Color? backgroundColor = null);

        Image GetImageFromBytes(byte[] bytes);
    }
}
