/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public interface IAssemblyGenerator: IWriteSource
    {
        GeneratedAssemblyInfo GenerateAssembly();
    }
}
