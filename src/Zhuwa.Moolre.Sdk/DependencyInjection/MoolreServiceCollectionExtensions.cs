using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Payments;
using Zhuwa.Moolre.Core;
using Zhuwa.Moolre.Reference;
using Zhuwa.Moolre.Sales;
using Zhuwa.Moolre.Sms;
using Zhuwa.Moolre.Transfers;
using Zhuwa.Moolre.Webhooks;

namespace Zhuwa.Moolre.DependencyInjection;

/// <summary>Registers the Moolre SDK with dependency injection.</summary>
public static class MoolreServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="MoolreClient"/> and its settings. The settings are checked when the application starts.
    /// The SDK sends through a named <see cref="HttpClient"/> (<c>Zhuwa.Moolre</c>), so you can add handlers to it with
    /// <c>services.AddHttpClient("Zhuwa.Moolre")</c>. Do not retry <c>POST</c> requests blindly: every Moolre call is a POST.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the environment, credentials and default wallet. Read secrets from configuration or a secret store.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddMoolre(this IServiceCollection services, Action<MoolreOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        // ValidateOnStart makes a bad configuration fail when the host starts, not on the first payment.
        services.AddOptions<MoolreOptions>().Configure(configure).ValidateOnStart();
        // TryAddEnumerable keeps this to one validator if AddMoolre is called twice.
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<MoolreOptions>, MoolreOptionsValidator>());

        // One named HttpClient for every Moolre call, so the application can add handlers (logging, a proxy) to it by name.
        services
            .AddHttpClient(MoolreHttpClientConfiguration.ClientName)
            .ConfigureHttpClient((provider, client) =>
                MoolreHttpClientConfiguration.Configure(client, provider.GetRequiredService<IOptionsMonitor<MoolreOptions>>().CurrentValue))
            // The credential headers are custom headers, which .NET keeps when it follows a redirect to another host.
            // Moolre does not redirect its API, so a redirect is treated as a failure instead of followed.
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false });

        // Singletons: the transport and area clients hold no per-call state, and the HttpClient is created per call by the factory.
        // TryAdd lets an application replace any of them (for example with a test double) by registering its own first.
        services.TryAddSingleton<MoolreTransport>();
        services.TryAddSingleton<MoolreAccounts, DefaultMoolreAccounts>();
        services.TryAddSingleton<MoolrePayments, DefaultMoolrePayments>();
        services.TryAddSingleton<MoolreSales, DefaultMoolreSales>();
        services.TryAddSingleton<MoolreSms, DefaultMoolreSms>();
        services.TryAddSingleton<MoolreTransfers, DefaultMoolreTransfers>();
        services.TryAddSingleton<MoolreReferenceData, DefaultMoolreReferenceData>();
        services.TryAddSingleton<MoolreClient, DefaultMoolreClient>();

        return services;
    }

    /// <summary>
    /// Registers <see cref="MoolreWebhookVerifier"/> for your payment callback endpoint. The secret is checked when the application starts.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the callback secret. Read it from configuration or a secret store.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddMoolreWebhooks(this IServiceCollection services, Action<MoolreWebhookOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        // Fail at startup, not on the first callback: an endpoint that cannot verify callbacks must not run.
        services.AddOptions<MoolreWebhookOptions>()
            .Configure(configure)
            .Validate(options => !string.IsNullOrEmpty(options.Secret), "MoolreWebhookOptions.Secret is required to verify callbacks.")
            .ValidateOnStart();

        services.TryAddSingleton<MoolreWebhookVerifier, DefaultMoolreWebhookVerifier>();

        return services;
    }
}
