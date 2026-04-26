namespace UserAccess.Infrastructure.CpfIdentityVerification.Options;

public class ApiCpfOptions
{
    public string BaseUrl { get; init; } = "https://apicpf.com";
    public string ApiKey { get; init; } = string.Empty;
}

