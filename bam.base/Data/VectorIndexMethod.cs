namespace Bam.Data
{
    /// <summary>
    /// Names the index access method used for a vector index.
    /// </summary>
    public enum VectorIndexMethod
    {
        /// <summary>
        /// IVFFlat: faster to build, smaller memory footprint; recall depends on the data
        /// present at index-creation time (create after loading data, or reindex after bulk loads).
        /// </summary>
        IvfFlat,
        /// <summary>
        /// HNSW: better query recall on changing data at the cost of slower builds and more memory.
        /// </summary>
        Hnsw
    }
}
