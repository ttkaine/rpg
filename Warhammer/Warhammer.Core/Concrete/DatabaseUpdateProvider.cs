using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class DatabaseUpdateProvider : IDatabaseUpdateProvider, IDisposable
    {
        private readonly WarhammerDataEntities _entities = new WarhammerDataEntities();

        public bool PerformUpdates(string scriptFolder)
        {
            try
            {

                Dictionary<int, string> commands = new Dictionary<int, string>();

                if (Directory.Exists(scriptFolder))
                {
                    DirectoryInfo directory = new DirectoryInfo(scriptFolder);

                    foreach (FileInfo fileInfo in directory.GetFiles("*.sql"))
                    {
                        int fileId;
                        if (int.TryParse(fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.')), out fileId))
                        {
                            if (!commands.ContainsKey(fileId))
                            {
                                commands.Add(fileId, File.ReadAllText(fileInfo.FullName));
                            }
                        }
                        //if (!Debugger.IsAttached)
                        //{
                        //    File.Delete(fileInfo.FullName);
                        //}
                    }

                    List<string> orderedCommands = commands.OrderBy(c => c.Key).Select(c => c.Value).ToList();

                    foreach (string orderedCommand in orderedCommands)
                    {
                        _entities.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,
                            orderedCommand);
                    }
                    return true;
                }

            }
            catch (Exception ex)
            {
                return false; 
            }
            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_entities != null)
                {
                    _entities.Dispose();
                }
            }
        }
    }
}
