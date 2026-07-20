namespace Bam.Data
{
    /// <summary>
    /// One column participating in a database index, with its sort direction.
    /// <see cref="SortOrder.Unspecified"/> renders no direction token, leaving the
    /// provider's default (ascending) in effect.
    /// </summary>
    public sealed class IndexColumn
    {
        /// <summary>
        /// Creates an index column.
        /// </summary>
        /// <param name="columnName">The database column name.</param>
        /// <param name="order">The sort direction; defaults to <see cref="SortOrder.Unspecified"/>.</param>
        public IndexColumn(string columnName, SortOrder order = SortOrder.Unspecified)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }
            if (columnName.Length == 0)
            {
                throw new ArgumentException("columnName is required.", nameof(columnName));
            }
            this.ColumnName = columnName;
            this.Order = order;
        }

        /// <summary>
        /// Gets the database column name.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Gets the sort direction.
        /// </summary>
        public SortOrder Order { get; }
    }
}
