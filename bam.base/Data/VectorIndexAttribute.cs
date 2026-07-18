namespace Bam.Data
{
    /// <summary>
    /// Declares a vector similarity index on a property that also carries a <see cref="VectorColumnAttribute"/>.
    /// The schema writer renders provider-specific index DDL (pgvector:
    /// <c>CREATE INDEX ... USING ivfflat (col vector_cosine_ops) WITH (lists = N)</c>).
    /// Note: ivfflat recall depends on the data present when the index is created — create the index
    /// after loading representative data, or reindex after bulk loads. Approximate vector indexes skip
    /// rows whose vector is null; pair similarity queries with a not-null filter on the column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class VectorIndexAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the index access method. Defaults to <see cref="VectorIndexMethod.IvfFlat"/>.
        /// </summary>
        public VectorIndexMethod Method { get; set; } = VectorIndexMethod.IvfFlat;

        /// <summary>
        /// Gets or sets the distance semantics the index optimizes for, which selects the operator class.
        /// Defaults to <see cref="VectorDistance.Cosine"/>.
        /// </summary>
        public VectorDistance Distance { get; set; } = VectorDistance.Cosine;

        /// <summary>
        /// Gets or sets the ivfflat list count (<c>WITH (lists = N)</c>). Ignored by other methods.
        /// Defaults to 100.
        /// </summary>
        public int Lists { get; set; } = 100;

        /// <summary>
        /// Gets or sets the index name. When not set, the schema writer derives <c>ix_{table}_{column}</c>.
        /// </summary>
        public string? Name { get; set; }
    }
}
