using System;
using Bam.Net.Data.Schema;

namespace Bam.Net.Data.Repositories
{
    public interface IHasTypeSchemaTempPathProvider
    {
        Func<ISchemaDefinition, ITypeSchema, string> TypeSchemaTempPathProvider { get; set; }
    }
}