/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Data.Schema
{
    public interface IXrefTable : ITable
    {
        string Left { get; set; }
        string Right { get; set; }
    }
}