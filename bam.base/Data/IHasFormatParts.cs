/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bam.Data
{
    public interface IHasFormatParts
    {
        List<IFormatPart> Parts
        {
            get;
            set;
        }
    }
}
