using System.Globalization;
using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("Vector should")]
public class VectorShould : UnitTestMenuContainer
{
    public VectorShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void RoundTripThroughPgvectorLiteral()
    {
        Vector vector = new Vector(new float[] { 0.1f, -2.5f, 3f });

        When.A<Vector>("round-trips through its pgvector literal",
            vector,
            (v) =>
            {
                string literal = v.ToString();
                Vector backAgain = Vector.Parse(literal);
                return new object[] { literal, backAgain };
            })
        .TheTest
        .ShouldPass(because =>
        {
            object[] results = (object[])because.Result;
            string literal = (string)results[0];
            Vector backAgain = (Vector)results[1];
            because.ItsTrue("literal is bracketed", literal.StartsWith("[") && literal.EndsWith("]"));
            because.ItsTrue("parsed vector equals the original", backAgain.Equals(new Vector(new float[] { 0.1f, -2.5f, 3f })));
            because.ItsTrue("dimension survives the round trip", backAgain.Dimensions == 3);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void FormatWithInvariantCultureRegardlessOfCurrentCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        When.A<Vector>("formats its literal under a comma-decimal culture",
            new Vector(new float[] { 1.5f, 2.25f }),
            (v) =>
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                try
                {
                    return v.ToString();
                }
                finally
                {
                    CultureInfo.CurrentCulture = originalCulture;
                }
            })
        .TheTest
        .ShouldPass(because =>
        {
            string literal = (string)because.Result;
            because.ItsTrue("literal uses dot decimal separators", literal.Equals("[1.5,2.25]"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ThrowOnMalformedLiteralOrDimensionMismatch()
    {
        When.A<string>("rejects malformed literals and mismatched dimensions",
            "not-a-literal",
            (malformed) =>
            {
                bool malformedThrew = false;
                bool mismatchThrew = false;
                bool emptyThrew = false;
                try
                {
                    Vector.Parse(malformed);
                }
                catch (FormatException)
                {
                    malformedThrew = true;
                }
                try
                {
                    Vector.Parse("[1,2,3]", 4);
                }
                catch (FormatException)
                {
                    mismatchThrew = true;
                }
                try
                {
                    Vector empty = new Vector(new float[] { });
                }
                catch (ArgumentException)
                {
                    emptyThrew = true;
                }
                return new bool[] { malformedThrew, mismatchThrew, emptyThrew };
            })
        .TheTest
        .ShouldPass(because =>
        {
            bool[] results = (bool[])because.Result;
            because.ItsTrue("malformed literal throws FormatException", results[0]);
            because.ItsTrue("dimension mismatch throws FormatException", results[1]);
            because.ItsTrue("empty component list throws ArgumentException", results[2]);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ConvertImplicitlyFromFloatArray()
    {
        When.A<float[]>("converts implicitly from a float array",
            new float[] { 4f, 5f, 6f },
            (values) =>
            {
                Vector vector = values;
                return vector;
            })
        .TheTest
        .ShouldPass(because =>
        {
            Vector vector = (Vector)because.Result;
            because.ItsTrue("dimension matches the source array", vector.Dimensions == 3);
            because.ItsTrue("components match the source array", vector.Values[0] == 4f && vector.Values[1] == 5f && vector.Values[2] == 6f);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
