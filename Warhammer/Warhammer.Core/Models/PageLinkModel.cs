namespace Warhammer.Core.Models
{
    public class PageLinkModel
    {
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public PageLinkType Type { get; set; }

        public PageLinkModel()
        {
            
        }
    }
}