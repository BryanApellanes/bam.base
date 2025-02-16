/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public interface IParameterInfo: IFilterToken
    {
		Func<string, string> ColumnNameFormatter { get; set; }
		string ParameterPrefix { get; set; }
        string ColumnName { get; set; } 
        int? Number { get; set; }
        int? SetNumber(int? value);
        object? Value { get; set; }
    }
}
