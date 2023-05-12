/*
	Copyright © Bryan Apellanes 2015  
*/
using System;

namespace Bam.Net.Data.Repositories
{
    public interface IAuditRepoData
    {
        string CreatedBy { get; set; }
        DateTime? Deleted { get; set; }
        DateTime? Modified { get; set; }
        string ModifiedBy { get; set; }

        T EnsureSingle<T>(IRepository repo, string modifiedBy, params string[] propertyNames) where T : class, new();
    }
}