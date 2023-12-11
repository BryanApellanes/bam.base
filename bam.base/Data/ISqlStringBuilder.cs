/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Bam.Net.Data
{
    public interface ISqlStringBuilder: IHasFilters
    {
        Func<string, string> ColumnNameFormatter { get; set; }
        //IEnumerable<IFilterToken> Filters { get; }
        string GoText { get; set; }
        int? NextNumber { get; set; }
        IParameterInfo[] Parameters { get; set; }
        bool SelectStar { get; set; }
        Func<string, string> TableNameFormatter { get; set; }

        event SqlExecuteDelegate Executed;

        ISqlStringBuilder And(AssignValue filter);
        ISqlStringBuilder And(dynamic parameters);
        ISqlStringBuilder And(IQueryFilter filter);
        ISqlStringBuilder And(string columnName, object value);
        void Comment(string comment);
        void CommentFormat(string format, params object[] args);
        ISqlStringBuilder Count(string tableName);
        ISqlStringBuilder Count<T>() where T : IDao, new();
        ISqlStringBuilder Delete(string tableName);
        void Execute(IDatabase db);
        IEnumerable<dynamic> ExecuteDynamicReader(IDatabase db);
        IEnumerable<T> ExecuteReader<T>(IDatabase db) where T : class, new();
        ISqlStringBuilder FormatInsert<T>(string tableName, params AssignValue[] values) where T : SetFormat, new();
        DataSet GetDataSet(IDatabase db, bool releaseConnection = true, DbConnection conn = null, DbTransaction tx = null);
        DataTable ExecuteGetDataTable(IDatabase db);
        DataTable GetDataTable(IDatabase db);
        ISqlStringBuilder Go();
        ISqlStringBuilder Id();
        ISqlStringBuilder Id(string idAs);
        ISqlStringBuilder Insert(IDao instance);
        ISqlStringBuilder Insert(string tableName, dynamic valueAssignments);
        ISqlStringBuilder Insert(string tableName, params AssignValue[] values);
        ISqlStringBuilder Insert<T>(T instance) where T : IDao, new();
        ISqlStringBuilder OrderBy(string columnName, SortOrder order = SortOrder.Ascending);
        ISqlStringBuilder OrderBy<C>(IOrderBy<C> orderBy) where C : IQueryFilter, IFilterToken, new();
        void Reset();
        ISqlStringBuilder Select(Type daoType);
        ISqlStringBuilder Select(Type daoType, params string[] columnNames);
        ISqlStringBuilder Select(string tableName, params string[] columnNames);
        ISqlStringBuilder Select<T>() where T : IDao, new();
        ISqlStringBuilder Select<T>(params string[] columns);
        ISqlStringBuilder SelectCount(string tableName);
        ISqlStringBuilder SelectCount<T>() where T : IDao, new();
        ISqlStringBuilder SelectTop(int topCount, string tableName, params string[] columnNames);
        ISqlStringBuilder SelectTop<T>(int topCount) where T : IDao, new();
        ISqlStringBuilder Top<T>(int topCount) where T : IDao, new();
        string ToString();
        bool TryExecute(IDatabase db);
        bool TryExecute(IDatabase db, out Exception ex);
        ISqlStringBuilder Update(IDao instance);
        ISqlStringBuilder Update(string tableName, Dictionary<string, object> valueAssignments);
        ISqlStringBuilder Update(string tableName, dynamic valueAssignments);
        ISqlStringBuilder Update(string tableName, params AssignValue[] values);
        ISqlStringBuilder Update<T>(T instance) where T : IDao;
        ISqlStringBuilder Where(AssignValue filter);
        ISqlStringBuilder Where(dynamic parameters);
        ISqlStringBuilder Where(IQueryFilter filter);
        ISqlStringBuilder Where(string columnName, object value);
        ISqlStringBuilder Where<C>(Func<C, IQueryFilter> where) where C : IQueryFilter, IFilterToken, new();
    }
}