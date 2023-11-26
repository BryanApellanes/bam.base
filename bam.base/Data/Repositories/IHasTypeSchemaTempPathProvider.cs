using System;
using Bam.Net.Data.Schema;

namespace Bam.Net.Data.Repositories
{
    public interface IHasTypeSchemaTempPathProvider
    {
        Func<IDaoSchemaDefinition, ITypeSchema, string> TypeSchemaTempPathProvider { get; set; }
    }
}