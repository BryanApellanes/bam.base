/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public interface IHasFilters
    {
        IEnumerable<IFilterToken> Filters { get; }
    }
}
