/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
	public interface IHasConnectionStringResolver
	{
		IConnectionStringResolver? ConnectionStringResolver { get; set; }
	}
}
