/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Reflection;

namespace Bam.Data
{
    public interface ISchemaWriter
    {
        string AddForeignKeyColumnFormat { get; }
        string CreateTableFormat { get; }
        bool EnableDrop { get; set; }
        string KeyColumnFormat { get; }

        event SqlStringBuilderDelegate DropEnabled;

        void DropAllTables<T>() where T : IDao;
        void DropTable(Type daoType);
        string GetColumnDefinition(ColumnAttribute column);
        string GetKeyColumnDefinition(KeyColumnAttribute keyColumn);
        void WriteAddForeignKey(string tableName, string nameOfReference, string nameOfColumn, string referencedTable, string referencedKey);
        void WriteCreateTable(string tableName, string columnDefinitions, dynamic[]? fks = null);
        ISchemaWriter WriteDropTable(string tableName);
        bool WriteSchemaScript(Assembly assembly);
        bool WriteSchemaScript(Type type);
        bool WriteSchemaScript<T>() where T : IDao;
    }
}