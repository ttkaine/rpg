namespace Warhammer.Core.Abstract
{
    public interface IApplicationInitalizeHandler
    {
        DatabaseUpdateResult UpdateDatabase(string scriptFolder);
    }
}