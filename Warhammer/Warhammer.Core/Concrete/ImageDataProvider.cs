using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Core.Concrete
{
    public class ImageDataProvider : IImageDataProvider
    {
        private readonly IImageRepository _imageRepository;
        

        public ImageDataProvider(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public PageImage GetPageImageForPage(int id, bool includeAll = false)
        {
            if (includeAll)
            {
                return _imageRepository.AllPageImages().FirstOrDefault(p => p.IsPrimary && p.PageId == id);
            }
            else
            {
                return _imageRepository.PageImages().FirstOrDefault(p => p.IsPrimary && p.PageId == id);
            }
           
        }

        public string GetPageName(int id)
        {
            return _imageRepository.Pages().Where(p => p.Id == id).Select(p => p.ShortName).FirstOrDefault();
        }

        public TrophyImage GetPageImageForTrophy(int id)
        {
            TrophyImage image = _imageRepository.Trophies().Where(t => t.Id == id)
                .Select(t => new TrophyImage { Data = t.ImageData, Mime = t.MimeType, Name = t.Name }).FirstOrDefault();
            return image;
        }

        public string GetImagefilenameForPage(int id)
        {
            string fileId = _imageRepository.AllPageImages().Where(p => p.IsPrimary && p.PageId == id).Select(i => i.FileIdentifier).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(fileId))
            {
                fileId = "default.png";
            }

            return fileId;
        }
    }

}