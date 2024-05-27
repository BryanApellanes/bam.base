/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using Bam.Data;

namespace Bam.Data
{
    public interface IParameterBuilder
    {
        DbParameter BuildParameter(string name, object value);
        DbParameter[] GetParameters(IHasFilters filter);        
    }
}
