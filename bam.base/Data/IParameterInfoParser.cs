/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public interface IParameterInfoParser: IHasParameterInfos, IHasFilters
    {
        string Parse();
        string Parse(int? number);
    }
}
