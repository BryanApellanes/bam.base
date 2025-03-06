namespace Bam.DependencyInjection;

public class DependencyLoopException : Exception
{
    public DependencyLoopException(Type type, HashSet<Type> chain) : base($"Dependency loop detected: {string.Join(" => ", chain.Select(t => t.Name))} => {type.Name}")
    {
        this.Chain = chain;
        this.LoopType = type;
    }
    
    public Type LoopType { get; set; }
    
    public HashSet<Type> Chain { get; }
}