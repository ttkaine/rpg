namespace Warhammer.Core.Abstract
{
    public interface IDomainProvider
    {
        bool IsMasterDomain { get; }
        string CurrentDomain { get; }
    }
}