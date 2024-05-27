/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Data;

namespace Bam.Data
{
    public interface IQuery<C, T> 
        where C : IQueryFilter, IFilterToken, new()
        where T: IDao, new()
    {
        Func<ColumnAttribute, string> ColumnNameProvider { get; set; }
        IDatabase Database { get; set; }
        DataTable GetDataTable();
        DataTable GetDataTable(IDatabase database);
        ISqlStringBuilder ToSqlStringBuilder(IDatabase db);
        DataTable Where(Func<C, IQueryFilter<C>> where, IOrderBy<C> orderBy = null, IDatabase db = null);
        //DataTable Where(QiQuery query, IDatabase db = null);
        DataTable Where(WhereDelegate<C> where, IDatabase db = null);
    }
}