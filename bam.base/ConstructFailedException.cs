/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bam.Incubation
{
    [Serializable]
    public class ConstructFailedException: Exception
    {
		public ConstructFailedException(Type type, Type[] ctorTypes)
			: base(GetMessage(type, ctorTypes))
		{ }

        private static string GetMessage(Type type, Type[] ctorTypes)
        {
            string kind = type.IsInterface ? "interface implementation" : "type";
            return $"Unable to construct instance of {kind} {type.Name}: {type.Name}({(ctorTypes == null || ctorTypes.Length == 0 ? "" : ctorTypes.ToDelimited<Type>(t => t.Name))})";
        }
    }
}
