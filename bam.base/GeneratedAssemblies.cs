/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam
{
	public static class GeneratedAssemblies
	{
		static readonly Dictionary<string, GeneratedAssemblyInfo> _generatedAssemblies = new Dictionary<string, GeneratedAssemblyInfo>();
		public static GeneratedAssemblyInfo? GetGeneratedAssemblyInfo(string name)
		{
			if (_generatedAssemblies.ContainsKey(name))
			{
				return _generatedAssemblies[name];
			}

			return null;
		}

		public static void SetAssemblyInfo(string name, GeneratedAssemblyInfo assembly)
		{
			_generatedAssemblies[name] = assembly;
		}
	}
}
