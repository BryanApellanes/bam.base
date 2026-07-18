namespace Bam.Data
{
    /// <summary>
    /// Declares a uuid-array column. Sets <see cref="ColumnAttribute.DbDataType"/> to <c>uuid[]</c>,
    /// which only the PostgreSQL provider supports (values bind natively as <c>Guid[]</c>); every
    /// other provider fails fast at DDL time, mirroring vector columns. The default literal is
    /// <c>'{}'</c> (an empty array) unless overridden, rendered as a <c>DEFAULT</c> clause by the
    /// schema writers.
    /// </summary>
    public class UuidArrayColumnAttribute : ColumnAttribute, IDefaultLiteralColumn
    {
        /// <summary>
        /// Declares a uuid-array column, defaulting to an empty-array database default.
        /// </summary>
        /// <param name="defaultLiteral">
        /// The default literal exactly as it should appear after <c>DEFAULT</c>; defaults to
        /// <c>"'{}'"</c> (empty array). Pass null for no default.
        /// </param>
        public UuidArrayColumnAttribute(string? defaultLiteral = "'{}'")
        {
            this.DbDataType = "uuid[]";
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
