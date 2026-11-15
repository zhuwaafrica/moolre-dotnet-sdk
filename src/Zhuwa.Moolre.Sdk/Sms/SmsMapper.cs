using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Sms;

/// <summary>Turns Moolre's SMS responses into the public result types. Field names live on the response models (for example <see cref="SenderIdWire"/>).</summary>
internal static class SmsMapper
{
    /// <summary>Maps a send response (<c>SMS01</c>), which carries no data.</summary>
    internal static SmsSendResult ToSmsSendResult(this ResponseEnvelope envelope) =>
        new(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode);

    /// <summary>Maps an SMS status response, whose <c>data</c> is an array of <c>{ ref, status }</c>.</summary>
    internal static SmsStatusResult ToSmsStatusResult(this ResponseEnvelope envelope)
    {
        var statuses = MoolreJson.ReadDataList(envelope.Data, MoolreJsonContext.Default.SmsStatusWire)
            .Select(s => new SmsDeliveryStatus(s.Reference ?? string.Empty, s.Status))
            .ToList();

        return new SmsStatusResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, statuses);
    }

    /// <summary>Maps a create sender IDs response (<c>ASMQ12</c>), which carries no data.</summary>
    internal static SenderIdsCreatedResult ToSenderIdsCreatedResult(this ResponseEnvelope envelope) =>
        new(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode);

    /// <summary>Maps a sender ID status response, whose <c>data</c> is one sender ID.</summary>
    internal static SenderIdStatusResult ToSenderIdStatusResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.SenderIdWire, envelope.HttpStatusCode);

        return new SenderIdStatusResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            wire.SenderId ?? string.Empty,
            ToApproval(wire.Approval),
            wire.IsWhitelisted);
    }

    /// <summary>
    /// Maps a list (<c>ASMQ08</c>) or approve (<c>ASMQ07</c>) response, whose <c>data</c> is an array of sender IDs. Moolre sends the
    /// <c>id</c> as a number in the list and as text in the approve response; both read as text.
    /// </summary>
    internal static SenderIdListResult ToSenderIdListResult(this ResponseEnvelope envelope)
    {
        var senderIds = MoolreJson.ReadDataList(envelope.Data, MoolreJsonContext.Default.SenderIdWire)
            .Select(s => new SenderIdInfo(s.Id ?? string.Empty, s.SenderId ?? string.Empty, ToApproval(s.Approval), s.IsWhitelisted))
            .ToList();

        return new SenderIdListResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, senderIds);
    }

    /// <summary>Maps an SMS account status response, whose <c>data</c> holds the balance.</summary>
    internal static SmsBalanceResult ToSmsBalanceResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.SmsBalanceWire, envelope.HttpStatusCode);

        return new SmsBalanceResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, wire.Balance);
    }

    // Moolre's approval text, kept exactly as received (Moolre can add values); a missing value reads as the default (empty).
    private static SenderIdApproval ToApproval(string? approval) =>
        string.IsNullOrWhiteSpace(approval) ? default : new SenderIdApproval(approval);
}
