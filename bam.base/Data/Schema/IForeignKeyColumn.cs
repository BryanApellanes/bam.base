/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Net.Data.Schema
{
    public interface IForeignKeyColumn : IColumn
    {
        DataTypes DataType { get; set; }
        string ReferencedClass { get; set; }
        string ReferencedKey { get; set; }
        string ReferencedTable { get; set; }
        string ReferenceName { get; set; }
        string ReferenceNameSuffix { get; set; }
        string ReferencingClass { get; set; }
    }
}