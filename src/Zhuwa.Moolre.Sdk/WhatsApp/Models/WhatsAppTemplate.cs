namespace Zhuwa.Moolre.WhatsApp;

/// <summary>WhatsApp template.</summary>
public class WhatsAppTemplate
{
    internal WhatsAppTemplate(
        string id,
        string name,
        string language,
        WhatsAppTemplateStatus status,
        string body,
        IReadOnlyList<string> placeholders)
    {
        Id = id;
        Name = name;
        Language = language;
        Status = status;
        Body = body;
        Placeholders = placeholders;
    }

    /// <summary>ID. Moolre field <c>id</c>.</summary>
    public string Id { get; }

    /// <summary>Name. Moolre field <c>name</c>.</summary>
    public string Name { get; }

    /// <summary>Language. Moolre field <c>language</c> ("en").</summary>
    public string Language { get; }

    /// <summary>Status. Moolre field <c>status</c>.</summary>
    public WhatsAppTemplateStatus Status { get; }

    /// <summary>Body. Moolre field <c>message</c> (with {{1}} placeholders).</summary>
    public string Body { get; }

    /// <summary>Placeholders. Moolre field <c>placeholders</c> (["1","2"]).</summary>
    public IReadOnlyList<string> Placeholders { get; }
}
