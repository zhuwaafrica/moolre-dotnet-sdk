namespace Zhuwa.Moolre;

/// <summary>
/// Gives you a <see cref="MoolreClient"/> for one of several wallets. Moolre issues API credentials per wallet (a key for one wallet cannot
/// act on another), so each wallet is registered by name with its own settings, using <c>AddMoolre(name, options =&gt; ...)</c>, and you
/// ask for it here by that name. Inject this class; apps with one wallet can inject <see cref="MoolreClient"/> directly.
/// </summary>
public abstract class MoolreClientFactory
{
    /// <summary>Creates the factory. For subclasses and test doubles.</summary>
    protected MoolreClientFactory()
    {
    }

    /// <summary>Gets the client for a registered wallet. The same name always gives the same client, which is safe to share.</summary>
    /// <param name="name">The name the wallet was registered with.</param>
    /// <returns>A client that sends every call with that wallet's credentials and settings.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No wallet is registered with that name.</exception>
    /// <exception cref="Microsoft.Extensions.Options.OptionsValidationException">The wallet's settings are invalid.</exception>
    public virtual MoolreClient Create(string name) => throw new NotImplementedException();
}
