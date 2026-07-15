namespace Bam.Data
{
    /// <summary>
    /// Names the distance semantics used for vector similarity ordering and vector index
    /// operator classes. Provider mappings (pgvector): Cosine = <c>&lt;=&gt;</c> / <c>vector_cosine_ops</c>,
    /// Euclidean = <c>&lt;-&gt;</c> / <c>vector_l2_ops</c>, InnerProduct = <c>&lt;#&gt;</c> / <c>vector_ip_ops</c>.
    /// </summary>
    public enum VectorDistance
    {
        /// <summary>
        /// Cosine distance.
        /// </summary>
        Cosine,
        /// <summary>
        /// Euclidean (L2) distance.
        /// </summary>
        Euclidean,
        /// <summary>
        /// Negative inner product.
        /// </summary>
        InnerProduct
    }
}
