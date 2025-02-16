/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public interface IQueryFilter<C>: IQueryFilter where C : IFilterToken, new()
    {
    }

    public interface IQueryFilter: IHasFilters, IHasParameterInfos
    {
        string Parse();
        string Parse(int? number);
    }
}
