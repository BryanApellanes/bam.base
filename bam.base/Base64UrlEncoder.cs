namespace Bam;

public class Base64UrlEncoder : IBase64UrlEncoder
{
    public string Encode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').Replace("=", "");
    }

    public byte[] Decode(string base64)
    {
        return Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
    }
}