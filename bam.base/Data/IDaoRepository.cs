/*
	Copyright © Bryan Apellanes 2015  
*/
using Bam.Data.Schema;
using Bam.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Bam.Data.Repositories
{
    public interface IDaoRepository : IRepository
    {
        string BaseNamespace { get; set; }
        Assembly DaoAssembly { get; set; }
        string DaoNamespace { get; set; }
        IDatabase Database { get; set; }
        bool KeepSource { get; set; }
        IDaoSchemaDefinition SchemaDefinition { get; }
        string SchemaName { get; set; }
        ITypeSchema TypeSchema { get; }
        Func<IDaoSchemaDefinition, ITypeSchema, string> TypeSchemaTempPathProvider { get; set; }
        bool WarningsAsErrors { get; set; }
        bool WrapByDefault { get; set; }

        event EventHandler GenerateDaoAssemblySucceeded;
        event EventHandler SchemaWarning;

        void AddReferenceAssemblies(params Assembly[] assemblies);
        T Construct<T>() where T : IAuditRepoData, new();
        object ConstructWrapper(Type baseType);

        Assembly EnsureDaoAssemblyAndSchema(bool useExisting = true);
        T First<T>(IQueryFilter query) where T : new();
        IEnumerable<TChildType> ForeignKeyCollectionLoader<TParentType, TChildType>(object poco) where TChildType : new();
        Assembly? GenerateDaoAssembly(bool useExisting = true);
        Type GetBaseType(Type wrapperType);
        IDao GetDaoInstance(object baseInstance);
        Type GetDaoType(Type pocoType);
        object? GetParentPropertyOfChild(object dtoChild, Type parentType);
        Type GetWrapperType(Type baseOrWrapperType);
        Type GetWrapperType<T>() where T : new();
        void Initialize();
        IEnumerable<object> Query(Type pocoType, IQueryFilter query, bool wrap);
        IEnumerable<T> Query<T>(Expression<Func<T, bool>> expression) where T : class, new();
        bool SetDaoCollectionValues(object poco, IDao daoInstance);
        string SetDaoNamespace(Type type);
        string SetDaoNamespace<T>();
        void SetParentProperties(object dtoInstance);
        bool SetDaoXrefCollectionValues(object poco, IDao daoInstance);
        //void Subscribe(ILogger logger);
        object ToDto(object instance);
        IEnumerable Top(int count, Type pocoType, IQueryFilter query);
        IEnumerable Top(int count, Type pocoType, IQueryFilter query, string sortByColumn);
        IEnumerable Top(int count, Type pocoType, IQueryFilter query, string sortByColumn, SortOrder sortOrder);
        IEnumerable Top(int count, Type pocoType, IQueryFilter query, string sortByColumn, SortOrder sortOrder, bool wrap);
        IEnumerable<T> Top<T>(int count, IQueryFilter query) where T : new();
        IEnumerable<T> Top<T>(int count, IQueryFilter query, string sortByColumn, SortOrder sortOrder) where T : new();
        IEnumerable<object> Wrap(Type baseType, IEnumerable daoInstances);
        object Wrap(Type baseType, object instance);
        IEnumerable<T> Wrap<T>(IEnumerable daoInstances) where T : new();
        T Wrap<T>(T baseInstance);
    }
}