/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data.Repositories
{
    /// <summary>
    /// Defines an event named GenerateDaoAssemblySucceeded
    /// </summary>
	public interface IGeneratesDaoAssembly
	{
		event EventHandler GenerateDaoAssemblySucceeded;
	}
}
