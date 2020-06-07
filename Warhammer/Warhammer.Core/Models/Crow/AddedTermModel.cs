using System.Collections.Generic;
using System.ComponentModel;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Models.Crow
{
    public class AddedTermModel
    {
        public Term Term { get; set; }
        public CrowCharacterAttributesModel Attributes { get; set; }
        [DisplayName("Select an Existing Role")]
        public int? SelectedRole { get; set; }
        [DisplayName("Add a new role")]
        public string AddedRole { get; set; }
        [DisplayName("Select an Existing Skill")]
        public int? SelectedSkill1 { get; set; }
        [DisplayName("Add a new skill")]
        public string AddedSkill1 { get; set; }
        [DisplayName("Select an Existing Skill")]
        public int? SelectedSkill2 { get; set; }
        [DisplayName("Add a new skill")]
        public string AddedSkill2 { get; set; }
    }
}