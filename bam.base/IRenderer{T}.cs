﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public interface IRenderer<T> : IRenderer
    {
        string Render(T toRender);
    }
}
