using System.Collections.Generic;
using System.Web.Mvc;
using Warhammer.Core;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class ManagePersonAttributesViewModel
    {
        public List<PersonAttribute> Attributes { get; set; }
        public AttributeType? NewAttributeType { get; set; }
        public int NewAttributeValue { get; set; }
        public string NewAttributeName { get; set; }
        public string NewAttributeDescription { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public bool NewAttributeIsPopulated => NewAttributeType.HasValue &&
                                               !string.IsNullOrWhiteSpace(NewAttributeName) && NewAttributeValue != 0;

        public bool NewAttributeIsPrivate { get; set; }
    }
}