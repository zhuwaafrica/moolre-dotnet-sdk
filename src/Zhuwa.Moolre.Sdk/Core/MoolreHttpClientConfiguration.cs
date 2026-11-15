namespace Zhuwa.Moolre.Core;

/// <summary>Configures the named <see cref="HttpClient"/> the SDK sends through.</summary>
internal static class MoolreHttpClientConfiguration
{
    /// <summary>The name of the <see cref="HttpClient"/> in <see cref="IHttpClientFactory"/>.</summary>
    internal const string ClientName = "Zhuwa.Moolre";

    /// <summary>The largest response body the SDK reads (16 MB).</summary>
    internal const long MaxResponseBytes = 16 * 1024 * 1024;

    internal static void Configure(HttpClient client, MoolreOptions options)
    {
        // BaseAddress is for tests only (see MoolreOptions); otherwise the environment decides.
        client.BaseAddress = options.BaseAddress ?? new Uri(
            options.Environment == MoolreEnvironment.Live ? MoolreEndpoints.LiveBaseAddress : MoolreEndpoints.SandboxBaseAddress);

        // The transport applies MoolreOptions.Timeout itself so it can tell a timeout from a cancellation.
        client.Timeout = Timeout.InfiniteTimeSpan;
        // Identifies the SDK to Moolre in their logs.
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Zhuwa.Moolre.Sdk");

        // Moolre's answers are small; a limit stops a faulty or hostile server filling memory.
        client.MaxResponseContentBufferSize = MaxResponseBytes;
    }
}
