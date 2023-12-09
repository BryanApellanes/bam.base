using System;

namespace Bam.Data.Schema
{
    public interface ITypeSchemaWarning
    {
        Type ForeignKeyType { get; set; }
        Type ParentType { get; set; }
        TypeSchemaWarnings Warning { get; set; }

        TypeSchemaWarningEventArgs ToEventArgs();
    }
}