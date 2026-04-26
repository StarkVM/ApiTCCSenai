using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using UserAccess.Domain.Helpers;
using UserAccess.Domain.Interfaces;
using UserAccess.Infrastructure.CpfIdentityVerification.Options;
using UserAccess.Infrastructure.CpfIdentityVerification.Response;

namespace UserAccess.Infrastructure.CpfIdentityVerification;

public class CpfValidator : ICpfValidator
{
    private readonly HttpClient _httpClient;
    private readonly ApiCpfOptions _options;

    public CpfValidator(HttpClient httpClient, IOptions<ApiCpfOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }
    
    public async Task<bool> ValidateAsync(string cpf, string fullName, DateOnly birthDate, CancellationToken cancellationToken)
    {
        var cleanCpf = cpf.Clean();

        if (!cleanCpf.CpfIsValid())
        {
            return false;
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/consulta?cpf={cleanCpf}"
        );

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Unexpected status code: {(int)response.StatusCode} ({response.StatusCode})");
        }
        
        var result = await response.Content.ReadFromJsonAsync<ApiCpfResponse>(cancellationToken: cancellationToken);

        if (result?.Data is null)
        {
            return false;
        }
        
        return 
            result.Data.Cpf.Clean() == cleanCpf &&
            Normalize(result.Data.Name) == Normalize(fullName) &&
            result.Data.BirthDate == birthDate;
    }

    private static string Normalize(string name)
    {
        return name.Trim().ToUpperInvariant();
    }
}