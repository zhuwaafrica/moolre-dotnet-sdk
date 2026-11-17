namespace Zhuwa.Moolre;

/// <summary>Result of the bundle catalog operation.</summary>
public class BundleCatalogResult : MoolreResult
{
    internal BundleCatalogResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        IReadOnlyList<DataBundle> mtn,
        IReadOnlyList<DataBundle> telecel,
        IReadOnlyList<DataBundle> at)
        : base(code, message, messages, httpStatusCode)
    {
        Mtn = mtn;
        Telecel = telecel;
        At = at;
    }

    /// <summary>Mtn. Moolre field <c>data.mtn</c>.</summary>
    public IReadOnlyList<DataBundle> Mtn { get; }

    /// <summary>Telecel. Moolre field <c>data.telecel</c>.</summary>
    public IReadOnlyList<DataBundle> Telecel { get; }

    /// <summary>At. Moolre field <c>data.at</c>.</summary>
    public IReadOnlyList<DataBundle> At { get; }
}
