using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Models.Crow
{
    public class CrowCharacterGenerationModel
    {
        public Person Person { get; set; }
        public int? PlayerId { get; set; }
        public CrowCharacterAttributesModel Attributes { get; set; }
        public List<Term> Terms { get; set; }
        public bool HasAttributes => Attributes != null && Attributes.PersonAttributes.Any();
        public AddedTermModel NextTerm { get; set; }
        public bool AddNewTerm { get; set; }
    }
}