namespace Bam.Data.Dynamic.Objects;

public interface IObjectifier
{
    object Objectify(string data);
    T Objectify<T>(string data);
}