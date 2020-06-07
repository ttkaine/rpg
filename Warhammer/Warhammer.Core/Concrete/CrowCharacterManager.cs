using System;
using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models.Crow;

namespace Warhammer.Core.Concrete
{
    public class CrowCharacterManager : ICrowCharacterManager
    {
        private readonly IRepository _repo;

        public CrowCharacterManager(IRepository repository)
        {
            _repo = repository;
        }

        public CrowCharacterAttributesModel GetCharacterAttributes(int id)
        {
            CrowCharacterAttributesModel model = new CrowCharacterAttributesModel();
            Person person = _repo.People().Single(p => p.Id == id);
            model.PlayerId = person.PlayerId;
            model.PersonId = id;
            model.PersonAttributes = _repo.PersonAttributes().Where(a => a.PersonId == id).ToList().Select(c => (CrowAttributeModel)new CrowAttributeModel().InjectFrom(c)).ToList();
            return model;
        }

        public List<DefaultPersonAttribute> GetDefaultPersonAttributes()
        {
            return _repo.DefaultPersonAttributes().ToList();
        }

        public CrowCharacterGenerationModel GetCharacterGenerationModel(int id)
        {
            CrowCharacterGenerationModel model = new CrowCharacterGenerationModel();
            model.Attributes = GetCharacterAttributes(id);
            Person person = _repo.People().Single(p => p.Id == id);
            model.PlayerId = person.PlayerId;
            model.Person = person;
            model.Terms = _repo.Terms().Where(t => t.PersonId == id).ToList();
            model.NextTerm = new AddedTermModel
            {
                Term = new Term
                { 
                    PlayerId = person.PlayerId.GetValueOrDefault(),
                    PersonId = person.Id,
                },
                Attributes = model.Attributes
            };
            if (model.Terms.Any())
            {
                model.NextTerm.Term.TermNumber = model.Terms.Max(t => t.TermNumber) + 1;
            }

            foreach (Term term in model.Terms)
            {
                term.StatOptions = model.Attributes.Stats;
            }
            return model;
        }

        public void InitializeCharacter(int id)
        {
            Person person = _repo.People().Single(p => p.Id == id);

            List<DefaultPersonAttribute> defaults = GetDefaultPersonAttributes();

            CrowCharacterAttributesModel current = GetCharacterAttributes(id);

            foreach (var attribute in current.PersonAttributes)
            {
                _repo.Delete(_repo.PersonAttributes().Single(s => s.Id == attribute.Id));
            }

            foreach (DefaultPersonAttribute attribute in defaults)
            {
                if (!person.IsNpc || attribute.IncludeForNpc)
                {
                    PersonAttribute personAttribute = new PersonAttribute();
                    personAttribute.InjectFrom(attribute);
                    personAttribute.Id = 0;
                    personAttribute.CurrentValue = attribute.InitialValue;
                    personAttribute.PersonId = person.Id;
                    personAttribute.XpSpent = 0;
                    personAttribute.PersonAttributeTypeEnum = attribute.PersonAttributeTypeEnum;
                    _repo.Save(personAttribute);
                }
            }
        }

        public void AddTerm(AddedTermModel addedTerm)
        {
            Term term = new Term();
            term.InjectFrom(addedTerm.Term);

            ExperiencePoint roleExperiencePoint = new ExperiencePoint{
                PersonId = addedTerm.Term.PersonId,
                PlayerId = addedTerm.Term.PlayerId,
                Awarded = DateTime.UtcNow
            };

            int roleId = 0;
            if (!string.IsNullOrWhiteSpace(addedTerm.AddedRole))
            {
                roleId = AddRole(addedTerm.Term.PersonId, addedTerm.AddedRole);
            }

            if (addedTerm.SelectedRole.HasValue)
            {
                roleId = addedTerm.SelectedRole.Value;
            }

            roleExperiencePoint.PersonAttributeId = roleId;

            for (int i = 0; i < 4; i++)
            {
                ExperiencePoint xp = (ExperiencePoint)new ExperiencePoint().InjectFrom(roleExperiencePoint);
                term.ExperiencePoints.Add(xp);
            }

            GetSkillXpForNewTerm(term, addedTerm.SelectedSkill1, addedTerm.AddedSkill1);
            GetSkillXpForNewTerm(term, addedTerm.SelectedSkill2, addedTerm.AddedSkill2);

            _repo.Save(term);
        }

        private void GetSkillXpForNewTerm(Term term, int? skillId, string skillName)
        {
            ExperiencePoint skillExperiencePoint = new ExperiencePoint
            {
                PersonId = term.PersonId,
                PlayerId = term.PlayerId,
                Awarded = DateTime.UtcNow
            };

            if (!string.IsNullOrWhiteSpace(skillName))
            {
                int addedSkillId = AddSkill(term.PersonId, skillName);
                if (!skillId.HasValue)
                {
                    skillId = addedSkillId;
                }
            }

            skillExperiencePoint.PersonAttributeId = skillId;
            term.ExperiencePoints.Add(skillExperiencePoint);
        }

        private int AddSkill(int personId, string skill)
        {
            PersonAttribute attribute = new PersonAttribute();
            attribute.AttributeType = AttributeType.Skill;
            attribute.Name = skill;
            attribute.PersonId = personId;
            return _repo.Save(attribute);
        }

        private int AddRole(int personId, string role)
        {
            PersonAttribute attribute = new PersonAttribute();
            attribute.AttributeType = AttributeType.Role; 
            attribute.Name = role;
            attribute.PersonId = personId;
            return _repo.Save(attribute);
        }
    }
}