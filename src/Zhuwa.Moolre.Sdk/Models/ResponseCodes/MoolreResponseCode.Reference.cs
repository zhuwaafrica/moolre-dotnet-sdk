namespace Zhuwa.Moolre;

// Reference data codes (bank list). The sandbox answers with DA01 where the documentation shows SD01.
// One file per API area keeps this list readable as Moolre adds codes; it is still one type for callers.
public readonly partial struct MoolreResponseCode
{
    /// <summary>Banks found (<c>SD01</c>).</summary>
    public static MoolreResponseCode BanksFound { get; } = new("SD01");

    /// <summary>Banks found alt (<c>DA01</c>).</summary>
    public static MoolreResponseCode BanksFoundAlt { get; } = new("DA01");
}
