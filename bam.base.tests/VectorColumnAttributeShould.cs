using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("VectorColumnAttribute should")]
public class VectorColumnAttributeShould : UnitTestMenuContainer
{
    public VectorColumnAttributeShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void MapDimensionsOntoColumnDdlSurface()
    {
        When.A<VectorColumnAttribute>("maps its dimension onto the column DDL surface",
            new VectorColumnAttribute(1536) { Name = "Embedding" },
            (attribute) => attribute)
        .TheTest
        .ShouldPass(because =>
        {
            VectorColumnAttribute attribute = (VectorColumnAttribute)because.Result;
            because.ItsTrue("DbDataType is vector", attribute.DbDataType.Equals("vector"));
            because.ItsTrue("MaxLength mirrors the dimension", attribute.MaxLength.Equals("1536"));
            because.ItsTrue("Dimensions is authoritative", attribute.Dimensions == 1536);
            because.ItsTrue("vector columns allow null by default", attribute.AllowNull);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RejectNonPositiveDimensions()
    {
        When.A<int>("rejects a non-positive dimension",
            0,
            (dimensions) =>
            {
                bool threw = false;
                try
                {
                    VectorColumnAttribute attribute = new VectorColumnAttribute(dimensions);
                }
                catch (ArgumentOutOfRangeException)
                {
                    threw = true;
                }
                return threw;
            })
        .TheTest
        .ShouldPass(because =>
        {
            because.ItsTrue("ArgumentOutOfRangeException was thrown", (bool)because.Result);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void CarryVectorIndexDeclarationDefaults()
    {
        When.A<VectorIndexAttribute>("carries vector index declaration defaults",
            new VectorIndexAttribute(),
            (attribute) => attribute)
        .TheTest
        .ShouldPass(because =>
        {
            VectorIndexAttribute attribute = (VectorIndexAttribute)because.Result;
            because.ItsTrue("method defaults to ivfflat", attribute.Method == VectorIndexMethod.IvfFlat);
            because.ItsTrue("distance defaults to cosine", attribute.Distance == VectorDistance.Cosine);
            because.ItsTrue("lists defaults to 100", attribute.Lists == 100);
            because.ItsTrue("name defaults to null for derivation by the schema writer", attribute.Name == null);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
