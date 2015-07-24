using System.Drawing;

namespace Warhammer.Core.Abstract
{
    public interface IImageProcessor
    {
        Image GetImageFromHtmlString(string html);
        Image ResizeImage(Image imgToResize, Size destinationSize);
        byte[] GetJpegFromImage(Image image);
        Image Crop(Image image, Rectangle cropArea);
    }
}
