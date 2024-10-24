/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.IO;
using System.Reflection;
using Bam.Data.Repositories;

namespace Bam.Data.Repositories
{
    /// <summary>
    /// The interface to implement for a class that persists
    /// objects to the filesystem in some form.
    /// </summary>
    public interface IObjectPersister: IObjectReader, IObjectWriter, IObjectQueryer
	{
        
	}
}
