/*
	Copyright © Bryan Apellanes 2015  
*/
using System;

namespace Bam.Net.Data
{
    public interface IQuerySet
    {
        IDatabase Database { get; set; }
        IQuerySetResults Results { get; }

        event SqlExecuteDelegate Executed;

        ISqlStringBuilder Count<T>() where T : IDao, new();
        void Execute(IDatabase db);
        ISqlStringBuilder Insert(IDao instance);
        ISqlStringBuilder Insert<T>(T instance) where T : IDao, new();
        void Select<C, T>(Func<C, IQueryFilter> where)
            where C : IFilterToken, new()
            where T : IDao, new();
        void Select<C, T>(IQueryFilter filter)
            where C : IFilterToken, new()
            where T : IDao, new();
        ISqlStringBuilder Select<T>() where T : IDao, new();
        ISqlStringBuilder Select<T>(params string[] columns);
        ISqlStringBuilder SelectCount<T>() where T : IDao, new();
        ISqlStringBuilder SelectTop<T>(int count) where T : IDao, new();
        ISqlStringBuilder Top<T>(int count) where T : IDao, new();
    }
}