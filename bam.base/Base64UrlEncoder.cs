namespace Bam;

public class Base64UrlEncoder : IBase64UrlEncoder
{
    public static Base64UrlEncoder Default { get; } = new Base64UrlEncoder();
    public string Encode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_'); 
    }

    public byte[] Decode(string base64)
    {
        return Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
    }
}