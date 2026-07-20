namespace Bam.Data
{
    /// <summary>
    /// Declares a database index on a Dao. On a property, declares a single-column index over
    /// that property's column (<see cref="Order"/> sets the direction; <see cref="ColumnNames"/>
    /// is ignored). On a class, declares a composite index over <see cref="ColumnNames"/>
    /// (per-column directions via <see cref="ColumnOrders"/>). Schema writers render the
    /// declaration as provider-specific <c>CREATE INDEX</c> DDL; providers that cannot honor
    /// <see cref="AccessMethod"/>, <see cref="OperatorClass"/>, or <see cref="StorageParameters"/>
    /// fail fast with <see cref="NotSupportedException"/> at DDL-build time. Note that DDL is
    /// rendered without an existence guard on providers whose dialect lacks one (matching
    /// <c>CREATE TABLE</c>), so index DDL is exactly as re-runnable as table DDL.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        /// <summary>
        /// Declares a property-level (single-column) index on the decorated property's column.
        /// </summary>
        public IndexAttribute()
        {
            this.ColumnNames = Array.Empty<string>();
        }

        /// <summary>
        /// Declares a class-level (composite) index over the specified column names, in order.
        /// </summary>
        /// <param name="columnNames">The database column names the index covers, in index order.</param>
        public IndexAttribute(params string[] columnNames)
        {
            this.ColumnNames = columnNames ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets or sets the index name. When not set, the schema writer derives
        /// <c>ix_{table}_{column}[_{column}...]</c>.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets whether the index is unique. Defaults to false.
        /// </summary>
        public bool Unique { get; set; }

        /// <summary>
        /// Gets the column names a class-level index covers, in index order. Empty for
        /// property-level declarations, where the column is inferred from the property's
        /// <see cref="ColumnAttribute"/>.
        /// </summary>
        public string[] ColumnNames { get; }

        /// <summary>
        /// Gets or sets the sort direction for a property-level index. Ignored by class-level
        /// declarations, which use <see cref="ColumnOrders"/>. Defaults to
        /// <see cref="SortOrder.Unspecified"/> (provider-default ascending).
        /// </summary>
        public SortOrder Order { get; set; } = SortOrder.Unspecified;

        /// <summary>
        /// Gets or sets per-column sort directions for a class-level index, parallel to
        /// <see cref="ColumnNames"/>. When null, every column uses
        /// <see cref="SortOrder.Unspecified"/>.
        /// </summary>
        public SortOrder[]? ColumnOrders { get; set; }

        /// <summary>
        /// Gets or sets the index access method (e.g. <c>ivfflat</c>, <c>hnsw</c>, <c>gin</c>),
        /// or null for the provider default (btree). Providers without access-method support
        /// fail fast when set.
        /// </summary>
        public virtual string? AccessMethod { get; set; }

        /// <summary>
        /// Gets or sets the operator class applied to the indexed column(s) (e.g.
        /// <c>vector_cosine_ops</c>), or null for the provider default. Providers without
        /// operator-class support fail fast when set.
        /// </summary>
        public virtual string? OperatorClass { get; set; }

        /// <summary>
        /// Gets or sets storage parameters rendered as a <c>WITH (...)</c> clause (e.g.
        /// <c>lists = 100</c>), or null for none. Providers without storage-parameter support
        /// fail fast when set.
        /// </summary>
        public virtual string? StorageParameters { get; set; }

        /// <summary>
        /// Resolves this declaration into a provider-neutral <see cref="IndexDefinition"/>,
        /// deriving the index name when <see cref="Name"/> is not set.
        /// </summary>
        /// <param name="tableName">The table the index is created on.</param>
        /// <param name="inferredColumnName">
        /// The column name inferred from the decorated property's <see cref="ColumnAttribute"/>
        /// for property-level declarations, or null for class-level declarations.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a class-level declaration names no columns, or when
        /// <see cref="ColumnOrders"/> is set but its length does not match
        /// <see cref="ColumnNames"/>.
        /// </exception>
        public IndexDefinition GetIndexDefinition(string tableName, string? inferredColumnName = null)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }
            List<IndexColumn> columns = new List<IndexColumn>();
            if (!string.IsNullOrEmpty(inferredColumnName))
            {
                columns.Add(new IndexColumn(inferredColumnName, Order));
            }
            else
            {
                if (ColumnNames.Length == 0)
                {
                    throw new InvalidOperationException($"A class-level index on table {tableName} must name at least one column.");
                }
                if (ColumnOrders != null && ColumnOrders.Length != ColumnNames.Length)
                {
                    throw new InvalidOperationException($"ColumnOrders length ({ColumnOrders.Length}) must match ColumnNames length ({ColumnNames.Length}) for the index on table {tableName}.");
                }
                for (int i = 0; i < ColumnNames.Length; i++)
                {
                    SortOrder order = ColumnOrders != null ? ColumnOrders[i] : SortOrder.Unspecified;
                    columns.Add(new IndexColumn(ColumnNames[i], order));
                }
            }
            string name = Name ?? $"ix_{tableName}_{string.Join("_", columns.Select(column => column.ColumnName))}";
            return new IndexDefinition(name, tableName, columns, Unique, AccessMethod, OperatorClass, StorageParameters);
        }
    }
}
