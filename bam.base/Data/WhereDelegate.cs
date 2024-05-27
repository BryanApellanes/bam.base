/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Data;
using System.Data;
using System.Data.Common;

namespace Bam.Data
{
    public delegate IQueryFilter WhereDelegate<C>(C where) where C : IQueryFilter, IFilterToken, new();
}
