/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Net.Data.Schema
{
    public interface IXrefTable : ITable
    {
        string Left { get; set; }
        string Right { get; set; }
    }
}