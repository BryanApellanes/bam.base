/*
	Copyright © Bryan Apellanes 2015  
*/
using System.IO;

namespace Bam.Net.Data.Schema
{
    public interface IDaoSchemaManagerResult
    {
        FileInfo DaoAssembly { get; set; }
        string ExceptionMessage { get; set; }
        string Message { get; set; }
        string Namespace { get; set; }
        string SchemaName { get; set; }
        string StackTrace { get; set; }
        bool Success { get; set; }
    }
}