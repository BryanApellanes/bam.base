namespace Bam.Data
{
    /// <summary>
    /// Declares a vector (embedding) column of a fixed dimension. Sets <see cref="ColumnAttribute.DbDataType"/>
    /// to <c>vector</c> and mirrors <see cref="Dimensions"/> into <see cref="ColumnAttribute.MaxLength"/>,
    /// which the schema writer renders as the type's parameter (e.g. <c>vector(1536)</c>).
    /// <see cref="Dimensions"/> is the authoritative, typed surface; <c>MaxLength</c> is an implementation
    /// detail of DDL rendering. Vector columns allow null by default because approximate vector indexes
    /// (e.g. pgvector's ivfflat) skip null vectors rather than failing on them.
    /// Currently supported by the PostgreSQL (pgvector) provider only; other providers fail fast at DDL time.
    /// </summary>
    public class VectorColumnAttribute : ColumnAttribute
    {
        /// <summary>
        /// Declares a vector column of the specified dimension.
        /// </summary>
        /// <param name="dimensions">The number of components each value in this column must have.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dimensions"/> is less than 1.</exception>
        public VectorColumnAttribute(int dimensions)
        {
            if (dimensions < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(dimensions), "A vector column requires a dimension of at least 1.");
            }
            this.Dimensions = dimensions;
            this.DbDataType = "vector";
            this.MaxLength = dimensions.ToString();
            this.AllowNull = true;
        }

        /// <summary>
        /// Gets the number of components each value in this column must have.
        /// </summary>
        public int Dimensions { get; }
    }
}
