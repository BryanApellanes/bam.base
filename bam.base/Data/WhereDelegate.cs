/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public delegate IQueryFilter WhereDelegate<C>(C where) where C : IQueryFilter, IFilterToken, new();
}
