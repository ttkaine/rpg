namespace Warhammer.Core.Entities
{
    public  partial class Creature
    {
        public ThreatLevel ThreatLevel
        {
            get { return (ThreatLevel) ThreatLevelId; }
            set { ThreatLevelId = (int) value; }
        }

        public string Breadcrumb
        {
            get
            {
                if (ParentCreature == null)
                {
                    return ShortName;
                }
                else
                {
                    return ParentCreature.Breadcrumb + " > " + ShortName;
                }
            }
        }
    }
}
