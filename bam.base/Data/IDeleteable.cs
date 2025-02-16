/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public interface IDeleteable
    {
        void Delete(IDatabase? db = null);
        void WriteDelete(ISqlStringBuilder sql);
    }
}
