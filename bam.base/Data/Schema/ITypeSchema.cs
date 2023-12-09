/*
	Copyright © Bryan Apellanes 2015  
*/
using Bam.Data.Schema;
using System;
using System.Collections.Generic;

namespace Bam.Net.Data.Repositories
{
    public interface ITypeSchema
    {
        DefaultDataTypeBehaviors DefaultDataTypeBehavior { get; set; }
        HashSet<ITypeFk> ForeignKeys { get; set; }
        string Hash { get; }
        string Name { get; set; }
        HashSet<Type> Tables { get; set; }
        HashSet<ITypeSchemaWarning> Warnings { get; set; }
        HashSet<ITypeXref> Xrefs { get; set; }

        string ToString();
    }
}