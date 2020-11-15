namespace Warhammer.Mvc.Models
{
    public class RandomMonsterViewModel
    {
        public string FirstAnimal { get; set; }
        public string SecondAnimal { get; set; }
        public string Feature { get; set;}
        public string Trait { get; set; }
        public string Ability { get; set; }
        public string Tactic { get; set; }
        public string Personality { get; set; }
        public string Weakness { get; set; }

        public string Blurb => $"This is a strange monster that seems to be half {FirstAnimal} and half {SecondAnimal}. It has a distinct {Feature} and is uncommonly {Trait}. It uses its uncanny ability of {Ability}, using a tactic of {Tactic} to achieve it's goals. It is totally {Personality}. {Weakness} may be its only weakness!";
    }
}