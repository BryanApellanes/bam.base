namespace Bam;

public class TypedBindingNotFoundException : BindingNotFoundException
{
    public TypedBindingNotFoundException(Type dependantType, Type interfaceType) : base(interfaceType, $"Binding not found for interface {interfaceType.Namespace}.{interfaceType.Name} dependant type {dependantType.Namespace}.{dependantType.Name}")
    {
        this.DependantType = dependantType;
    }
    
    public Type DependantType { get; init; }
}