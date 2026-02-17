/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class ForeignKeyAttribute: ColumnAttribute
    {
        public ForeignKeyAttribute()
            : base()
        {
            this.Suffix = string.Empty;
			this.AllowNull = true;
        }

        public string ForeignKeyName => $"FK_{Table}_{ReferencedTable}{Suffix}";

        public string ReferencedKey { get; set; } = null!;

        public string ReferencedTable { get; set; } = null!;

        public string Suffix { get; set; }
    }
}
