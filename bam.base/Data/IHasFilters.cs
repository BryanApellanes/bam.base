/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;

namespace Bam.Data
{
    public interface IHasFilters
    {
        IEnumerable<IFilterToken> Filters { get; }
    }
}
