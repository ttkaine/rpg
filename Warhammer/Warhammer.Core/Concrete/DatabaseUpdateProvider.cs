using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class DatabaseUpdateProvider : IDatabaseUpdateProvider, IDisposable
    {
        private readonly WarhammerDataEntities _entities = new WarhammerDataEntities();

        private DatabaseUpdateResult _result = new DatabaseUpdateResult
        {
            DebugMessages = new Collection<string>(),
            ErrorMessages = new Collection<string>(),
            Versions = new Collection<ChangeLog>(),
            Successful = false
        };

        public DatabaseUpdateResult PerformUpdates(string scriptFolder)
        {
            return PerformUpdates(scriptFolder, string.Empty);
        }

        public DatabaseUpdateResult PerformUpdates(string scriptFolder, string backupPath)
        {

            _result.DebugMessages.Add("DB backup path = " + backupPath);
            _result.DebugMessages.Add("DB script folder = " + scriptFolder);
            try
            {
                Dictionary<int, string> commands = new Dictionary<int, string>();

                if (Directory.Exists(scriptFolder))
                {
                    _result.DebugMessages.Add("Folder Found");
                    DirectoryInfo directory = new DirectoryInfo(scriptFolder);
                    _result.DebugMessages.Add(string.Format("Found {0} files", directory.GetFiles("*.sql").Count()));
                    foreach (FileInfo fileInfo in directory.GetFiles("*.sql"))
                    {
                        int fileId;
                        if (int.TryParse(fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.')), out fileId))
                        {
                            if (!commands.ContainsKey(fileId))
                            {
                                commands.Add(fileId, File.ReadAllText(fileInfo.FullName));
                                _result.DebugMessages.Add(string.Format("Adding file ID: {0}", fileId));
                            }
                        }
                    }

                    List<string> orderedCommands = commands.OrderBy(c => c.Key).Select(c => c.Value).ToList();

                    if (orderedCommands.Any())
                    {
                        _result.DebugMessages.Add(string.Format("Database is {0}", _entities.Database.Connection.Database));
                        //var runScripts = BackupDatabase(_entities.Database.Connection.Database, backupPath);
                        //_result.DebugMessages.Add(string.Format("Database backup returned {0}", runScripts));

                        var runCount = 0;
                        //if (runScripts)
                        //{
                        //    _result.DebugMessages.Add("Database backed up");
                        foreach (string orderedCommand in orderedCommands)
                        {
                            _entities.Database.ExecuteSqlCommand(orderedCommand);
                            _entities.SaveChanges();
                            runCount += 1;
                        }
                        // }
                        _result.DebugMessages.Add(string.Format("Ran {0} files", runCount));

                    }
                    List<ChangeLog> versionInfo = _entities.ChangeLogs.OrderByDescending(v => v.DateTime).ToList();

                    _result.Versions = versionInfo;

                    _result.DebugMessages.Add("Validate Database Schema vs Entity Model");
                    DatabaseValidationResult validateResult = ValidateSchema();

                    foreach (string error in validateResult.Errors)
                    {
                        _result.ErrorMessages.Add(error);
                    }

                    foreach (string warning in validateResult.Warnings)
                    {
                        _result.DebugMessages.Add(warning);
                    }


                    _result.Successful = validateResult.IsValid;
                }
            }
            catch (Exception ex)
            {
                _result.Successful = false;
                _result.Versions = null;

                Exception exception = ex;
                while (exception != null)
                {
                    _result.ErrorMessages.Add(exception.Message);
                    exception = exception.InnerException;
                }
            }
            return _result;
        }


        public struct DatabaseValidationResult
        {
            public bool IsValid
            {
                get { return !Errors.Any(); }
            }

            public List<string> Errors { get; set; }
            public List<string> Warnings { get; set; }
        }

        public DatabaseValidationResult ValidateSchema()
        {
            DatabaseValidationResult result = new DatabaseValidationResult { Errors = new List<string>(), Warnings = new List<string>() };

            return result; //#### todo fix validation for inheritance - see Tenjin?
            var oc = ((IObjectContextAdapter)_entities).ObjectContext;

            var items = oc.MetadataWorkspace.GetItems(DataSpace.CSpace).OfType<EntityType>();

            _entities.Database.Connection.Open();




            foreach (var entityType in items)
            {
                String[] tableRestrictions = new String[4];
                tableRestrictions[0] = _entities.Database.Connection.Database;
                tableRestrictions[2] = entityType.Name;
                DataTable schemaData = _entities.Database.Connection.GetSchema("Columns", tableRestrictions);

                if (schemaData.Rows.Count == 0)
                {
                    tableRestrictions[2] = entityType.Name + "s";
                    schemaData = _entities.Database.Connection.GetSchema("Columns", tableRestrictions);
                    if (schemaData.Rows.Count > 0)
                    {
                        result.Warnings.Add(string.Format("Table for entity {0} should not be plural in the database", entityType.Name));
                    }
                }

                List<string> columnNames = (from DataRow dataRow in schemaData.Rows select dataRow.ItemArray[3].ToString()).ToList();

                List<string> entityProps = entityType.Properties.Select(p => p.Name).ToList();
                FindMissingColumns(entityProps, columnNames, 1, result, entityType.Name);
            }
            return result;
        }

        private void FindMissingColumns(List<string> entityProps, List<string> columnNames, int tolerance, DatabaseValidationResult result, string tableName)
        {
            List<string> missing = entityProps.Where(p => !columnNames.Any(c => p.StartsWith(c) && Math.Abs(c.Length - p.Length) <= tolerance)).ToList();

            foreach (string missingProp in missing)
            {
                if (missingProp.Contains("_"))
                {
                    string amendedProp = missingProp.Replace('_', ' ');
                    if (columnNames.Contains(amendedProp))
                    {
                        result.Warnings.Add(string.Format("Columns should not contain spaces: '{1}' in {0} should be {2}", tableName, amendedProp, amendedProp.Replace(" ", "")));
                    }
                    else
                    {
                        result.Errors.Add(string.Format("Missing column for Entity {0}. Expected a column called {1}", tableName, missingProp));
                    }
                }
                else
                {
                    result.Errors.Add(string.Format("Missing column for Entity {0}. Expected a column called {1}", tableName, missingProp));
                }
            }

            List<string> additional = columnNames.Where(p => !entityProps.Any(c => p.StartsWith(c) && Math.Abs(c.Length - p.Length) <= tolerance)).ToList();
            foreach (string extraColumn in additional)
            {
                result.Warnings.Add(string.Format("Database contains column '{1}' in {0} which doesn't appear in the entity model", tableName, extraColumn));
            }
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
