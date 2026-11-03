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
    /// Decodes base64url text, padded or not.  Strict: only <c>A-Z</c>, <c>a-z</c>, <c>0-9</c>,
    /// <c>-</c> and <c>_</c> are accepted (never <c>+</c>, <c>/</c> or whitespace), trailing
    /// <c>=</c> is accepted only when it exactly completes the final group, and a length that
    /// no byte sequence encodes to is rejected.
    /// </summary>
    /// <param name="base64">The base64url text.</param>
    /// <returns>The decoded bytes.</returns>
    /// <exception cref="FormatException">The text is not valid base64url.</exception>
    public byte[] Decode(string base64)
    {
        ArgumentNullException.ThrowIfNull(base64);
        int end = base64.Length;
        while (end > 0 && base64[end - 1] == '=')
        {
            end--;
        }
        int padding = base64.Length - end;
        for (int i = 0; i < end; i++)
        {
            if (!IsUrlAlphabet(base64[i]))
            {
                throw new FormatException($"The input is not valid base64url: character at position {i} is outside the base64url alphabet.");
            }
        }
        int remainder = end % 4;
        if (remainder == 1)
        {
            throw new FormatException("The input is not valid base64url: its length leaves a single trailing character, which no byte sequence encodes to.");
        }
        int required = remainder == 0 ? 0 : 4 - remainder;
        if (padding != 0 && padding != required)
        {
            throw new FormatException("The input is not valid base64url: its padding does not complete the final group.");
        }
        string standard = base64.Substring(0, end).Replace('-', '+').Replace('_', '/') + new string('=', required);
        return Convert.FromBase64String(standard);
    }

    private static bool IsUrlAlphabet(char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_';
    }
}
