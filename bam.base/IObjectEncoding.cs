using Bam.Storage;

namespace Bam
{
    public interface IObjectEncoding : IRawData
    {
        Type Type { get; }
    }
}
