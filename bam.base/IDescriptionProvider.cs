using System.Reflection;

namespace Bam.Documentation
{
    /// <summary>
    /// Provides descriptions for reflected MemberInfos
    /// </summary>
    public interface IDescriptionProvider
    {
        string GetTypeDescription(Type type);
        string GetMethodDescription(MethodInfo method);
        string GetParameterDescription(ParameterInfo parameterInfo);
        string GetPropertyDescription(PropertyInfo propertyInfo);
    }
}
