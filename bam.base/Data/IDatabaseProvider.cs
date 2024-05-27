using Bam.Logging;
using System;

namespace Bam.Data
{
    public interface IDatabaseProvider
    {
        ILogger Logger { get; set; }
        void SetDatabases(params object[] instances);

        IDatabase GetAppDatabase(IApplicationNameProvider appNameProvider, string databaseName);
        IDatabase GetSysDatabase(string databaseName);
        IDatabase GetAppDatabaseFor(IApplicationNameProvider appNameProvider, object instance);
        IDatabase GetSysDatabaseFor(object instance);
        IDatabase GetAppDatabaseFor(IApplicationNameProvider appNameProvider, Type objectType, string info = null);
        IDatabase GetSysDatabaseFor(Type objectType, string info = null);
    }
}