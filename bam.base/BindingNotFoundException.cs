namespace Bam
{
    public class BindingNotFoundException : Exception
    {
        internal BindingNotFoundException(Type interfaceType, string message) : base(message)
        {
            this.InterfaceType = interfaceType;
        }
        
        public BindingNotFoundException(Type interfaceType) : base($"Binding not found for interface {interfaceType.Namespace}.{interfaceType.Name}")
        {
            this.InterfaceType = interfaceType;
        }

        public Type InterfaceType { get; private set; }
    }
}
