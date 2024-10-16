﻿using System;

namespace Bam.Data.Repositories
{
    public interface IRepoData
    {
        DateTime? Created { get; set; }
        string Cuid { get; set; }
        ulong Id { get; set; }
        string Uuid { get; set; }

        T EnsurePersisted<T>(IRepository repo) where T : class, new();
        T EnsureSingle<T>(IRepository repo, string modifiedBy, params string[] propertyNames) where T : class, new();
        bool GetIsPersisted();
        bool GetIsPersisted(out IRepository repo);
        T QueryFirstOrDefault<T>(IRepository repo, params string[] propertyNames) where T : class, new();
        IRepoData Save(IRepository repo);
    }
}