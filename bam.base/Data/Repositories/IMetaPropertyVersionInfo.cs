/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Reflection;

namespace Bam.Net.Data.Repositories
{
    public interface IMetaPropertyVersionInfo
    {
        string Hash { get; }
        DateTime LastWrite { get; }
        string Name { get; }
        PropertyInfo PropertyInfo { get; }
        object Value { get; }
        int Version { get; }
    }
}