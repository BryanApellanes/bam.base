/*
	Copyright © Bryan Apellanes 2015  
*/
using Bam.Net.Data.Schema;
using Bam.Net.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Bam.Net.Data.Repositories
{
    public interface IDaoRepository
    {
        string BaseNamespace { get; set; }
        Assembly DaoAssembly { get; set; }
        string DaoNamespace { get; set; }
        IDatabase Database { get; set; }
        bool KeepSource { get; set; }
        ISchemaDefinition SchemaDefinition { get; }
        string SchemaName { get; set; }
        ITypeSchema TypeSchema { get; }
        Func<ISchemaDefinition, ITypeSchema, string> TypeSchemaTempPathProvider { get; set; }
        bool WarningsAsErrors { get; set; }
        bool WrapByDefault { get; set; }

        event EventHandler GenerateDaoAssemblySucceeded;
        event EventHandler SchemaWarning;

        void AddReferenceAssemblies(params Assembly[] assemblies);
        void AddType(Type type);
        void BatchRetrieveAll(Type dtoOrPocoType, int batchSize, Action<IEnumerable<object>> processor);
        T Construct<T>() where T : IAuditRepoData, new();
        object ConstructWrapper(Type baseType);
        object Create(object toCreate);
        object Create(Type type, object toCreate);
        T Create<T>(T toCreate) where T : class, new();
        bool Delete(object toDelete);
        bool Delete(Type type, object toDelete);
        bool Delete<T>(T toDelete) where T : new();
        Assembly EnsureDaoAssemblyAndSchema(bool useExisting = true);
        T First<T>(IQueryFilter query) where T : new();
        IEnumerable<TChildType> ForeignKeyCollectionLoader<TChildType>(object poco) where TChildType : new();
        IEnumerable<TChildType> ForeignKeyCollectionLoader<TParentType, TChildType>(object poco) where TChildType : new();
        Assembly GenerateDaoAssembly(bool useExisting = true);
        Type GetBaseType(Type wrapperType);
        IDao GetDaoInstance(object baseInstance);
        Type GetDaoType(Type pocoType);
        object GetParentPropertyOfChild(object dtoChild, Type parentType);
        Type GetWrapperType(Type baseOrWrapperType);
        Type GetWrapperType<T>() where T : new();
        void Initialize();
        IEnumerable<object> Query(string propertyName, object value);
        IEnumerable<object> Query(Type type, Dictionary<string, object> queryParameters);
        IEnumerable<object> Query(Type type, Func<object, bool> predicate);
        IEnumerable<object> Query(Type pocoType, IQueryFilter query);
        IEnumerable<object> Query(Type pocoType, IQueryFilter query, bool wrap);
        IEnumerable<T> Query<T>(Dictionary<string, object> queryParameters) where T : class, new();
        IEnumerable<T> Query<T>(dynamic query) where T : class, new();
        IEnumerable<T> Query<T>(Expression<Func<T, bool>> expression) where T : class, new();
        IEnumerable<T> Query<T>(Func<T, bool> predicate) where T : class, new();
        IEnumerable<T> Query<T>(IQueryFilter query) where T : class, new();
        object Retrieve(Type objectType, long id);
        object Retrieve(Type objectType, string uuid);
        object Retrieve(Type objectType, ulong id);
        T Retrieve<T>(int id) where T : class, new();
        T Retrieve<T>(long id) where T : class, new();
        T Retrieve<T>(string uuid) where T : class, new();
        T Retrieve<T>(ulong id) where T : class, new();
        IEnumerable<object> RetrieveAll(Type dtoOrPocoType);
        IEnumerable<T> RetrieveAll<T>() where T : class, new();
        bool SetChildDaoCollectionValues(object poco, IDao daoInstance);
        string SetDaoNamespace(Type type);
        string SetDaoNamespace<T>();
        void SetParentProperties(object dtoInstance);
        bool SetXrefDaoCollectionValues(object poco, IDao daoInstance);
        void Subscribe(ILogger logger);
        object ToDto(object instance);
        IEnumerable Top(int count, Type pocoType, IQueryFilter query);
        IEnumerable Top(int count, Type pocoType, IQueryFilter query, string sortByColumn);
        IEnumerable Top(int count, Type pocoType, IQueryFilter query, string sortByColumn, SortOrder sortOrder);
        IEnumerable Top(int count, Type pocoType, IQueryFilter query, string sortByColumn, SortOrder sortOrder, bool wrap);
        IEnumerable<T> Top<T>(int count, IQueryFilter query) where T : new();
        IEnumerable<T> Top<T>(int count, IQueryFilter query, string sortByColumn, SortOrder sortOrder) where T : new();
        object Update(object toUpdate);
        object Update(Type type, object toUpdate);
        T Update<T>(T toUpdate) where T : new();
        IEnumerable<object> Wrap(Type baseType, IEnumerable daoInstances);
        object Wrap(Type baseType, object instance);
        IEnumerable<T> Wrap<T>(IEnumerable daoInstances) where T : new();
        T Wrap<T>(T baseInstance);
    }
}