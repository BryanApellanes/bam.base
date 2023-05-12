/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Net.Analytics
{
    public interface IDiffReport
    {
        DeletedDiffReportToken[] Deleted { get; }
        InsertedDiffReportToken[] Inserted { get; }
        DiffReportToken[] Tokens { get; set; }

        void Save(string filePath);
    }
}