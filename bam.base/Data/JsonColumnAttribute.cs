namespace Bam.Data
{
    /// <summary>
    /// Declares a JSON column. Sets <see cref="ColumnAttribute.DbDataType"/> to <c>jsonb</c>, which
    /// the PostgreSQL provider renders natively (values bind as jsonb-typed parameters); every other
    /// provider degrades to its nearest text type for storage and retrieval only — JSON operators
    /// and querying are not portable off PostgreSQL. An optional default literal (e.g. <c>"'[]'"</c>
    /// or <c>"'{}'"</c>) is rendered as a <c>DEFAULT</c> clause by the schema writers.
    /// </summary>
    public class JsonColumnAttribute : ColumnAttribute, IDefaultLiteralColumn
    {
        /// <summary>
        /// Declares a JSON column, optionally with a database default literal.
        /// </summary>
        /// <param name="defaultLiteral">
        /// The default literal exactly as it should appear after <c>DEFAULT</c> (e.g. <c>"'[]'"</c>),
        /// or null for no default.
        /// </param>
        public JsonColumnAttribute(string? defaultLiteral = null)
        {
            this.DbDataType = "jsonb";
            this.DefaultLiteral = defaultLiteral;
        }

        /// <summary>
        /// Gets the database default literal, or null when the column declares no default.
        /// </summary>
        public string? DefaultLiteral { get; }

        /// <summary>
        /// Gets the rendered <c>DEFAULT</c> clause with a leading space, or the empty string when
        /// the column declares no default.
        /// </summary>
        public string GetDefaultClause()
        {
            return string.IsNullOrEmpty(DefaultLiteral) ? string.Empty : $" DEFAULT {DefaultLiteral}";
        }
    }
}
