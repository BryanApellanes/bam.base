/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Data
{
    public interface IOrderBy<C> where C : IQueryFilter, IFilterToken, new()
    {
        C Column { get; }
        SortOrder SortOrder { get; }
    }
}