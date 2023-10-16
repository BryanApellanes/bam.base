/*
	Copyright © Bryan Apellanes 2015  
*/
using Bam.Net.Incubation;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Bam.Net.Data
{
    public interface IDao
    {
        bool AutoDeleteChildren { get; set; }
        bool AutoHydrateChildrenOnDelete { get; set; }
        void HydrateChildren(IDatabase database = null);
        string[] Columns { get; }
        IDatabase Database { get; set; }
        DataRow DataRow { get; set; }
        string DefaultSortProperty { get; set; }
        bool ForceInsert { get; set; }
        bool ForceUpdate { get; set; }
        ulong? IdValue { get; set; }
        Action<IDao> Initializer { get; set; }
        bool IsNew { get; set; }
        string KeyColumnName { get; }
        object PrimaryKey { get; set; }
        DependencyProvider ServiceProvider { get; }
        bool HasNewValues { get; }
        Func<IDao, IQueryFilter> UniqueFilterProvider { get; set; }

        event ICommittableDelegate AfterCommit;
        event DaoDelegate AfterDelete;
        event DaoDelegate AfterWriteCommit;
        event DaoDelegate AfterWriteDelete;
        event DaoDelegate BeforeCommit;
        event DaoDelegate BeforeDelete;
        event DaoDelegate BeforeWriteCommit;
        event DaoDelegate BeforeWriteDelete;

        T Column<T>(string columnName, object value = null);
        object ColumnValue(string columnName, object value = null);
        T ColumnValue<T>(string columnName, object value = null);
        void Commit();
        void Commit(IDaoTransaction tx);
        void Commit(IDatabase db);
        void Commit(IDatabase db, bool commitChildren);
        int CompareTo(object obj);
        string ConnectionName();
        void Delete(IDatabase database = null);
        DataTypes GetDataType(string columnName);
        string GetDbDataType(string columnName);
        ulong? GetId();
        Type[] GetSchemaTypes();
        TypeCode GetTypeCode(DataTypes dataTypes);
        IQueryFilter GetUniqueFilter();
        AssignValue[] GetNewAssignValues();
        void Hydrate(IDatabase database = null);
        void Insert(IDatabase db = null);
        void OnInitialize();
        void PreLoadChildCollections();
        void ResetChildren();
        void Save();
        void Save(IDatabase db);
        Task SaveAsync(IDatabase db = null);
        void SetId(object value);
        void SetId(ulong? id);
        void SetUuid();
        void SetValue(string columnName, object value);
        string TableName();
        ulong? TryGetId(Action<Exception> exceptionHandler = null);
        void Undelete(IDatabase db = null);
        void Undo(IDatabase db = null);
        void Update(IDatabase db = null);
        bool ValidateRequiredProperties(out string[] messages);
        void WriteChildDeletes(ISqlStringBuilder sql);
        void WriteCommit(ISqlStringBuilder sqlStringBuilder);
        void WriteCommit(ISqlStringBuilder sqlStringBuilder, IDatabase db);
        void WriteDelete(ISqlStringBuilder sql);
        void WriteInsert(ISqlStringBuilder sqlStringBuilder);
        void WriteUpdate(ISqlStringBuilder sqlStringBuilder);
    }
}