/*
	Copyright © Bryan Apellanes 2015  
*/
using System;

namespace Bam.Net.Data.Schema
{
    public interface IDaoSchemaDefinition
    {
        string DbType { get; set; }
        string File { get; set; }
        IForeignKeyColumn[] ForeignKeys { get; set; }
        Exception LastException { get; }
        string Name { get; set; }
        ITable[] Tables { get; set; }
        IXrefTable[] Xrefs { get; set; }

        ISchemaManagerResult AddForeignKey(IForeignKeyColumn fk);
        ISchemaManagerResult AddTable(ITable table);
        ISchemaManagerResult AddXref(IXrefTable xref);
        ITable GetTable(string tableName);
        IXrefTable GetXref(string tableName);
        IDaoSchemaDefinition CombineWith(IDaoSchemaDefinition schemaDefinition);
        IXrefInfo[] LeftXrefsFor(string tableName);
        void RemoveTable(string tableName);
        void RemoveTable(ITable table);
        void RemoveXref(string name);
        void RemoveXref(IXrefTable xrefTable);
        IXrefInfo[] RightXrefsFor(string tableName);
        void Save();
        void Save(string filePath);
    }
}