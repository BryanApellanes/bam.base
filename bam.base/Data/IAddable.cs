/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
	public interface IAddable 
	{
		void Add(object value);
        void Clear(IDatabase? db = null);
	}
}
