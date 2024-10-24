﻿/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Data.Schema
{
    public interface IForeignKeyColumn : IColumn
    {
        string ReferencedClass { get; set; }
        string ReferencedKey { get; set; }
        string ReferencedTable { get; set; }
        string ReferenceName { get; set; }
        string ReferenceNameSuffix { get; set; }
        string ReferencingClass { get; set; }
    }
}