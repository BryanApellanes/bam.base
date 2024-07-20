using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Primitives;

namespace Bam;

public class CircularDependencyException : Exception
{
    public CircularDependencyException(HashSet<Type> types) : base(GetMessage(types))
    {
    }

    private static string GetMessage(HashSet<Type> types)
    {
        StringBuilder message = new StringBuilder();
        Type[] typeArray = types.ToArray();
        for (int i = 0; i < typeArray.Length; i++)
        {
            message.Append(typeArray[i].FullName);
            if (i != types.Count - 1)
            {
                message.Append(", ");
            }
        }

        return message.ToString();
    }
}