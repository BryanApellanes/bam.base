using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public class BindingNotFoundException : Exception
    {
        public BindingNotFoundException(Type interfaceType) : base($"Binding not found for interface {interfaceType.Namespace}.{interfaceType.Name}")
        {
            this.InterfaceType = interfaceType;
        }

        public Type InterfaceType { get; private set; }
    }
}
