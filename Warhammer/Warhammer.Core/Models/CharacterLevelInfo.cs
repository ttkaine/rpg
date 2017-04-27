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
        public int AverageStatValue { get; set; }
        public int NumberOfStats { get; set; }
        public int TotalAverageStatValue => AverageStatValue*NumberOfStats;
        public int XpSpent { get; set; }
        public int TotalWear { get; set; }
        public int TotalHarm { get; set; }
        public int NumberOfWear { get; set; }
        public int NumberOfHarm { get; set; }
        public bool HasAttributeMoveAvailable { get; set; }
    }
}