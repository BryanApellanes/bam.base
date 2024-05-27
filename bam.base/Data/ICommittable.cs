/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bam.Data
{
    public interface ICommittable: IDeleteable
    {
        event ICommittableDelegate AfterCommit;
        void Commit(IDatabase db = null);
        void WriteCommit(ISqlStringBuilder sql, IDatabase db = null);
    }
}
