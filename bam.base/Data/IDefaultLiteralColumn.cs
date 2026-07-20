namespace Bam.Data
{
    /// <summary>
    /// Contract for column attributes that carry an optional database default literal, letting
    /// schema writers render a <c>DEFAULT</c> clause without knowing the concrete attribute type.
    /// </summary>
    public interface IDefaultLiteralColumn
    {
        /// <summary>
        /// Gets the database default literal exactly as it should appear after <c>DEFAULT</c>
        /// (e.g. <c>'[]'</c> or <c>'{}'</c>), or null when the column declares no default.
        /// </summary>
        string? DefaultLiteral { get; }

        /// <summary>
        /// Gets the rendered <c>DEFAULT</c> clause with a leading space (e.g. <c> DEFAULT '[]'</c>),
        /// or the empty string when the column declares no default.
        /// </summary>
        string GetDefaultClause();
    }
}
