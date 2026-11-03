namespace Bam;

/// <summary>
/// Base64url (RFC 4648 §5) encoding without padding: <c>+</c>/<c>/</c> become <c>-</c>/<c>_</c>
/// and trailing <c>=</c> is omitted.  <see cref="Decode"/> accepts both the unpadded canonical
/// form and padded input.
/// </summary>
public class Base64UrlEncoder : IBase64UrlEncoder
{
    public static Base64UrlEncoder Default { get; } = new Base64UrlEncoder();

    /// <summary>
    /// Encodes the bytes as unpadded base64url.
    /// </summary>
    /// <param name="bytes">The bytes to encode.</param>
    /// <returns>The base64url text.</returns>
    public string Encode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>
    /// Decodes base64url text, padded or not.
    /// </summary>
    /// <param name="base64">The base64url text.</param>
    /// <returns>The decoded bytes.</returns>
    /// <exception cref="FormatException">The text is not valid base64url (including a length that no byte sequence encodes to).</exception>
    public byte[] Decode(string base64)
    {
        ArgumentNullException.ThrowIfNull(base64);
        string standard = base64.Replace('-', '+').Replace('_', '/').TrimEnd('=');
        int remainder = standard.Length % 4;
        if (remainder == 1)
        {
            throw new FormatException("The input is not valid base64url: its length leaves a single trailing character, which no byte sequence encodes to.");
        }
        if (remainder != 0)
        {
            standard = standard.PadRight(standard.Length + (4 - remainder), '=');
        }
        return Convert.FromBase64String(standard);
    }
}
