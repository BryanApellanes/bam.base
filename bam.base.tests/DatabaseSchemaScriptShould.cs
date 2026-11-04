using Bam.Data;
using Bam.Data.SQLite;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

/// <summary>
/// Covers <see cref="IDatabase.ExecuteSchemaScript(ISqlStringBuilder)"/> (bam.base#40): called through
/// <see cref="IDatabase"/>, both overloads reach <c>ExecuteSql(ISqlStringBuilder, IParameterBuilder)</c> exactly once.
/// Against a bam.data whose <c>Database</c> doesn't define the method, that's the default interface method; from
/// bam.data#28 on it's <c>Database</c>'s own. <see cref="ScriptOutcome.DefinedByImplementation"/> says which ran.
/// </summary>
[UnitTestMenu("Database schema script should")]
public class DatabaseSchemaScriptShould : UnitTestMenuContainer
{
    public DatabaseSchemaScriptShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void RunTheScriptThroughExecuteSql()
    {
        When.A<CountingDatabase>("runs a schema script through IDatabase, with and without a parameter builder",
            NewDatabase,
            database =>
            {
                try
                {
                    IDatabase asInterface = database;
                    asInterface.ExecuteSchemaScript(new SqlStringBuilder("CREATE TABLE SchemaScriptTable (Id INTEGER)"));
                    int afterFirst = database.Executed;
                    asInterface.ExecuteSchemaScript(new SqlStringBuilder("CREATE TABLE OtherScriptTable (Id INTEGER)"), database.ServiceProvider.Get<IParameterBuilder>());
                    bool defined = typeof(SQLiteDatabase).GetMethod(nameof(IDatabase.ExecuteSchemaScript), new[] { typeof(ISqlStringBuilder) }) is not null;
                    return new ScriptOutcome(afterFirst, database.Executed - afterFirst, defined);
                }
                finally
                {
                    Cleanup(database);
                }
            })
            .TheTest
            .ShouldPass<ScriptOutcome>((because, outcome) =>
            {
                because.ItsTrue($"the one-argument script reaches ExecuteSql once ({outcome.OneArgument}, {(outcome.DefinedByImplementation ? "Database's own method" : "the interface default")})", outcome.OneArgument == 1);
                because.ItsTrue($"the two-argument script reaches ExecuteSql once ({outcome.TwoArguments})", outcome.TwoArguments == 1);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    private static CountingDatabase NewDatabase()
    {
        string directory = Path.Combine(Path.GetTempPath(), "bam_base_tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        return new CountingDatabase(directory);
    }

    private static void Cleanup(SQLiteDatabase database)
    {
        try
        {
            string directory = Path.GetDirectoryName(database.DatabaseFile?.FullName) ?? string.Empty;
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
        catch (IOException)
        {
            // best effort: SQLite may still hold the file briefly
        }
    }

    /// <summary>A SQLite database that counts the builder scripts it executes.</summary>
    private sealed class CountingDatabase : SQLiteDatabase
    {
        public CountingDatabase(string directory) : base(directory, "BamBaseSchemaScript")
        {
        }

        public int Executed { get; private set; }

        public override void ExecuteSql(ISqlStringBuilder builder, IParameterBuilder parameterBuilder)
        {
            Executed++;
            base.ExecuteSql(builder, parameterBuilder);
        }
    }

    private sealed record ScriptOutcome(int OneArgument, int TwoArguments, bool DefinedByImplementation);
}
