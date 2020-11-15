using System;
using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Models;

namespace Warhammer.Core.Concrete
{
    public class RandomItemGenerator : IRandomItemGenerator
    {
        private readonly Random _dice = new Random();


        private List<string> Tactics = new List<string>
        {
            "Ambush",
"Create barrier",
"Gang up",
"Manipulate",
"Scatter foes",
"Target nearest",
"Call for support",
"Deceive",
"Build strength",
"Mock",
"Stalk",
"Target richest",
"Capture",
"Demand duel",
"Go berserk",
"Monologue",
"Steal from",
"Target strong",
"Charge!",
"Disorient",
"Harry",
"Order minion",
"Swarm",
"Target weak",
"Climb foes",
"Encircle",
"Hurl foes",
"Protect leader",
"Target insolent",
"Toy with",
"Compel worship",
"Evade",
"Immobilise",
"Protect self",
"Target leader",
"Use terrain"
        };

        private List<string> Personality = new List<string>
        {
            "Alien",
"Devious",
"Fanatical",
"Jaded",
"Meticulous",
"Psychopathic",
"Aloof",
"Distractible",
"Forgetful",
"Jovial ",
"Mystical",
"Sophisticated",
"Bored",
"Educated",
"Generous",
"Legalistic",
"Obsessive",
"Touchy",
"Cautious",
"Embittered",
"Hateful",
"Manipulative",
"Out of touch",
"Unimpressed",
"Cowardly",
"Envious",
"Honourable",
"Megalomaniac",
"Paranoid",
"Vain",
"Curious",
"Erudite",
"Humble",
"Melancholy",
"Polite",
"Xenophobic"
        };

        private List<string> Weakness = new List<string>
        {
            "Bells",
"Conversation",
"Heat",
"Mistletoe",
"Puzzles",
"True Name",
"Birdsong",
"Deformity",
"Holy Icon",
"Moonlight",
"Riddles",
"Val. Materials",
"Children",
"Flattery",
"Holy Water",
"Music",
"Rituals",
"Weak spot",
"Cold",
"Flowers",
"Home Cooking",
"Methods",
"Silver",
"Weapon Items",
"Cold Iron",
"Gifts",
"Insanities ",
"Phylactery",
"Sunlight",
"Wine",
"Competition",
"Gold",
"Mirrors",
"Phys. Elements",
"Tears",
"Wormwood"
        };

        private List<string> Abilities = new List<string>
        {
        "Absorbing",
"Duplicating",
"Gaze weapon",
"Mimicking",
"Radioactive",
"Strangling",
"Acid blood",
"Electric",
"Hypnotising",
"Mind-reading",
"Reflective",
"Super strength",
"Anti-magic",
"Entangling",
"Impervious",
"Paralysing",
"Regenerating",
"Telekinetic",
"Blinding",
"Ethereal Effect",
"Invisible",
"Phasing",
"Shapeshifting",
"Teleporting ",
"Breath weapon",
"Exploding",
"Life-draining",
"Physical Effect",
"Spell-casting",
"Vampiric",
"Camouflaging",
"Flying",
"Magnetic",
"Poisonous",
"Stealthy",
"Wall-crawling"
        };

        private List<string> Traits = new List<string>
        {
            "Amphibious",
"Crystalline",
"Fearless",
"Illusory",
"Phys. Element",
"Skeletal",
"Bloated",
"Decaying",
"Fluffy",
"Intelligent",
"Planar",
"Slimy",
"Brittle",
"Ether. Element",
"Fungal",
"Iridescent",
"Reflective",
"Sticky",
"Cannibal",
"Ethereal",
"Gelatinous",
"Luminous",
"Rubbery",
"Sticky",
"Clay-like",
"Ever young",
"Geometric",
"Many-headed",
"Shadowy",
"Tiny",
"Colossal",
"Eyeless",
"Hardened",
"Mechanical",
"Sharp",
"Translucent"
        };

        private List<string> Feature = new List<string>
        {
            "Antlers",
"Fangs",
"Legless",
"Plates",
"Shell",
"Tail",
"Beak",
"Fins",
"Long tongue",
"Plumage",
"Spikes",
"Talons",
"Carapace",
"Fur",
"Many-eyed",
"Proboscis",
"Spinnerets",
"Tentacles",
"Claws",
"Gills",
"Many-limbed",
"Scales",
"Spines",
"Trunk",
"Compound eyes",
"Hooves",
"Mucus",
"Segments",
"Stinger",
"Tusks",
"Eye Stalks",
"Horns",
"Pincers",
"Shaggy hair",
"Suction cups",
"Wings"
        };

