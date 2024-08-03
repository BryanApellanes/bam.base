/*
	Copyright © Bryan Apellanes 2015  
*/
using Bam.Incubation;
using Bam.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Bam.Data;

namespace Bam.Data
{
    public interface IDatabase
    {
        Func<ColumnAttribute, string> ColumnNameProvider { get; set; }
        ColumnNameListProvider ColumnNameListProvider { get; }
        IDbConnectionManager ConnectionManager { get; set; }
        string? ConnectionName { get; set; }
        string? ConnectionString { get; set; }
        int MaxConnections { get; set; }
        string Name { get; }
        string ParameterPrefix { get; set; }
        string[] SchemaNames { get; }
        bool SelectStar { get; set; }
        DependencyProvider ServiceProvider { get; set; }

        event EventHandler CommandException;
        event EventHandler CommandExecuted;
        event EventHandler ReaderException;
        event EventHandler ReaderExecuted;

        IDaoTransaction BeginTransaction();
        DbCommand CreateCommand();
        DbConnection CreateConnection();
        DbConnectionStringBuilder CreateConnectionStringBuilder();
        T CreateConnectionStringBuilder<T>() where T : DbConnectionStringBuilder, new();
        DbParameter CreateParameter(string name, object value);
        bool Equals(object obj);
        DbDataReader ExecuteReader(ISqlStringBuilder sqlStatement);
        DbDataReader ExecuteReader(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn);
        DbDataReader ExecuteReader(string sqlStatement, DbParameter[] dbParameters, DbConnection? conn = null);
        DbDataReader ExecuteReader(string sqlStatement, DbParameter[] dbParameters, out DbConnection conn);
        DbDataReader ExecuteReader(string sqlStatement, object dbParameters);
        DbDataReader ExecuteReader(string sqlStatement, object dbParameters, out DbConnection conn);
        IEnumerable<T> ExecuteReader<T>(ISqlStringBuilder sqlStatement, Action<DbDataReader>? onReaderExecuted = null) where T : class, new();
        IEnumerable<T> ExecuteReader<T>(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn, bool closeConnection = true, Action<DbDataReader>? onReaderExecuted = null) where T : class, new();
        IEnumerable<T> ExecuteReader<T>(string sqlStatement, DbParameter[] dbParameters, DbConnection? conn = null, bool closeConnection = true, Action<DbDataReader>? onReaderExecuted = null) where T : class, new();
        IEnumerable<T> ExecuteReader<T>(string sqlStatement, DbParameter[] dbParameters, out DbConnection conn, Action<DbDataReader>? onReaderExecuted = null) where T : class, new();
        IEnumerable<T> ExecuteReader<T>(string sqlStatement, object dbParameters, Action<DbDataReader>? onReaderExecuted = null) where T : class, new();
        IEnumerable<T> ExecuteReader<T>(string sqlStatement, params DbParameter[] dbParameters) where T : class, new();
        void ExecuteSql(ISqlStringBuilder builder);
        void ExecuteSql(ISqlStringBuilder builder, IParameterBuilder parameterBuilder);
        void ExecuteSql(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn, bool releaseConnection = true);
        void ExecuteSql(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn, Action<Exception> exceptionHandler, bool releaseConnection = true);
        void ExecuteSql(string sqlStatement, CommandType commandType, params DbParameter[] dbParameters);
        void ExecuteSql(string sqlStatement, object dbParameters);
        void ExecuteSql(string sqlStatement, params DbParameter[] dbParameters);
        void ExecuteSql<T>(ISqlStringBuilder builder) where T : IDao;
        void ExecuteStoredProcedure(string sprocName, params DbParameter[] dbParameters);
        Dictionary<EnumType, DaoType> FillEnumDictionary<EnumType, DaoType>(Dictionary<EnumType, DaoType> dictionary, string nameColumn)
            where DaoType : IDao, new()
            where EnumType : notnull;
        IEnumerable<DataRow> GetDataRowsFromReader(ISqlStringBuilder sqlStatement);
        IEnumerable<DataRow> GetDataRowsFromReader(ISqlStringBuilder sqlStatement, out DbConnection conn);
        IEnumerable<DataRow> GetDataRowsFromReader(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn);
        IEnumerable<DataRow> GetDataRowsFromReader(string sqlStatement, DbParameter[] dbParameters, DbConnection conn);
        IEnumerable<DataRow> GetDataRowsFromReader(string sqlStatement, DbParameter[] dbParameters, out DbConnection conn);
        DataSet GetDataSetFromSql(string sqlStatement, CommandType commandType, bool releaseConnection, DbConnection conn, DbTransaction tx, params DbParameter[] dbParamaters);
        DataSet GetDataSetFromSql(string sqlStatement, CommandType commandType, bool releaseConnection, DbConnection conn, params DbParameter[] dbParamaters);
        DataSet GetDataSetFromSql(string sqlStatement, CommandType commandType, bool releaseConnection, params DbParameter[] dbParamaters);
        DataSet GetDataSetFromSql(string sqlStatement, CommandType commandType, params DbParameter[] dbParamaters);
        DataSet GetDataSetFromSql<T>(string sqlStatement, CommandType commandType, bool releaseConnection, DbConnection conn, DbTransaction tx, params DbParameter[] dbParamaters);
        DataTable GetDataTable(ISqlStringBuilder sqlStringBuilder);
        DataTable GetDataTable(string sqlStatement, CommandType commandType, params DbParameter[] dbParameters);
        DataTable GetDataTable(string sqlStatement, Dictionary<string, object> parameters);
        DataTable GetDataTable(string sqlStatement, object dynamicParameters);
        DataTable GetDataTable(string sqlStatement, params DbParameter[] dbParameters);
        DataTable GetDataTableFromReader(ISqlStringBuilder sqlStatement);
        DataTable GetDataTableFromReader(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn, bool closeConnection = true);
        DataTable GetDataTableFromReader(string sqlStatement, DbParameter[] dbParameters, DbConnection conn = null);
        DataTable GetDataTableFromReader(string sqlStatement, DbParameter[] dbParameters, out DbConnection conn);
        DataTable GetDataTableFromReader(string sqlStatement, object dbParameters);
        DataTable GetDataTableFromReader(string sqlStatement, object dbParameters, out DbConnection conn);
        IDataTypeTranslator GetDataTypeTranslator();
        DbConnection GetDbConnection();
        DbParameter[] GetDbParameters(ISqlStringBuilder sqlStringBuilder);
        DataRow GetFirstRow(string sqlStatement, CommandType commandType, params DbParameter[] dbParameters);
        DataRow GetFirstRow(string sqlStatement, Dictionary<string, object> dbParameters);
        DataRow GetFirstRow(string sqlStatement, params DbParameter[] dbParameters);
        int GetHashCode();
        IHydrator GetHydrator();
        long? GetIdValue(IDao dao);
        long? GetIdValue<T>(DataRow row) where T : IDao, new();
        long? GetLongValue(string columnName, DataRow row);
        DbConnection GetOpenDbConnection();
        DbParameter[] GetParameters(ISqlStringBuilder sqlStringBuilder);
        IQuery<C, T> GetQuery<C, T>()
            where C : IQueryFilter, IFilterToken, new()
            where T : IDao, new();
        IQuery<C, T> GetQuery<C, T>(Delegate where)
            where C : IQueryFilter, IFilterToken, new()
            where T : IDao, new();
        IQuery<C, T> GetQuery<C, T>(Func<C, IQueryFilter<C>> where, IOrderBy<C> orderBy = null)
            where C : IQueryFilter, IFilterToken, new()
            where T : IDao, new();
        IQuery<C, T> GetQuery<C, T>(WhereDelegate<C> where, IOrderBy<C> orderBy = null)
            where C : IQueryFilter, IFilterToken, new()
            where T : IDao, new();
        IQuerySet GetQuerySet();
        ISchemaWriter GetSchemaWriter();
        T GetService<T>();
        ISqlStringBuilder GetSqlStringBuilder();
        void Hydrate(IDao dao);
        T New<T>() where T : IDao, new();
        IEnumerable<dynamic> Query(string sqlQuery, Dictionary<string, object> dictDbParameters, string typeName = null);
        IEnumerable<dynamic> Query(string sqlQuery, object dynamicDbParameters, string typeName = null);
        IEnumerable<T> Query<T>(string sqlQuery, Dictionary<string, object> dbParameters);
        IEnumerable<T> Query<T>(string sqlQuery, Func<DataRow, T> rowProcessor, params DbParameter[] dbParameters);
        IEnumerable<T> Query<T>(string sqlQuery, object dynamicDbParameters);
        IEnumerable<T> Query<T>(string sqlQuery, params DbParameter[] dbParameters);
        T QuerySingle<T>(ISqlStringBuilder sql);
        T QuerySingle<T>(string singleValueQuery, object dynamicParamters);
        T QuerySingle<T>(string singleValueQuery, params DbParameter[] dbParameters);
        IEnumerable<T> QuerySingleColumn<T>(string singleColumnQuery, object dynamicParameters);
        IEnumerable<T> QuerySingleColumn<T>(string singleColumnQuery, params DbParameter[] dbParameters);
        void ReleaseConnection(DbConnection conn);
        T Save<T>(T dao) where T : IDao, new();
        ISqlStringBuilder Sql();
        EnsureSchemaStatus TryEnsureSchema(Assembly assembly, ILogger logger = null);
        EnsureSchemaStatus TryEnsureSchema(Type type, ILogger logger = null);
        EnsureSchemaStatus TryEnsureSchema(Type type, bool force, out Exception ex, ILogger logger = null);
        EnsureSchemaStatus TryEnsureSchema(Type type, out Exception ex, ILogger logger = null);
        EnsureSchemaStatus TryEnsureSchema<T>(ILogger logger = null);
    }
}