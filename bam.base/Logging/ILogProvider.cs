﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Logging
{
    public interface ILogProvider
    {
        LogReaderFactory LogReaderFactory { get; set; }
        ILogger GetLogger();
        ILogReader GetLogReader();
    }
}
