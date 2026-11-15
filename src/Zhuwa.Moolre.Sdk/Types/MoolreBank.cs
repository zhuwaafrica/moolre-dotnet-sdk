namespace Zhuwa.Moolre;

/// <summary>Moolre bank.</summary>
public class MoolreBank
{
    internal MoolreBank(
        string name,
        string code)
    {
        Name = name;
        Code = code;
    }

    /// <summary>Name. Moolre field <c>name</c>.</summary>
    public string Name { get; }

    /// <summary>Code. Moolre field <c>code</c> (use as BankCode / sublistid).</summary>
    public string Code { get; }
}
