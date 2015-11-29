namespace Warhammer.Core.Entities
{
    public partial class Comment
    {
        public bool IsPlayerComment
        {
            get { return !PersonId.HasValue; }
           
        }
    }
}
