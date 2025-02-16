/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam
{
    public interface IAssemblyGenerator: ICodeWriter
    {
        GeneratedAssemblyInfo? GenerateAssembly();
    }
}
