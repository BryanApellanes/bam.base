using Microsoft.AspNetCore.WebUtilities;

namespace Bam;

public class WebEncoderBase64Encoder : IBase64UrlEncoder
{
    public string Encode(byte[] bytes)
    {
        return WebEncoders.Base64UrlEncode(bytes);
    }

    public byte[] Decode(string base64)
    {
        return WebEncoders.Base64UrlDecode(base64);
    }
}