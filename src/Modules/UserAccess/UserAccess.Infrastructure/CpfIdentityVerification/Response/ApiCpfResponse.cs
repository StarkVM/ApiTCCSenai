using System.Text.Json.Serialization;

namespace UserAccess.Infrastructure.CpfIdentityVerification.Response;

public class ApiCpfResponse
{
    [JsonPropertyName("code")]
    public int Code { get; init; }
    
    [JsonPropertyName("data")]
    public ApiCpfData? Data{ get; init; }
}

public class ApiCpfData
{
    [JsonPropertyName("cpf")]
    public string Cpf { get; init; } = string.Empty;
    
    [JsonPropertyName("nome")]
    public string Name { get; init; } = string.Empty;
    
    [JsonPropertyName("data_nascimento")]
    public DateOnly BirthDate { get; init; }
}