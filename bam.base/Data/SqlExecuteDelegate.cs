/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bam.Data
{
    public delegate void SqlExecuteDelegate(ISqlStringBuilder sql, IDatabase db);    
}
