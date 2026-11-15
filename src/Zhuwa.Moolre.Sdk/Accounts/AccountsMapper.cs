using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Accounts;

/// <summary>
/// Turns Moolre's account responses into the public result types. Static and hand-written: no reflection, so it is safe
/// for trimming and AOT, and every field rule is visible in one place. The client sends the call; this reads the answer.
/// </summary>
internal static class AccountsMapper
{
    /// <summary>Maps a <c>SW01</c> account status response.</summary>
    /// <param name="envelope">The parsed response. Moolre's shapes vary, so every field read falls back to a default.</param>
    internal static AccountStatusResult ToAccountStatusResult(this ResponseEnvelope envelope)
    {
        var data = envelope.Data;

        return new AccountStatusResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            data.ReadDecimal("balance"),
            data.ReadString("accountname") ?? string.Empty,
            data.ReadFlag("activated"),
            data.ReadUri("callback"));
    }
}
