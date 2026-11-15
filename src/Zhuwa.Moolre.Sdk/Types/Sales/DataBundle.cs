namespace Zhuwa.Moolre;

/// <summary>Data bundle.</summary>
public class DataBundle
{
    internal DataBundle(
        string bundleId,
        decimal price,
        string label,
        double megabytes,
        string validity,
        string category)
    {
        BundleId = bundleId;
        Price = price;
        Label = label;
        Megabytes = megabytes;
        Validity = validity;
        Category = category;
    }

    /// <summary>Bundle ID. Moolre field <c>bundle</c>.</summary>
    public string BundleId { get; }

    /// <summary>Price. Moolre field <c>price</c> (string, GHS).</summary>
    public decimal Price { get; }

    /// <summary>Label. Moolre field <c>data</c> (e.g. "20.73 MB").</summary>
    public string Label { get; }

    /// <summary>Megabytes. Moolre field <c>data_mb</c>.</summary>
    public double Megabytes { get; }

    /// <summary>Validity. Moolre field <c>validity</c> (e.g. "No Expiry").</summary>
    public string Validity { get; }

    /// <summary>Category. Moolre field <c>category</c>.</summary>
    public string Category { get; }
}
