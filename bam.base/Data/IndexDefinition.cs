namespace Bam.Data
{
    /// <summary>
    /// A provider-neutral, resolved description of one database index, ready for a schema
    /// writer to render as <c>CREATE INDEX</c> DDL. Produced from an <see cref="IndexAttribute"/>
    /// by <see cref="IndexAttribute.GetIndexDefinition(string, string?)"/>.
    /// </summary>
    public sealed class IndexDefinition
    {
        /// <summary>
        /// Creates an index definition.
        /// </summary>
        /// <param name="name">The index name.</param>
        /// <param name="tableName">The table the index is created on.</param>
        /// <param name="columns">The ordered columns the index covers.</param>
        /// <param name="unique">True to create a unique index.</param>
        /// <param name="accessMethod">The index access method (e.g. <c>ivfflat</c>), or null for the provider default.</param>
        /// <param name="operatorClass">The operator class applied to the indexed column(s), or null for the provider default.</param>
        /// <param name="storageParameters">Storage parameters rendered as a <c>WITH (...)</c> clause, or null for none.</param>
        public IndexDefinition(string name, string tableName, IReadOnlyList<IndexColumn> columns, bool unique = false, string? accessMethod = null, string? operatorClass = null, string? storageParameters = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }
            if (columns == null || columns.Count == 0)
            {
                throw new ArgumentException($"An index must cover at least one column: {name} on {tableName}.", nameof(columns));
            }
            this.Name = name;
            this.TableName = tableName;
            this.Columns = columns;
            this.Unique = unique;
            this.AccessMethod = accessMethod;
            this.OperatorClass = operatorClass;
            this.StorageParameters = storageParameters;
        }

        /// <summary>
        /// Gets the index name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the table the index is created on.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Gets the ordered columns the index covers.
        /// </summary>
        public IReadOnlyList<IndexColumn> Columns { get; }

        /// <summary>
        /// Gets whether the index is unique.
        /// </summary>
        public bool Unique { get; }

        /// <summary>
        /// Gets the index access method (e.g. <c>ivfflat</c>), or null for the provider default (btree).
        /// </summary>
        public string? AccessMethod { get; }

        /// <summary>
        /// Gets the operator class applied to the indexed column(s), or null for the provider default.
        /// </summary>
        public string? OperatorClass { get; }

        /// <summary>
        /// Gets the storage parameters rendered as a <c>WITH (...)</c> clause, or null for none.
        /// </summary>
        public string? StorageParameters { get; }

        /// <summary>
        /// Gets whether the definition carries provider-specific index options
        /// (<see cref="AccessMethod"/>, <see cref="OperatorClass"/>, or <see cref="StorageParameters"/>).
        /// Providers without support for these options fail fast when this is true.
        /// </summary>
        public bool HasAccessMethodOptions
        {
            get
            {
                return !string.IsNullOrEmpty(AccessMethod) || !string.IsNullOrEmpty(OperatorClass) || !string.IsNullOrEmpty(StorageParameters);
            }
        }
    }
}
