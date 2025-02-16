/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class TableAttribute: Attribute
    {
        public TableAttribute() { }
        public TableAttribute(string tableName, string connectionName)
        {
            this.TableName = tableName;
            this.ConnectionName = connectionName;
        }

        public string TableName { get; set; }

        /// <summary>
        /// Logical name given to the schema that
        /// the table is part of.  
        /// </summary>
        public string ConnectionName { get; set; }
    }
}
