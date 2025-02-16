/*
	Copyright © Bryan Apellanes 2015  
*/

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
