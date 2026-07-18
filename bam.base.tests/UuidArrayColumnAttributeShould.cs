using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("UuidArrayColumnAttribute should")]
public class UuidArrayColumnAttributeShould : UnitTestMenuContainer
{
    public UuidArrayColumnAttributeShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void DeclareUuidArrayWithEmptyArrayDefault()
    {
        When.A<UuidArrayColumnAttribute>("declares uuid[] with an empty-array default",
            new UuidArrayColumnAttribute { Name = "RelatedEpisodeIds" },
            (attribute) => attribute)
        .TheTest
        .ShouldPass<UuidArrayColumnAttribute>((because, _, attribute) =>
        {
            because.ItsTrue("DbDataType is uuid[]", attribute.DbDataType.Equals("uuid[]"));
            because.ItsTrue("DefaultLiteral defaults to '{}'", attribute.DefaultLiteral!.Equals("'{}'"));
            because.ItsTrue("default clause renders with leading space", attribute.GetDefaultClause().Equals(" DEFAULT '{}'"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void SuppressDefaultWhenLiteralIsNull()
    {
        When.A<UuidArrayColumnAttribute>("suppresses the DEFAULT clause when the literal is null",
            new UuidArrayColumnAttribute(null) { Name = "RelatedEpisodeIds" },
            (attribute) => attribute)
        .TheTest
        .ShouldPass<UuidArrayColumnAttribute>((because, _, attribute) =>
        {
            because.ItsTrue("DefaultLiteral is null", attribute.DefaultLiteral == null);
            because.ItsTrue("default clause is empty", attribute.GetDefaultClause().Equals(string.Empty));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void NormalizeEmptyLiteralToNullThroughTheContract()
    {
        When.A<UuidArrayColumnAttribute>("normalizes an empty default literal to null through IDefaultLiteralColumn",
            new UuidArrayColumnAttribute(string.Empty) { Name = "RelatedEpisodeIds" },
            (attribute) => attribute)
        .TheTest
        .ShouldPass<UuidArrayColumnAttribute>((because, _, attribute) =>
        {
            IDefaultLiteralColumn contract = attribute;
            because.ItsTrue("DefaultLiteral is null through the contract", contract.DefaultLiteral == null);
            because.ItsTrue("default clause is empty through the contract", contract.GetDefaultClause().Equals(string.Empty));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
