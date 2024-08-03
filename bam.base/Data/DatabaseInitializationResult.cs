/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bam.Data
{
    public class DatabaseInitializationResult
    {
        public DatabaseInitializationResult()
        {
            this.Success = false;            
        }

        public DatabaseInitializationResult(IDatabase db, Exception? ex = null)
        {
            this.Database = db;
            this.Exception = ex;
            this.Success = ex == null;
        }

        public IDatabase? Database { get; private set; }
        public Exception? Exception { get; set; }
        public bool Success { get; set; }
        public IDatabaseInitializer? Initializer { get; set; }
    }
}
