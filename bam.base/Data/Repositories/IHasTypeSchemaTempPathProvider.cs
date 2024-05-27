using System;
using Bam.Data.Schema;

namespace Bam.Data.Repositories
{
    public interface IHasTypeSchemaTempPathProvider
    {
        Func<IDaoSchemaDefinition, ITypeSchema, string> TypeSchemaTempPathProvider { get; set; }
    }
}