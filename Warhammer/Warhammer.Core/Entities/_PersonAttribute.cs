namespace Warhammer.Core.Entities
{
    public partial class PersonAttribute
    {
        public AttributeType AttributeType
        {
            get { return (AttributeType)PersonAttributeTypeEnum; }
            set { PersonAttributeTypeEnum = (int)value; }
        }
    }
}