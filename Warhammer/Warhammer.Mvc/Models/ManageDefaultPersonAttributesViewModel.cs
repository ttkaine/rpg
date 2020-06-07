using System.Collections.Generic;
using System.Web.Mvc;
using Warhammer.Core;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class ManageDefaultPersonAttributesViewModel
    {
        public List<DefaultPersonAttribute> Attributes { get; set; }
        public AttributeType? NewAttributeType { get; set; }
        public int NewAttributeValue { get; set; }
        public string NewAttributeName { get; set; }
        public string NewAttributeDescription { get; set; }
        public bool NewAttributeIsPopulated => NewAttributeType.HasValue &&
                                               !string.IsNullOrWhiteSpace(NewAttributeName) && NewAttributeValue != 0;

        public bool NewAttributeIsPrivate { get; set; }
        public bool NewAttributeIsNpc { get; set; }
    }
}