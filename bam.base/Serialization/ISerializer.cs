﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public interface ISerializer
    {
        byte[] Serialize(object data);
    }
}
