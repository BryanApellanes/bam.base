namespace Bam.DependencyInjection;

public class DependencyLoopException : Exception
{
    public DependencyLoopException(HashSet<Type> chain) : base($"Dependency loop detected: {string.Join(" => ", chain.Select(t => t.Name))} => {chain.First().Name}")
    {
        this.Chain = chain;
    }
    
    public HashSet<Type> Chain { get; }
}