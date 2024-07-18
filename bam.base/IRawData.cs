using System.Text;
using Bam;

namespace Bam.Storage;

public interface IRawData
{
    HashAlgorithms HashAlgorithm { get; }
    Encoding Encoding { get; }
    ulong HashId { get; }
    /// <summary>
    /// Gets the hash hex string equivalent.
    /// </summary>
    string HashHexString { get; }
    byte[] Hash { get; }
    byte[] Value { get; }
}