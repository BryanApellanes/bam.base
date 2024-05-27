/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
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
