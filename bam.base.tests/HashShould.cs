using Bam;
using Bam.CoreServices;
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
        byte[] backAgain = hashHex.HashToByteArray();
        
        backAgain.SequenceEqual(hashBytes).ShouldBeTrue("byte arrays didn't match");
    }
}