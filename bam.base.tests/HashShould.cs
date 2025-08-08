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
        byte[] hashBytes = value.ToHashBytes(HashAlgorithms.SHA256);
        string hashHex = hashBytes.ToHexString();
        byte[] backAgain = hashHex.HexToByteArray();
        
        backAgain.SequenceEqual(hashBytes).ShouldBeTrue("byte arrays didn't match");
    }
}