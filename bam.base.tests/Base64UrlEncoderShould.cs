using System.Text;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("Base64UrlEncoder should", Selector = "b64u")]
public class Base64UrlEncoderShould : UnitTestMenuContainer
{
    public Base64UrlEncoderShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void RoundTripEveryLength()
    {
        After.Setup(reg =>
        {
            reg.Set(Base64UrlEncoder.Default);
        })
        .When<Base64UrlEncoder>("encodes and decodes byte arrays of every length from 0 to 64", (encoder, reg) =>
        {
            List<int> failedLengths = new List<int>();
            List<int> paddedOutputs = new List<int>();
            List<int> nonUrlSafeOutputs = new List<int>();
            for (int length = 0; length <= 64; length++)
            {
                byte[] bytes = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    bytes[i] = (byte)(251 + i);
                }
                string encoded = encoder.Encode(bytes);
                if (encoded.Contains('=') )
                {
                    paddedOutputs.Add(length);
                }
                if (encoded.Contains('+') || encoded.Contains('/'))
                {
                    nonUrlSafeOutputs.Add(length);
                }
                try
                {
                    byte[] decoded = encoder.Decode(encoded);
                    if (!decoded.AsSpan().SequenceEqual(bytes))
                    {
                        failedLengths.Add(length);
                    }
                }
                catch (FormatException)
                {
                    failedLengths.Add(length);
                }
            }
            return new RoundTripOutcome(failedLengths, paddedOutputs, nonUrlSafeOutputs);
        })
        .TheTest
        .ShouldPass<RoundTripOutcome>((because, outcome) =>
        {
            because.ItsTrue($"every length round-trips (failed: {string.Join(",", outcome.FailedLengths)})", outcome.FailedLengths.Count == 0);
            because.ItsTrue("encoded output carries no padding", outcome.PaddedOutputs.Count == 0);
            because.ItsTrue("encoded output uses only the url-safe alphabet", outcome.NonUrlSafeOutputs.Count == 0);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void MatchRfc4648Vectors()
    {
        After.Setup(reg =>
        {
            reg.Set(Base64UrlEncoder.Default);
        })
        .When<Base64UrlEncoder>("encodes the RFC 4648 section 10 vectors", (encoder, reg) =>
        {
            return new VectorOutcome(
                encoder.Encode(Encoding.ASCII.GetBytes("f")),
                encoder.Encode(Encoding.ASCII.GetBytes("fo")),
                encoder.Encode(Encoding.ASCII.GetBytes("foo")),
                encoder.Encode(Encoding.ASCII.GetBytes("foob")),
                encoder.Encode(Encoding.ASCII.GetBytes("fooba")),
                encoder.Encode(Encoding.ASCII.GetBytes("foobar")),
                encoder.Encode(new byte[] { 0xfb, 0xff }),
                Encoding.ASCII.GetString(encoder.Decode("Zm9vYg")),
                Encoding.ASCII.GetString(encoder.Decode("Zm9vYg==")),
                encoder.Decode("-_8"));
        })
        .TheTest
        .ShouldPass<VectorOutcome>((because, outcome) =>
        {
            because.ItsTrue("f encodes as Zg", outcome.F == "Zg");
            because.ItsTrue("fo encodes as Zm8", outcome.Fo == "Zm8");
            because.ItsTrue("foo encodes as Zm9v", outcome.Foo == "Zm9v");
            because.ItsTrue("foob encodes as Zm9vYg", outcome.Foob == "Zm9vYg");
            because.ItsTrue("fooba encodes as Zm9vYmE", outcome.Fooba == "Zm9vYmE");
            because.ItsTrue("foobar encodes as Zm9vYmFy", outcome.Foobar == "Zm9vYmFy");
            because.ItsTrue("0xfbff encodes with the url-safe alphabet as -_8", outcome.UrlSafe == "-_8");
            because.ItsTrue("unpadded Zm9vYg decodes to foob", outcome.DecodedUnpadded == "foob");
            because.ItsTrue("padded Zm9vYg== decodes to foob", outcome.DecodedPadded == "foob");
            because.ItsTrue("-_8 decodes to 0xfbff", outcome.DecodedUrlSafe.AsSpan().SequenceEqual(new byte[] { 0xfb, 0xff }));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RejectImpossibleLengthAndInvalidCharacters()
    {
        After.Setup(reg =>
        {
            reg.Set(Base64UrlEncoder.Default);
        })
        .When<Base64UrlEncoder>("decodes inputs that no byte sequence produces", (encoder, reg) =>
        {
            return new RejectionOutcome(Rejects(encoder, "Z"), Rejects(encoder, "Zm9vY"), Rejects(encoder, "Zm9v!g"), Rejects(encoder, "Zm9vYg"));
        })
        .TheTest
        .ShouldPass<RejectionOutcome>((because, outcome) =>
        {
            because.ItsTrue("a single trailing character is rejected", outcome.SingleCharacter);
            because.ItsTrue("a 4n+1 length is rejected", outcome.LengthFourNPlusOne);
            because.ItsTrue("a character outside the alphabet is rejected", outcome.InvalidCharacter);
            because.ItsTrue("a valid unpadded input is accepted", !outcome.Valid);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private static bool Rejects(Base64UrlEncoder encoder, string input)
    {
        try
        {
            encoder.Decode(input);
            return false;
        }
        catch (FormatException)
        {
            return true;
        }
    }

    private sealed record RoundTripOutcome(List<int> FailedLengths, List<int> PaddedOutputs, List<int> NonUrlSafeOutputs);

    private sealed record VectorOutcome(string F, string Fo, string Foo, string Foob, string Fooba, string Foobar, string UrlSafe,
        string DecodedUnpadded, string DecodedPadded, byte[] DecodedUrlSafe);

    private sealed record RejectionOutcome(bool SingleCharacter, bool LengthFourNPlusOne, bool InvalidCharacter, bool Valid);
}
