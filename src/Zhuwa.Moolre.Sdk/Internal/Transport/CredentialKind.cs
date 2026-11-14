namespace Zhuwa.Moolre.Internal.Transport;

/// <summary>
/// Which of Moolre's credentials an endpoint needs. Each call sets only the headers of its own kind, so one
/// credential is never sent where it is not required.
/// </summary>
internal enum CredentialKind
{
    /// <summary>No credentials (reference data).</summary>
    None = 0,

    /// <summary><c>X-API-USER</c> and the private <c>X-API-KEY</c>.</summary>
    Private = 1,

    /// <summary><c>X-API-USER</c> and the public <c>X-API-PUBKEY</c>.</summary>
    Public = 2,

    /// <summary><c>X-API-USER</c> and <c>X-API-KEY</c>, which Moolre accepts as either the public or the private key.</summary>
    Either = 3,

    /// <summary>The SMS service <c>X-API-VASKEY</c>.</summary>
    Sms = 4,

    /// <summary>The WhatsApp service <c>X-API-VASKEY</c>.</summary>
    WhatsApp = 5,
}
