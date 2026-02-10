using Bam.DependencyInjection;
using Bam.Services;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("Hash should")]
public class HashShould : UnitTestMenuContainer
{
    public HashShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void RoundTrip()
    {
        string value = 256.RandomLetters();

        When.A<string>("round-trips hash bytes through hex string",
            value,
            (v) =>
            {
                byte[] hashBytes = v.ToHashBytes(HashAlgorithms.SHA256);
                string hashHex = hashBytes.ToHexString();
                byte[] backAgain = hashHex.HexToByteArray();
                return new object[] { hashBytes, backAgain };
            })
        .TheTest
        .ShouldPass(because =>
        {
            object[] results = (object[])because.Result;
            byte[] hashBytes = (byte[])results[0];
            byte[] backAgain = (byte[])results[1];
            because.ItsTrue("byte arrays match", backAgain.SequenceEqual(hashBytes));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
