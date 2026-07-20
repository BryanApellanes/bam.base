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
    public void DeriveGeneralIndexSurfaceFromVectorSettings()
    {
        When.A<VectorIndexAttribute>("derives the general index surface from its vector settings",
            new VectorIndexAttribute { Distance = VectorDistance.Euclidean, Lists = 50 },
            (attribute) =>
            {
                VectorIndexAttribute hnsw = new VectorIndexAttribute { Method = VectorIndexMethod.Hnsw, Distance = VectorDistance.InnerProduct };
                bool setterThrew = false;
                try
                {
                    attribute.AccessMethod = "btree";
                }
                catch (InvalidOperationException)
                {
                    setterThrew = true;
                }
                IndexDefinition definition = attribute.GetIndexDefinition("VectorTestTable", "Embedding");
                return new VectorSurfaceOutcome(
                    attribute.AccessMethod,
                    attribute.OperatorClass,
                    attribute.StorageParameters,
                    hnsw.AccessMethod,
                    hnsw.OperatorClass,
                    hnsw.StorageParameters,
                    setterThrew,
                    definition);
            })
        .TheTest
        .ShouldPass<VectorSurfaceOutcome>((because, outcome) =>
        {
            because.ItsTrue("ivfflat derives from the default method", "ivfflat".Equals(outcome.IvfFlatAccessMethod));
            because.ItsTrue("euclidean selects the l2 operator class", "vector_l2_ops".Equals(outcome.IvfFlatOperatorClass));
            because.ItsTrue("ivfflat carries its list count as storage parameters", "lists = 50".Equals(outcome.IvfFlatStorageParameters));
            because.ItsTrue("hnsw derives from the method", "hnsw".Equals(outcome.HnswAccessMethod));
            because.ItsTrue("inner product selects the ip operator class", "vector_ip_ops".Equals(outcome.HnswOperatorClass));
            because.ItsTrue("hnsw declares no storage parameters", outcome.HnswStorageParameters == null);
            because.ItsTrue("assigning the derived access method throws", outcome.SetterThrew);
            because.ItsTrue("the resolved definition carries the vector options", outcome.Definition.HasAccessMethodOptions && "ivfflat".Equals(outcome.Definition.AccessMethod));
            because.ItsTrue("the resolved definition derives the index name", outcome.Definition.Name.Equals("ix_VectorTestTable_Embedding"));
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

    private sealed record VectorSurfaceOutcome(
        string? IvfFlatAccessMethod,
        string? IvfFlatOperatorClass,
        string? IvfFlatStorageParameters,
        string? HnswAccessMethod,
        string? HnswOperatorClass,
        string? HnswStorageParameters,
        bool SetterThrew,
        IndexDefinition Definition);
}
