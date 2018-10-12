namespace Warhammer.Core.Models
{
    public class PageToggleModel
    {
        public int PageId { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public bool Selected { get; set; }
    }
}