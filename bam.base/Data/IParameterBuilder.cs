/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data.Common;

namespace Bam.Data
{
    public interface IParameterBuilder
    {
        DbParameter BuildParameter(string name, object value);
        DbParameter[] GetParameters(IHasFilters filter);        
    }
}
