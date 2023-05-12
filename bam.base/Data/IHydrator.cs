using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Net.Data
{
    /// <summary>
    /// When implemented provides a Dao hydration mechanism.  See Hydrator.
    /// </summary>
    public interface IHydrator
    {
        bool TryHydrateChildren(IDao dao, IDatabase database = null);
        void HydrateChildren(IDao dao, IDatabase database = null);
    }
}
