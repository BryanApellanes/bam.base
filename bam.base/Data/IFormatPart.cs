/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Net.Data
{
    public interface IFormatPart
    {
        Func<string, string> ColumnNameFormatter { get; set; }
        int? NextNumber { get; }
        IParameterInfo[] Parameters { get; set; }
        int? StartNumber { get; set; }

        void AddParameter(IParameterInfo parameter);
        string Parse();
    }
}