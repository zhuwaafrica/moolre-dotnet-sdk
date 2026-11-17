namespace Zhuwa.Moolre.Core;

/// <summary>
/// One registered wallet's name, added to dependency injection by each <c>AddMoolre</c> call, so the factory knows which names exist and
/// can refuse an unknown one with a clear error. The wallet registered without a name uses the empty name.
/// </summary>
/// <param name="Name">The wallet's name.</param>
internal sealed record MoolreWalletName(string Name);
