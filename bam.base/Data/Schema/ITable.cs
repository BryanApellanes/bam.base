/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Net.Data.Schema
{
    public interface ITable
    {
        IColumn this[string columnName] { get; }

        string ClassName { get; set; }
        IColumn[] Columns { get; set; }
        string ConnectionName { get; set; }
        IForeignKeyColumn[] ForeignKeys { get; set; }
        IColumn Key { get; }
        string Name { get; set; }
        IForeignKeyColumn[] ReferencingForeignKeys { get; set; }

        void AddColumn(IColumn column);
        void AddColumn(string columnName, DataTypes type, bool allowNull = true);
        string GetPropertyName(string columnName);
        bool HasColumn(string columnName);
        bool HasColumn(string columnName, out IColumn column);
        void RemoveColumn(IColumn column);
        void RemoveColumn(string columnName);
        void SetForeignKeyColumn(string columnName, string referencedColumn, string referencedTable);
        void SetKeyColumn(string columnName);
        void SetPropertyName(string columnName, string propertyName);
    }
}