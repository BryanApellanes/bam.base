/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bam.Logging
{
    public delegate void LogEntryAddedListener(string applicationName, LogEvent logEvent);   
}
