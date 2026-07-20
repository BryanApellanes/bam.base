using System.Reflection;
using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("IndexAttribute should")]
public class IndexAttributeShould : UnitTestMenuContainer
{
    public IndexAttributeShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void CarryGeneralIndexDeclarationDefaults()
    {
        When.A<IndexAttribute>("carries general index declaration defaults",
            new IndexAttribute(),
            (attribute) => attribute)
        .TheTest
        .ShouldPass<IndexAttribute>((because, attribute) =>
        {
            because.ItsTrue("name defaults to null for derivation by the schema writer", attribute.Name == null);
            because.ItsTrue("indexes are not unique by default", !attribute.Unique);
            because.ItsTrue("column names default to empty for property-level inference", attribute.ColumnNames.Length == 0);
            because.ItsTrue("order defaults to unspecified", attribute.Order == SortOrder.Unspecified);
            because.ItsTrue("column orders default to null", attribute.ColumnOrders == null);
            because.ItsTrue("access method defaults to null", attribute.AccessMethod == null);
            because.ItsTrue("operator class defaults to null", attribute.OperatorClass == null);
            because.ItsTrue("storage parameters default to null", attribute.StorageParameters == null);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ResolvePropertyLevelDeclarationsToDefinitions()
    {
        When.A<IndexAttribute>("resolves a property-level declaration to a definition",
            new IndexAttribute { Order = SortOrder.Descending, Unique = true },
            (attribute) => attribute.GetIndexDefinition("Episode", "CreatedAt"))
        .TheTest
        .ShouldPass<IndexDefinition>((because, definition) =>
        {
            because.ItsTrue("the name derives from table and column", definition.Name.Equals("ix_Episode_CreatedAt"));
            because.ItsTrue("the table name is carried", definition.TableName.Equals("Episode"));
            because.ItsTrue("one column is covered", definition.Columns.Count == 1);
            because.ItsTrue("the inferred column is used", definition.Columns[0].ColumnName.Equals("CreatedAt"));
            because.ItsTrue("the property-level order is applied", definition.Columns[0].Order == SortOrder.Descending);
            because.ItsTrue("uniqueness is carried", definition.Unique);
            because.ItsTrue("no access-method options are present", !definition.HasAccessMethodOptions);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ResolveClassLevelCompositeDeclarationsToDefinitions()
    {
        When.A<IndexAttribute>("resolves a class-level composite declaration to a definition",
            new IndexAttribute("TenantId", "CreatedAt") { ColumnOrders = new SortOrder[] { SortOrder.Unspecified, SortOrder.Descending } },
            (attribute) => attribute.GetIndexDefinition("Episode"))
        .TheTest
        .ShouldPass<IndexDefinition>((because, definition) =>
        {
            because.ItsTrue("the name derives from table and all columns", definition.Name.Equals("ix_Episode_TenantId_CreatedAt"));
            because.ItsTrue("both columns are covered in order", definition.Columns.Count == 2 && definition.Columns[0].ColumnName.Equals("TenantId") && definition.Columns[1].ColumnName.Equals("CreatedAt"));
            because.ItsTrue("per-column orders are applied", definition.Columns[0].Order == SortOrder.Unspecified && definition.Columns[1].Order == SortOrder.Descending);
            because.ItsTrue("the index is not unique by default", !definition.Unique);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void HonorExplicitNamesAndAccessMethodOptions()
    {
        When.A<IndexAttribute>("honors an explicit name and access-method options",
            new IndexAttribute { Name = "ix_custom", AccessMethod = "gin", OperatorClass = "jsonb_path_ops", StorageParameters = "fastupdate = off" },
            (attribute) => attribute.GetIndexDefinition("Episode", "Metadata"))
        .TheTest
        .ShouldPass<IndexDefinition>((because, definition) =>
        {
            because.ItsTrue("the explicit name wins over derivation", definition.Name.Equals("ix_custom"));
            because.ItsTrue("the access method is carried", "gin".Equals(definition.AccessMethod));
            because.ItsTrue("the operator class is carried", "jsonb_path_ops".Equals(definition.OperatorClass));
            because.ItsTrue("the storage parameters are carried", "fastupdate = off".Equals(definition.StorageParameters));
            because.ItsTrue("access-method options are flagged", definition.HasAccessMethodOptions);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RejectInvalidClassLevelDeclarations()
    {
        When.A<IndexAttribute>("rejects invalid class-level declarations",
            new IndexAttribute(),
            (attribute) =>
            {
                bool emptyColumnsThrew = false;
                try
                {
                    attribute.GetIndexDefinition("Episode");
                }
                catch (InvalidOperationException)
                {
                    emptyColumnsThrew = true;
                }
                bool mismatchedOrdersThrew = false;
                try
                {
                    IndexAttribute mismatched = new IndexAttribute("TenantId", "CreatedAt") { ColumnOrders = new SortOrder[] { SortOrder.Descending } };
                    mismatched.GetIndexDefinition("Episode");
                }
                catch (InvalidOperationException)
                {
                    mismatchedOrdersThrew = true;
                }
                return new IndexRejectionOutcome(emptyColumnsThrew, mismatchedOrdersThrew);
            })
        .TheTest
        .ShouldPass<IndexRejectionOutcome>((because, outcome) =>
        {
            because.ItsTrue("a class-level declaration naming no columns throws", outcome.EmptyColumnsThrew);
            because.ItsTrue("a column-orders length mismatch throws", outcome.MismatchedOrdersThrew);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ResolveDeclarationsAppliedAsAttributes()
    {
        When.A<Type>("resolves declarations applied as attributes through reflection",
            typeof(IndexedFixture),
            (fixtureType) =>
            {
                IndexAttribute[] classLevel = fixtureType.GetCustomAttributes<IndexAttribute>(false).ToArray();
                PropertyInfo property = fixtureType.GetProperty(nameof(IndexedFixture.CreatedAt))!;
                IndexAttribute[] propertyLevel = property.GetCustomAttributes<IndexAttribute>(false).ToArray();
                return new AttributeUsageOutcome(classLevel, propertyLevel);
            })
        .TheTest
        .ShouldPass<AttributeUsageOutcome>((because, outcome) =>
        {
            because.ItsTrue("both class-level declarations are retrieved (AllowMultiple)", outcome.ClassLevel.Length == 2);
            because.ItsTrue("the unique single-column declaration round-trips", outcome.ClassLevel.Any(attribute => attribute.Unique && attribute.ColumnNames.Length == 1 && attribute.ColumnNames[0].Equals("TenantId")));
            because.ItsTrue("the composite declaration round-trips", outcome.ClassLevel.Any(attribute => !attribute.Unique && attribute.ColumnNames.Length == 2 && attribute.ColumnNames[1].Equals("CreatedAt")));
            because.ItsTrue("the property-level declaration is retrieved", outcome.PropertyLevel.Length == 1);
            because.ItsTrue("the property-level order round-trips", outcome.PropertyLevel[0].Order == SortOrder.Descending);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [Index("TenantId", Unique = true)]
    [Index("TenantId", "CreatedAt")]
    private sealed class IndexedFixture
    {
        [Index(Order = SortOrder.Descending)]
        public string? CreatedAt { get; set; }
    }

    private sealed record IndexRejectionOutcome(bool EmptyColumnsThrew, bool MismatchedOrdersThrew);

    private sealed record AttributeUsageOutcome(IndexAttribute[] ClassLevel, IndexAttribute[] PropertyLevel);
}
