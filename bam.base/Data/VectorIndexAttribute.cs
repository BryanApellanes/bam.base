namespace Bam.Data
{
    /// <summary>
    /// Declares a vector similarity index on a property that also carries a <see cref="VectorColumnAttribute"/>.
    /// A specialization of <see cref="IndexAttribute"/> whose access method, operator class, and storage
    /// parameters are derived from <see cref="Method"/>, <see cref="Distance"/>, and <see cref="Lists"/>.
    /// The schema writer renders provider-specific index DDL (pgvector:
    /// <c>CREATE INDEX ... USING ivfflat (col vector_cosine_ops) WITH (lists = N)</c>).
    /// Note: ivfflat recall depends on the data present when the index is created — create the index
    /// after loading representative data, or reindex after bulk loads. Approximate vector indexes skip
    /// rows whose vector is null; pair similarity queries with a not-null filter on the column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class VectorIndexAttribute : IndexAttribute
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
        /// Gets the pgvector access method name derived from <see cref="Method"/>
        /// (<c>ivfflat</c> or <c>hnsw</c>). Setting is not supported — set <see cref="Method"/> instead.
        /// </summary>
        public override string? AccessMethod
        {
            get
            {
                return Method == VectorIndexMethod.Hnsw ? "hnsw" : "ivfflat";
            }
            set
            {
                throw new InvalidOperationException($"{nameof(VectorIndexAttribute)} derives {nameof(AccessMethod)} from {nameof(Method)}; set {nameof(Method)} instead.");
            }
        }

        /// <summary>
        /// Gets the pgvector operator class derived from <see cref="Distance"/>
        /// (<c>vector_cosine_ops</c>, <c>vector_l2_ops</c>, or <c>vector_ip_ops</c>).
        /// Setting is not supported — set <see cref="Distance"/> instead.
        /// </summary>
        public override string? OperatorClass
        {
            get
            {
                switch (Distance)
                {
                    case VectorDistance.Euclidean:
                        return "vector_l2_ops";
                    case VectorDistance.InnerProduct:
                        return "vector_ip_ops";
                    case VectorDistance.Cosine:
                    default:
                        return "vector_cosine_ops";
                }
            }
            set
            {
                throw new InvalidOperationException($"{nameof(VectorIndexAttribute)} derives {nameof(OperatorClass)} from {nameof(Distance)}; set {nameof(Distance)} instead.");
            }
        }

        /// <summary>
        /// Gets the storage parameters derived from <see cref="Method"/> and <see cref="Lists"/>
        /// (<c>lists = N</c> for ivfflat, null for hnsw). Setting is not supported — set
        /// <see cref="Lists"/> instead.
        /// </summary>
        public override string? StorageParameters
        {
            get
            {
                return Method == VectorIndexMethod.IvfFlat ? $"lists = {Lists}" : null;
            }
            set
            {
                throw new InvalidOperationException($"{nameof(VectorIndexAttribute)} derives {nameof(StorageParameters)} from {nameof(Lists)}; set {nameof(Lists)} instead.");
            }
        }
    }
}
