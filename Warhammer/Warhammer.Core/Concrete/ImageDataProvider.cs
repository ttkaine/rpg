using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class ImageDataProvider : IImageDataProvider
    {
        private readonly IImageRepository _imageRepository;
        

        public ImageDataProvider(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public PageImage GetPageImageForPage(int id)
        {
            return _imageRepository.PageImages().FirstOrDefault(p => p.IsPrimary && p.PageId == id);
        }

        public string GetPageName(int id)
        {
            return _imageRepository.Pages().Where(p => p.Id == id).Select(p => p.ShortName).FirstOrDefault();
        }
    }

}