using System.Collections.Generic;
using System.Linq;

namespace Warhammer.Core.Models.Crow
{
    public class CrowCharacterAttributesModel
    {
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public int? PlayerId { get; set; }


        public List<CrowAttributeModel> PersonAttributes { get; set; }

        public List<CrowAttributeModel> Stats => PersonAttributes.Where(a => a.AttributeType == AttributeType.Stat).OrderBy(a => a.Id).ToList();
        public List<CrowAttributeModel> Magic => PersonAttributes.Where(a => a.AttributeType == AttributeType.Magic).OrderBy(a => a.Id).ToList();
        public List<CrowAttributeModel> MagicItems => PersonAttributes.Where(a => a.AttributeType == AttributeType.MagicItem).OrderBy(a => a.Id).ToList();
        public List<CrowAttributeModel> Skills => PersonAttributes.Where(a => a.AttributeType == AttributeType.Skill).OrderBy(a => a.Name).ToList();
        public List<CrowAttributeModel> Roles => PersonAttributes.Where(a => a.AttributeType == AttributeType.Role).OrderBy(a => a.Name).ToList();
        public List<CrowAttributeModel> Disciplines => PersonAttributes.Where(a => a.AttributeType == AttributeType.Discipline).OrderBy(a => a.Name).ToList();
        public List<CrowAttributeModel> Resilience => PersonAttributes.Where(a => a.AttributeType == AttributeType.Resilience).OrderBy(a => a.Name).ToList();
        public List<CrowAttributeModel> Resolve => PersonAttributes.Where(a => a.AttributeType == AttributeType.Resolve).OrderBy(a => a.Name).ToList();
        public List<CrowAttributeModel> Descriptors => PersonAttributes.Where(a => a.AttributeType == AttributeType.Descriptor).OrderBy(a => a.Name).ToList();
        public List<CrowAttributeModel> Wear => PersonAttributes.Where(a => a.AttributeType == AttributeType.Wear).OrderBy(a => a.CurrentValue).ToList();
        public List<CrowAttributeModel> Harm => PersonAttributes.Where(a => a.AttributeType == AttributeType.Harm).OrderBy(a => a.CurrentValue).ToList();
        public List<CrowAttributeModel> Edge => PersonAttributes.Where(a => a.AttributeType == AttributeType.Edge).OrderBy(a => a.CurrentValue).ToList();


    }
}