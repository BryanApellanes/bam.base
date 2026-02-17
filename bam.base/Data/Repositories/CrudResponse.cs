namespace Bam.Data.Repositories
{
    public class CrudResponse
    {
        /// <summary>
        /// The connection name also called the
        /// schema name or context name
        /// </summary>
        public string CxName { get; set; } = null!;
        public bool Success { get; set; }
        public object Dao { get; set; } = null!; // json
        public string Message { get; set; } = null!;
    }
}
