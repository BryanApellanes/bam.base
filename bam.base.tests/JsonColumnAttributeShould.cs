using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("JsonColumnAttribute should")]
public class JsonColumnAttributeShould : UnitTestMenuContainer
{
    public JsonColumnAttributeShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void DeclareJsonbWithNoDefaultByDefault()
    {
        When.A<JsonColumnAttribute>("declares jsonb with no default literal by default",
            new JsonColumnAttribute { Name = "Metadata" },
            (attribute) => attribute)
        .TheTest
        .ShouldPass<JsonColumnAttribute>((because, _, attribute) =>
        {
            because.ItsTrue("DbDataType is jsonb", attribute.DbDataType.Equals("jsonb"));
            because.ItsTrue("DefaultLiteral is null", attribute.DefaultLiteral == null);
            because.ItsTrue("default clause is empty", attribute.GetDefaultClause().Equals(string.Empty));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderDefaultClauseFromLiteral()
    {
        When.A<JsonColumnAttribute>("renders the DEFAULT clause from its literal",
            new JsonColumnAttribute("'[]'") { Name = "Lessons" },
            (attribute) => attribute)
        .TheTest
        .ShouldPass<JsonColumnAttribute>((because, _, attribute) =>
        {
            because.ItsTrue("DefaultLiteral carries the literal", attribute.DefaultLiteral!.Equals("'[]'"));
            because.ItsTrue("default clause renders with leading space", attribute.GetDefaultClause().Equals(" DEFAULT '[]'"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void NormalizeEmptyLiteralToNullThroughTheContract()
    {
        When.A<JsonColumnAttribute>("normalizes an empty default literal to null through IDefaultLiteralColumn",
            new JsonColumnAttribute(string.Empty) { Name = "Metadata" },
            (attribute) => attribute)
        .TheTest
        .ShouldPass<JsonColumnAttribute>((because, _, attribute) =>
        {
            IDefaultLiteralColumn contract = attribute;
            because.ItsTrue("DefaultLiteral is null through the contract", contract.DefaultLiteral == null);
            because.ItsTrue("default clause is empty through the contract", contract.GetDefaultClause().Equals(string.Empty));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
