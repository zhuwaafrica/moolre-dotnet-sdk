using System.Net.Http.Headers;

namespace Zhuwa.Moolre.Internal.Transport;

/// <summary>Decides which credential headers an operation sends, and refuses to send a call that lacks them.</summary>
internal static class MoolreRequestPolicy
{
    internal const string ApiUserHeader = "X-API-USER";
    internal const string PrivateKeyHeader = "X-API-KEY";
    internal const string PublicKeyHeader = "X-API-PUBKEY";
    internal const string VasKeyHeader = "X-API-VASKEY";

    /// <summary>Adds the credential headers the operation needs.</summary>
    /// <exception cref="InvalidOperationException">A credential the operation needs is not configured.</exception>
    internal static void ApplyCredentials(HttpRequestHeaders headers, MoolreEndpoint endpoint, MoolreOptions options)
    {
        // The sandbox does not check the private or public key, so only the live API insists on them.
        var keysRequired = options.Environment == MoolreEnvironment.Live;

        switch (endpoint.Credential)
        {
            case CredentialKind.None:
                break;
            case CredentialKind.Private:
                AddUser(headers, endpoint, options);
                AddKey(headers, PrivateKeyHeader, options.PrivateKey, keysRequired, endpoint, nameof(MoolreOptions.PrivateKey));
                break;
            case CredentialKind.Public:
                AddUser(headers, endpoint, options);
                AddKey(headers, PublicKeyHeader, options.PublicKey, keysRequired, endpoint, nameof(MoolreOptions.PublicKey));
                break;
            case CredentialKind.Either:
                // Moolre accepts the public or the private key here and reads it from X-API-KEY. Prefer the private key when both exist.
                AddUser(headers, endpoint, options);
                var either = string.IsNullOrWhiteSpace(options.PrivateKey) ? options.PublicKey : options.PrivateKey;
                AddKey(headers, PrivateKeyHeader, either, keysRequired, endpoint, $"{nameof(MoolreOptions.PrivateKey)} or {nameof(MoolreOptions.PublicKey)}");
                break;
            // SMS and WhatsApp use their own VAS keys, which the sandbox also requires. They are sent with no user header.
            case CredentialKind.Sms:
                AddKey(headers, VasKeyHeader, options.SmsVasKey, required: true, endpoint, nameof(MoolreOptions.SmsVasKey));
                break;
            case CredentialKind.WhatsApp:
                AddKey(headers, VasKeyHeader, options.WhatsAppVasKey, required: true, endpoint, nameof(MoolreOptions.WhatsAppVasKey));
                break;
            // A new CredentialKind must be handled above; failing loudly here is safer than sending a call with no credentials.
            default:
                throw new InvalidOperationException($"Unknown credential kind '{endpoint.Credential}' for {endpoint.Name}.");
        }
    }

    private static void AddUser(HttpRequestHeaders headers, MoolreEndpoint endpoint, MoolreOptions options) =>
        AddKey(headers, ApiUserHeader, options.ApiUser, required: true, endpoint, nameof(MoolreOptions.ApiUser));

    // Adds one header. A blank value is skipped unless the call requires it, in which case the call fails before it is sent.
    private static void AddKey(HttpRequestHeaders headers, string header, string? value, bool required, MoolreEndpoint endpoint, string optionName)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            try
            {
                // Add validates the value (no line breaks or control characters), so a bad value cannot add extra headers.
                headers.Add(header, value.Trim());
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException($"MoolreOptions.{optionName} contains characters that cannot be sent in an HTTP header.", ex);
            }
        }
        else if (required)
        {
            throw new InvalidOperationException($"{endpoint.Name} needs MoolreOptions.{optionName}, but it is not set.");
        }
    }
}
