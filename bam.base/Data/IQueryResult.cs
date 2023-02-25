/*
	Copyright © Bryan Apellanes 2015  
*/
using System.Data;

namespace Bam.Net.Data
{
    public interface IQueryResult
    {
        IDatabase Database { get; set; }
        DataRow DataRow { get; set; }
        DataTable DataTable { get; }

        T As<T>() where T : IHasDataTable, new();
        void SetDataTable(DataTable table);
    }
}