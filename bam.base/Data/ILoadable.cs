/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
	public interface ILoadable: ICommittable
	{
		void Load();
		void Load(IDatabase database);
	}
}
