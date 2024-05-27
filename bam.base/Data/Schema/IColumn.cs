/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Data.Schema
{
    public interface IColumn
    {
        bool AllowNull { get; set; }
        DataTypes DataType { get; set; }
        string DbDataType { get; set; }
        bool Key { get; set; }
        string MaxLength { get; set; }
        string Name { get; set; }
        string NativeType { get; }
        string PropertyName { get; set; }
        string TableClassName { get; set; }
        string TableName { get; set; }
    }
}