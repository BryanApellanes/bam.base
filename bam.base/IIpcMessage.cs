/*
	Copyright © Bryan Apellanes 2015  
*/
using Bam.Logging;
using System;

namespace Bam
{
    public interface IIpcMessage : ILoggable
    {
        int AcquireLockRetryInterval { get; set; }
        string CurrentLockerId { get; set; }
        string CurrentLockerMachineName { get; set; }
        string LastExceptionMessage { get; set; }
        int LockTimeout { get; set; }
        Type MessageType { get; set; }
        string Name { get; set; }

        event EventHandler AcquireLockException;
        event EventHandler WaitingForLock;

        T Read<T>();
        bool Write(object data);
    }
}