﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public interface IExceptionReporter
    {
        void ReportException(string message, Exception exception);
        void ReportException(Exception exception);
    }
}
