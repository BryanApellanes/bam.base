using System;
using System.Collections.Generic;
using System.Text;

namespace Bam
{
    public interface IRenderer
    {
        string Render(object toRender);
    }
}
