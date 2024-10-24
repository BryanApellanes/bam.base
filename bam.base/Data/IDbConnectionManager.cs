﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Data
{
    public interface IDbConnectionManager
    {
        IDatabase Database { get; set; }
        int MaxConnections { get; set; }
        int LifetimeMilliseconds { get; set; }
        bool BlockOnRelease { get; set; }
        StateChangeEventHandler StateChangeEventHandler { get; set; }
        DbConnection GetDbConnection();
        void ReleaseConnection(DbConnection dbConnection);
    }
}
