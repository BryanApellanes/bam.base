/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data;

namespace Bam.Data
{
    public interface IHasDataRow
    {
        IDatabase Database { get; set; }
        DataRow DataRow { get; set; }
    }
}
