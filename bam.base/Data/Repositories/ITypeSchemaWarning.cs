using System;

namespace Bam.Net.Data.Repositories
{
    public interface ITypeSchemaWarning
    {
        Type ForeignKeyType { get; set; }
        Type ParentType { get; set; }
        TypeSchemaWarnings Warning { get; set; }

        TypeSchemaWarningEventArgs ToEventArgs();
    }
}