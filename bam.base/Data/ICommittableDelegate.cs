﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Data
{
    public delegate void ICommittableDelegate(IDatabase db, ICommittable committable);
}
