using Microsoft.Extensions.Options;

namespace Zhuwa.Moolre.Internal.Options;

/// <summary>Checks <see cref="MoolreOptions"/> when the application starts.</summary>
internal sealed class MoolreOptionsValidator : IValidateOptions<MoolreOptions>
{
    public ValidateOptionsResult Validate(string? name, MoolreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Collect every problem so the developer can fix them all in one go, not one error per restart.
        var failures = new List<string>();

        if (options.Timeout <= TimeSpan.Zero)
        {
            failures.Add("MoolreOptions.Timeout must be greater than zero.");
        }

        // Credentials travel in headers, so a plain http address would send them in clear text.
        if (options.BaseAddress is not null && (!options.BaseAddress.IsAbsoluteUri || options.BaseAddress.Scheme != Uri.UriSchemeHttps))
        {
            failures.Add("MoolreOptions.BaseAddress must be an absolute https address.");
        }

        // Enum.IsDefined is banned in this repo (BannedSymbols.txt), so the valid values are listed in a pattern.
        if (options.Environment is not (MoolreEnvironment.Sandbox or MoolreEnvironment.Live))
        {
            failures.Add("MoolreOptions.Environment must be Sandbox or Live.");
        }

        CheckHeaderValue(failures, nameof(MoolreOptions.ApiUser), options.ApiUser);
        CheckHeaderValue(failures, nameof(MoolreOptions.PrivateKey), options.PrivateKey);
        CheckHeaderValue(failures, nameof(MoolreOptions.PublicKey), options.PublicKey);
        CheckHeaderValue(failures, nameof(MoolreOptions.SmsVasKey), options.SmsVasKey);
        CheckHeaderValue(failures, nameof(MoolreOptions.WhatsAppVasKey), options.WhatsAppVasKey);
        CheckHeaderValue(failures, nameof(MoolreOptions.AccountNumber), options.AccountNumber);

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    // Credentials are sent as HTTP headers, so a line break or other control character would be rejected (or abused).
    private static void CheckHeaderValue(List<string> failures, string name, string? value)
    {
        if (value is not null && value.Any(char.IsControl))
        {
            failures.Add($"MoolreOptions.{name} must not contain control characters such as line breaks.");
        }
    }
}
