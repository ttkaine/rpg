namespace Warhammer.Core.Entities
{
    public partial class PersonAttribute
    {
        public bool IsStatType
        {
            get
            {
                return AttributeType == AttributeType.Stat || AttributeType == AttributeType.Magic || AttributeType == AttributeType.MagicItem;
            }
        }

        public bool Remove { get; set; }

        public AttributeType AttributeType
        {
            get { return (AttributeType)PersonAttributeTypeEnum; }
            set { PersonAttributeTypeEnum = (int)value; }
        }
    }
}