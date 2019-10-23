using Warhammer.Core.Models;

namespace Warhammer.Mvc.Models
{
    public class ImageViewModel
    {
        public ImageViewModel()
        {

        }

        public ImageViewModel(PageLinkModel pageLink)
        {
            Id = pageLink.Id;
            Name = pageLink.ShortName;
            Url = pageLink.ImageUrl;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}