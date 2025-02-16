namespace Bam
{
    public interface ITypeResolver
    {
        Type ResolveType(string typeName);
        Type ResolveType(string nameSpace, string typeName);
    }
}
