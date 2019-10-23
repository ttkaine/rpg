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

        public string GetImagefilenameForPage(int id)
        {
            return _imageRepository.Pages().Where(p => p.Id == id).Select(p => p.FileIdentifier).FirstOrDefault();
        }
    }

}