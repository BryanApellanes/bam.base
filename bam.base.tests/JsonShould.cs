using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("Json should")]
public class JsonShould : UnitTestMenuContainer
{
    public JsonShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void CarryRawTextWithIdentityConversions()
    {
        When.A<string>("carries raw JSON text through identity conversions",
            "{\"answer\":42}",
            (text) =>
            {
                Json json = text;
                string? roundTripped = json;
                return new JsonConversionOutcome(json.Value, json.ToString(), roundTripped);
            })
        .TheTest
        .ShouldPass<JsonConversionOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("Value holds the raw text", outcome.Value.Equals("{\"answer\":42}"));
            because.ItsTrue("ToString returns the raw text", outcome.Text.Equals("{\"answer\":42}"));
            because.ItsTrue("implicit conversion back to string round-trips", "{\"answer\":42}".Equals(outcome.RoundTripped));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ExposeEmptyArrayAndObjectShapes()
    {
        When.A<Json>("exposes the empty array and object shapes",
            Json.EmptyArray,
            (emptyArray) => new JsonEmptyShapesOutcome(emptyArray.Value, Json.EmptyObject.Value))
        .TheTest
        .ShouldPass<JsonEmptyShapesOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("EmptyArray is []", outcome.EmptyArray.Equals("[]"));
            because.ItsTrue("EmptyObject is {}", outcome.EmptyObject.Equals("{}"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RejectNullText()
    {
        When.A<string>("rejects null JSON text",
            "unused - the constructor argument under test is null",
            (text) =>
            {
                bool threw = false;
                try
                {
                    Json json = new Json(null!);
                }
                catch (ArgumentNullException)
                {
                    threw = true;
                }
                return threw;
            })
        .TheTest
        .ShouldPass<bool>((because, _, threw) =>
        {
            because.ItsTrue("ArgumentNullException was thrown", threw);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void EqualByRawTextValue()
    {
        When.A<Json>("equates by raw text value",
            new Json("[1,2]"),
            (json) => new JsonEqualityOutcome(
                json.Equals(new Json("[1,2]")),
                json.Equals(new Json("[1,3]")),
                json.GetHashCode() == new Json("[1,2]").GetHashCode()))
        .TheTest
        .ShouldPass<JsonEqualityOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("identical text is equal", outcome.SameTextEqual);
            because.ItsTrue("different text is not equal", !outcome.DifferentTextEqual);
            because.ItsTrue("hash codes agree with equality", outcome.HashCodesMatch);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private sealed record JsonConversionOutcome(string Value, string Text, string? RoundTripped);
    private sealed record JsonEmptyShapesOutcome(string EmptyArray, string EmptyObject);
    private sealed record JsonEqualityOutcome(bool SameTextEqual, bool DifferentTextEqual, bool HashCodesMatch);
}
