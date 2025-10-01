//using Microsoft.AspNetCore.WebUtilities;

namespace Bam;

public class WebEncoderBase64Encoder : IBase64UrlEncoder
{
    public string Encode(byte[] bytes)
    {
        //return WebEncoders.Base64UrlEncode(bytes);
        return Base64UrlEncoder.Default.Encode(bytes);
    }

    public byte[] Decode(string base64)
    {
        //return WebEncoders.Base64UrlDecode(base64);
        return Base64UrlEncoder.Default.Decode(base64);
    }
}