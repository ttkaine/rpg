using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using Warhammer.Core.Models;

namespace Warhammer.Core.Concrete
{
    /// <summary>
    /// ConvertHtml - main class to encode (embed) and decode (extract) images to/from Html.
    /// 
    /// Author: Dennis Lang - 2014
    /// http://home.comcast.net/~lang.dennis/
    ///
    /// This file is part of InlineHtmlImages.
    ///
    /// InlineHtmlImages is free software: you can redistribute it and/or modify
    /// it under the terms of the GNU General Public License as published by
    /// the Free Software Foundation, either version 3 of the License, or
    /// (at your option) any later version.
    ///
    /// InlineHtmlImages is distributed in the hope that it will be useful,
    /// but WITHOUT ANY WARRANTY; without even the implied warranty of
    /// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    /// GNU General Public License for more details.
    ///
    /// The GNU General Public License: http://www.gnu.org/licenses/

    /// </summary>
    class ConvertHtml
    {
        readonly Regex _imagePat = new Regex("(<img [^>]+|image: *url[^)]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        readonly Regex _imageSrcPat = new Regex("src=(\"[^\"]+\"|'[^']+'| *[^ ]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        readonly Regex _imageUrlPat = new Regex(@"url\(([^)]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public ConvertHtml()
        {
            OkayToOverwrite = false;
        }

        public bool OkayToOverwrite
        {
            get;
            set;
        }

        /// <summary>
        /// Convert base 64 data to an image.
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        private static Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        public List<ExtractedImage> ExtractImagesInHtml(string text)
        {
            List<ExtractedImage> images = new List<ExtractedImage>();
            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    const char separator1 = ',';
                    char separator2 = char.MinValue;

                    foreach (Match match in _imagePat.Matches(text))
                    {
                        int pos = match.Index;
                        string imgStr = text.Substring(pos, match.Length);
                        Match mImg;
                        switch (char.ToLower(imgStr[0]))
                        {
                            case 'i': // background-image: url(foo.png);
                                mImg = _imageUrlPat.Match(imgStr);
                                separator2 = char.MinValue;
                                break;
                            case '<':
                                mImg = _imageSrcPat.Match(imgStr);
                                separator2 = '\n';
                                break;
                            default:
                                mImg = null;
                                break;
                        }

                        if (mImg != null && mImg.Groups.Count == 2)
                        {
                            string imageData = mImg.Groups[1].Value.Trim().Replace("'", "").Replace("\"", "");

                            if (imageData.StartsWith("data"))
                            {
                                int sepIdx = imageData.IndexOf(separator1);
                                if (separator2 != char.MinValue && imageData[sepIdx + 1] == separator2)
                                    sepIdx++;

                                string imageBase64 = imageData.Substring(sepIdx + 1);
                                try
                                {
                                    Image image = Base64ToImage(imageBase64);
                                    if (image != null)
                                    {
                                        images.Add(new ExtractedImage {Image = image, OriginalSrc = mImg.Value});
                                    }
                                }
// ReSharper disable once EmptyGeneralCatchClause
                                catch (Exception)
                                {
                                }
                            }
                            else
                            {
                                try
                                {
                                    //  Image bmp = Image.FromFile(imageData);
                                    //   images.Add(new ExtractedImage { Image = bmp, OriginalSrc = mImg.Value });
                                }
// ReSharper disable once EmptyGeneralCatchClause
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                }
// ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                }
            }

            return images;
        }
    }
}
