using System.Text;
using Bam.Net;

namespace Bam.Storage;

public interface IRawData
{
    HashAlgorithms HashAlgorithm { get; }
    Encoding Encoding { get; }
    ulong HashId { get; }
    string HashString { get; }
    byte[] Hash { get; }
    byte[] Value { get; }
}