using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Abstract
{
    public struct DatabaseUpdateResult
    {
        public bool Successful { get; set; }
        public ICollection<ChangeLog> Versions { get; set; }
        public ICollection<string> ErrorMessages { get; set; }
        public ICollection<string> DebugMessages { get; set; }
    }

    public interface IDatabaseUpdateProvider
    {
        DatabaseUpdateResult PerformUpdates(string scriptFolder);
        DatabaseUpdateResult PerformUpdates(string scriptFolder, string backupPath);
    }
}
