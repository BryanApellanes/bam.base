using System;

namespace Bam.Data.Schema
{
    public class TypeSchemaWarningEventArgs : EventArgs
    {
        public TypeSchemaWarnings Warning { get; set; }
        public Type? ParentType { get; set; }
        public Type? ForeignKeyType { get; set; }
        public string ParentTypeName => ParentType?.Name;
        public string ForeignKeyTypeName => ForeignKeyType?.Name;
        public string[]? Namespaces { get; set; }

        public static TypeSchemaWarningEventArgs FromTypeSchemaWarning(ITypeSchemaWarning warning)
        {
            return new TypeSchemaWarningEventArgs()
            {
                ParentType = warning.ParentType,
                ForeignKeyType = warning.ForeignKeyType,
                Warning = warning.Warning
            };
        }
    }
}