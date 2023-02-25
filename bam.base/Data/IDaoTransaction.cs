/*
	Copyright © Bryan Apellanes 2015  
*/
using System;

namespace Bam.Net.Data
{
    public interface IDaoTransaction
    {
        IDatabase Database { get; }

        event EventHandler Committed;
        event EventHandler Disposed;
        event EventHandler RolledBack;

        void Commit();
        void Dispose();
        void Rollback();
    }
}