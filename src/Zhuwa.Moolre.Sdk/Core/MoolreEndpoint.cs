namespace Zhuwa.Moolre.Core;

/// <summary>
/// One Moolre API operation: where it is, how it is called, which credentials it needs and whether repeating it is safe.
/// </summary>
/// <param name="Name">The SDK operation, for example <c>Transfers.SendAsync</c> (used in exceptions and logs).</param>
/// <param name="Method">The HTTP method.</param>
/// <param name="Path">The path below the base address (several operations share one path).</param>
/// <param name="Type">
/// The integer <c>type</c> field that tells operations on a shared path apart, or <see langword="null"/> when the
/// operation has none (bundles, WhatsApp).
/// </param>
/// <param name="Credential">The credentials the operation needs.</param>
/// <param name="IsSafeToRepeat">
/// <see langword="true"/> only for read-only operations. Anything that moves money, creates something or sends a
/// message is <see langword="false"/> and is never retried automatically.
/// </param>
internal sealed record MoolreEndpoint(
    string Name,
    HttpMethod Method,
    string Path,
    int? Type,
    CredentialKind Credential,
    bool IsSafeToRepeat);
