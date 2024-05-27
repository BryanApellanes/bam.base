/*
	Copyright © Bryan Apellanes 2015  
*/
using System;

namespace Bam.Data
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