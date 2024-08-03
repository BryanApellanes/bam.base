/*
	Copyright © Bryan Apellanes 2015  
*/
using System;

namespace Bam.Data
{
    public interface IQuerySet: ISqlStringBuilder
    {
        IDatabase Database { get; set; }
        IQuerySetResults Results { get; }

        void Select<C, T>(Func<C, IQueryFilter> where)
            where C : IFilterToken, new()
            where T : IDao, new();
        void Select<C, T>(IQueryFilter filter)
            where C : IFilterToken, new()
            where T : IDao, new();
    }
}