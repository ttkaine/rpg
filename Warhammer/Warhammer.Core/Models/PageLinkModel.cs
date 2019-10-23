using System;
using Warhammer.Core.Extensions;

namespace Warhammer.Core.Models
{
    public class PageLinkModel
    {
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public PageLinkType Type { get; set; }
        public DateTime Created { get; set; }
        public Type BaseType { get; set; }
        public string FileIdentifier { get; set; }
        public string ImageUrl => FileIdentifier.ToImageUrl();

        public PageLinkModel()
        {
            
        }
    }
}