namespace Warhammer.Core.Abstract
{
    public interface IDatabaseUpdateProvider
    {
        bool PerformUpdates(string scriptFolder);
    }
}
