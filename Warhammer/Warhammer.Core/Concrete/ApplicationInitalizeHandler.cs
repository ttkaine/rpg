using Warhammer.Core.Abstract;

namespace Warhammer.Core.Concrete
{
    public class ApplicationInitalizeHandler : IApplicationInitalizeHandler
    {
        private readonly IDatabaseUpdateProvider _dbUpdate;

        public ApplicationInitalizeHandler(IDatabaseUpdateProvider dbUpdate)
        {
            _dbUpdate = dbUpdate;
        }

        public DatabaseUpdateResult UpdateDatabase(string scriptFolder)
        {
            return _dbUpdate.PerformUpdates(scriptFolder);
        }
    }
}