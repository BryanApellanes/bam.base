/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Bam.Data.Repositories
{
	public class MetaPropertyVersionInfo :IMetaPropertyVersionInfo
	{
		public MetaPropertyVersionInfo() { }

		public string Hash { get; internal set; }
		public PropertyInfo PropertyInfo { get; internal set; } 
		public DateTime LastWrite { get; internal set; }
		public string Name { get; internal set; }
		public int Version { get; internal set; }
		public object Value { get; internal set; }
	}
}
