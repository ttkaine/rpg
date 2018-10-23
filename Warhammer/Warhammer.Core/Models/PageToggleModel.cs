namespace Warhammer.Core.Models
{
    public class PageToggleModel
    {
        public PageToggleModel()
        {
            ShowImage = true;
        }

        public int PageId { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public bool Selected { get; set; }
        public bool ShowImage { get; set; }
    }
}