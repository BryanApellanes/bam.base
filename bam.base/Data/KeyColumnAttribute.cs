/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class KeyColumnAttribute: ColumnAttribute
    {
        public KeyColumnAttribute()
            : base()
        { }

        public override bool AllowNull
        {
            get
            {
                return false;
            }
            set
            {
                //
            }
        }
    }
}
