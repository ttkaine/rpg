using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Models.Crow;

namespace Warhammer.Core.Entities
{
    public partial class Term
    {
        public string AgeBracket
        {
            get
            {
                int startAge = 14;
                for (int i = 0; i <= TermNumber; i++)
                {
                    startAge = startAge + i;
                }

                int endAge = startAge + TermNumber + 1;

                if (startAge == 14)
                {
                    return "0 - 14";
                }
                if (startAge == 15)
                {
                    return "15";
                }
                else
                {
                    return $"{startAge} - {endAge}";
                }
            }
        }

        public List<ExperiencePoint> RolePoints => ExperiencePoints
            .Where(s => s.PersonAttribute.PersonAttributeTypeEnum == (int) AttributeType.Role).ToList();
        public List<ExperiencePoint> SkillPoints => ExperiencePoints
            .Where(s => s.PersonAttribute.PersonAttributeTypeEnum == (int)AttributeType.Skill).ToList();

        public List<CrowAttributeModel> StatOptions { get; set; }
    }
}