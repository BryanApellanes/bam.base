using System;
using System.Collections.Generic;

namespace Bam.Data.Repositories
{
    /// <summary>
    /// When implemented by a derived class, enables querying of persisted types using
    /// QueryFilter objects.  See also, <see cref="QueryFilter"/>
    /// </summary>
    public interface IQueryFilterable
    {
        IEnumerable<object> Query(Type pocoType, IQueryFilter query);
        IEnumerable<T> Query<T>(IQueryFilter query) where T : class, new();
    }
}