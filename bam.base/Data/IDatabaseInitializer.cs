/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using Bam.Incubation;

namespace Bam.Data
{
    public interface IDatabaseInitializer
    {
        DatabaseInitializationResult Initialize(string connectionName);
        void Ignore(params Type[] types);
        void Ignore(params string[] connectionNames);
    }
}
