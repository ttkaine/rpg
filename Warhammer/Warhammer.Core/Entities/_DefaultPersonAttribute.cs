namespace Warhammer.Core.Entities
{
    public partial class DefaultPersonAttribute
    {
        public bool Remove { get; set; }

        public AttributeType AttributeType
        {
            get { return (AttributeType)PersonAttributeTypeEnum; }
            set { PersonAttributeTypeEnum = (int)value; }
        }
    }
}