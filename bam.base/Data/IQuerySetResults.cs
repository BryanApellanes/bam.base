/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Data
{
    public interface IQuerySetResults
    {
        IHasDataTable this[int index] { get; }

        int Count { get; }
        IDatabase Database { get; set; }

        T As<T>(int index) where T : IHasDataTable, new();
        long ToCountResult(int index);
        T ToDao<T>(int index) where T : IDao, new();
    }
}