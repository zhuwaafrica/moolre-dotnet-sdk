using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Payments;
using Zhuwa.Moolre.Reference;
using Zhuwa.Moolre.Sales;
using Zhuwa.Moolre.Sms;
using Zhuwa.Moolre.Transfers;
using Zhuwa.Moolre.WhatsApp;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// Builds one <see cref="MoolreClient"/> per registered wallet, each with its own transport reading that wallet's named settings. Clients
/// hold no per-call state, so each is built once and reused.
/// </summary>
internal sealed class DefaultMoolreClientFactory(
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<MoolreOptions> options,
    ILoggerFactory loggerFactory,
    IEnumerable<MoolreWalletName> registeredWallets) : MoolreClientFactory
{
    // The names AddMoolre registered; asking for any other name is a configuration mistake and fails clearly.
    private readonly HashSet<string> _names = new(registeredWallets.Select(w => w.Name), StringComparer.Ordinal);

    private readonly ConcurrentDictionary<string, MoolreClient> _clients = new(StringComparer.Ordinal);

    public override MoolreClient Create(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!_names.Contains(name))
        {
            throw new InvalidOperationException(name.Length == 0
                ? "No default Moolre wallet is registered. Call AddMoolre(options => ...) or ask for a named wallet."
                : $"No Moolre wallet named '{name}' is registered. Call AddMoolre(\"{name}\", options => ...).");
        }

        // Reading the settings here runs their validation, so a wallet with invalid settings fails now, not on its first call.
        _ = options.Get(name);

        return _clients.GetOrAdd(name, Build);
    }

    private MoolreClient Build(string name)
    {
        var transport = new MoolreTransport(httpClientFactory, options, loggerFactory.CreateLogger<MoolreTransport>(), name);

        return new DefaultMoolreClient(
            new DefaultMoolreAccounts(transport),
            new DefaultMoolrePayments(transport),
            new DefaultMoolreSales(transport),
            new DefaultMoolreSms(transport),
            new DefaultMoolreTransfers(transport),
            new DefaultMoolreWhatsApp(transport),
            new DefaultMoolreReferenceData(transport));
    }
}
