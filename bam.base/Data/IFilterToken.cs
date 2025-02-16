/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public interface IFilterToken
    {
        string Operator { get; set; }
        string? ToString();
    }
}