        private List<string> Animal = new List<string> {
            "Albatross",
            "Crane",
            "Flamingo",
            "Kingfisher",
            "Moth",
            "Rooster",
            "Bat",
            "Crow",
            "Fly",
            "Locust",
            "Owl",
            "Sparrow",
            "Beetle",
            "Crow ",
            "Flying Squirrel",
            "Magpie",
            "Parrot",
            "Swan",
            "Bird of paradise",
            "Eagle",
            "Goose",
            "Mantis",
            "Peacock",
            "Vulture",
            "Butterfly",
            "Falcon",
            "Gull",
            "Mockingbird",
            "Pelican",
            "Wasp",
            "Condor",
            "Firefly",
            "Hummingbird",
            "Mosquito",
            "Pteranodon",
            "Woodpecker",
            "Ant",
"Caterpillar",
"Ferret",
"Mole",
"Rat",
"Snake",
"Ape",
"Centipede",
"Fox",
"Ostrich",
"Rhinoceros",
"Spider",
"Armadillo",
"Chameleon",
"Giraffe",
"Ox",
"Scorpion",
"Squirrel",
"Badger",
"Cockroach",
"Goat",
"Porcupine",
"Sheep",
"Tiger",
"Bear",
"Deer",
"Horse",
"Rabbit",
"Slug",
"Wolf",
"Boar",
"Elephant",
"Human",
"Raccoon",
"Snail",
"Wolverine",
"Alligator",
"Dolphin",
"Lobster",
"Octopus",
"See Anemone",
"Squid",
"Amoeba",
"Eel",
"Manatee",
"Otter",
"Sea Urchin",
"Swordfish",
"Anglerfish",
"Frog",
"Manta Ray",
"Penguin",
"Seahorse",
"Tadpole",
"Beaver",
"Hippopotamus",
"Muskrat",
"Platypus",
"Seal",
"Turtle",
"Clam",
"Jellyfish",
"Narwhal",
"Pufferfish",
"Shark",
"Walrus",
"Crab",
"Leech",
"Newt",
"Salamander",
"Shrimp",
"Whale"
        };


        private int Roll(int sides = 6, int count = 1)
        {
            int total = 0;
            for (int i = 0; i < count; i++)
            {
                total = total + _dice.Next(1, sides + 1);
            }
            return total;
        }

        public RandomItemResult PersonAge()
        {
            int age = Roll(50) + Roll(6, 4);
            return new RandomItemResult { Name = "Age", Content = age.ToString()};
        }

        public RandomItemResult PersonSex()
        {
            int roll = Roll(2);
            string sex = roll == 1 ? "Male" : "Female";
            return new RandomItemResult { Name = "Sex", Content = sex };
        }

        public RandomItemResult PersonOrientation()
        {
            int roll = Roll(20);
            string orientation;
            switch (roll)
            {
                case 20:
                    orientation = "Gay";
                    break;
                case 19:
                    orientation = "Bisexual";
                    break;
                default:
                    orientation = "Straight";
                    break;
            }

            return new RandomItemResult { Name = "Sex", Content = $"{orientation} ({roll})" };
        }

        public RandomItemResult MonsterCreature()
        {
            return new RandomItemResult
            {
                Name = "Animal",
                Content = Animal.OrderBy(s => s).Skip(Roll(Animal.Count, 1)-1).FirstOrDefault()
            };
        }

        public RandomItemResult MonsterFeature()
        {
            return new RandomItemResult
            {
                Name = "Feature",
                Content = Feature.OrderBy(s => s).Skip(Roll(Feature.Count, 1) - 1).FirstOrDefault()
            };
        }

        public RandomItemResult MonsterTrait()
        {
            return new RandomItemResult
            {
                Name = "Trait",
                Content = Traits.OrderBy(s => s).Skip(Roll(Traits.Count, 1) - 1).FirstOrDefault()
            };
        }

        public RandomItemResult MonsterAbility()
        {
            return new RandomItemResult
            {
                Name = "Ability",
                Content = Abilities.OrderBy(s => s).Skip(Roll(Abilities.Count, 1) - 1).FirstOrDefault()
            };
        }

        public RandomItemResult MonsterTactic()
        {
            return new RandomItemResult
            {
                Name = "Tactic",
                Content = Tactics.OrderBy(s => s).Skip(Roll(Tactics.Count, 1) - 1).FirstOrDefault()
            };
        }

        public RandomItemResult MonsterPersonality()
        {
            return new RandomItemResult
            {
                Name = "Personality",
                Content = Personality.OrderBy(s => s).Skip(Roll(Personality.Count, 1) - 1).FirstOrDefault()
            };
        }

        public RandomItemResult MonsterWeakness()
        {
            return new RandomItemResult
            {
                Name = "Weakness",
                Content = Weakness.OrderBy(s => s).Skip(Roll(Weakness.Count, 1) - 1).FirstOrDefault()
            };
        }
    }
}