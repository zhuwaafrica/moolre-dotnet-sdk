using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Zhuwa.Moolre.Core;
using Zhuwa.Moolre.Webhooks;

namespace Zhuwa.Moolre.DependencyInjection;

/// <summary>Registers the Moolre SDK with dependency injection.</summary>
public static class MoolreServiceCollectionExtensions
{
    /// <summary>
    /// Registers one wallet as the default <see cref="MoolreClient"/>. Inject <see cref="MoolreClient"/> (or one of its areas, such as
    /// <c>MoolrePayments</c>) to use it. The settings are checked when the application starts. The SDK sends through a named
    /// <see cref="HttpClient"/> (<c>Zhuwa.Moolre</c>), so you can add handlers to it with <c>services.AddHttpClient("Zhuwa.Moolre")</c>.
    /// Do not retry <c>POST</c> requests blindly: every Moolre call is a POST.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the environment, credentials and wallet. Read secrets from configuration or a secret store.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddMoolre(this IServiceCollection services, Action<MoolreOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        AddWallet(services, Options.DefaultName, configure);

        // The default wallet is also available directly, for apps with one wallet. Each area is the default client's own area, so
        // injecting MoolrePayments and MoolreClient.Payments gives the same object. TryAdd lets an application replace any of them
        // (for example with a test double) by registering its own first.
        services.TryAddSingleton(provider => provider.GetRequiredService<MoolreClientFactory>().Create(Options.DefaultName));
        services.TryAddSingleton(provider => provider.GetRequiredService<MoolreClient>().Accounts);
        services.TryAddSingleton(provider => provider.GetRequiredService<MoolreClient>().Payments);
        services.TryAddSingleton(provider => provider.GetRequiredService<MoolreClient>().Sales);
        services.TryAddSingleton(provider => provider.GetRequiredService<MoolreClient>().Sms);
        services.TryAddSingleton(provider => provider.GetRequiredService<MoolreClient>().Transfers);
        services.TryAddSingleton(provider => provider.GetRequiredService<MoolreClient>().WhatsApp);
        services.TryAddSingleton(provider => provider.GetRequiredService<MoolreClient>().ReferenceData);

        return services;
    }

    /// <summary>
    /// Registers a named wallet. Moolre issues credentials per wallet (a key for one wallet cannot act on another), so a business with
    /// several wallets registers each one by name with its own settings, then injects <see cref="MoolreClientFactory"/> and calls
    /// <c>Create(name)</c>. The settings are checked when the application starts. Each wallet sends through its own named
    /// <see cref="HttpClient"/>, <c>Zhuwa.Moolre:&lt;name&gt;</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The wallet's name, used with <see cref="MoolreClientFactory.Create"/>.</param>
    /// <param name="configure">Sets this wallet's environment, credentials and account number.</param>
    /// <returns>The same service collection.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null, empty or white space.</exception>
    public static IServiceCollection AddMoolre(this IServiceCollection services, string name, Action<MoolreOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(configure);

        AddWallet(services, name, configure);
        return services;
    }

    // Registers one wallet: its named settings (checked at startup), its own HttpClient and its name for the factory.
    private static void AddWallet(IServiceCollection services, string name, Action<MoolreOptions> configure)
    {
        // ValidateOnStart makes a bad configuration fail when the host starts, not on the first payment.
        services.AddOptions<MoolreOptions>(name).Configure(configure).ValidateOnStart();

        // TryAddEnumerable keeps these to one each however many wallets are registered.
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<MoolreOptions>, MoolreOptionsValidator>());
        services.TryAddSingleton<MoolreClientFactory, DefaultMoolreClientFactory>();
        services.AddSingleton(new MoolreWalletName(name));

        // One named HttpClient per wallet, configured from that wallet's settings, so the application can add handlers to it by name.
        services
            .AddHttpClient(MoolreHttpClientConfiguration.ClientNameFor(name))
            .ConfigureHttpClient((provider, client) =>
                MoolreHttpClientConfiguration.Configure(client, provider.GetRequiredService<IOptionsMonitor<MoolreOptions>>().Get(name)))
            // The credential headers are custom headers, which .NET keeps when it follows a redirect to another host.
            // Moolre does not redirect its API, so a redirect is treated as a failure instead of followed.
            // Authentication uses headers, so the pooled handler does not need to store or replay cookies.
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false, UseCookies = false });
    }

    /// <summary>
    /// Registers <see cref="MoolreWebhookVerifier"/> for your payment callback endpoint. The settings are checked when the application starts.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// Sets the callback secret (<see cref="MoolreWebhookOptions.Secret"/>) or one per wallet (<see cref="MoolreWebhookOptions.WalletSecrets"/>).
    /// Read them from configuration or a secret store.
    /// </param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddMoolreWebhooks(this IServiceCollection services, Action<MoolreWebhookOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        // Fail at startup, not on the first callback: an endpoint that cannot verify callbacks must not run.
        services.AddOptions<MoolreWebhookOptions>()
            .Configure(configure)
            .Validate(
                options => !string.IsNullOrEmpty(options.Secret) || options.WalletSecrets.Count > 0,
                "Set MoolreWebhookOptions.Secret or at least one entry in MoolreWebhookOptions.WalletSecrets to verify callbacks.")
            .Validate(
                options => options.WalletSecrets.All(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrEmpty(pair.Value)),
                "Every MoolreWebhookOptions.WalletSecrets entry needs a wallet account number and a secret.")
            .Validate(
                options => options.SignatureTolerance > TimeSpan.Zero,
                "MoolreWebhookOptions.SignatureTolerance must be greater than zero.")
            .ValidateOnStart();

        // The signature check compares Moolre's timestamp with the current time. TryAdd keeps a TimeProvider the application (or a test)
        // has registered already, the same way ASP.NET Core and Microsoft.Extensions.Caching register it.
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<MoolreWebhookVerifier, DefaultMoolreWebhookVerifier>();

        return services;
    }
}
