namespace Warhammer.Core.Models
{
    public class CharacterLevelInfo
    {
        public int TotalAdvancesTaken { get; set; }
        public decimal CurrentXp { get; set; }
        public int TotalRoles { get; set; }
        public int TotalStats { get; set; }
        public int TotalSkills { get; set; }
        public int TotalDescriptors { get; set; }
        public bool CanEdit { get; set; }
    }
}