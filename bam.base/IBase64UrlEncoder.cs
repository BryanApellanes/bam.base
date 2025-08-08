namespace Bam;

public interface IBase64UrlEncoder
{
    string Encode(byte[] bytes);
    byte[] Decode(string base64);
}