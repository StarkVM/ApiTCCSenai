using System.Text.Json;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using UserAccess.Domain.Interfaces;
using UserAccess.Infrastructure.IdentityVerification.Didit.Options;

namespace UserAccess.Infrastructure.IdentityVerification.Didit.SignatureValidator;

public class DiditWebhookSignatureValidator : IIdentityVerificationWebhookAuthenticator
{
    private const long AllowedTimestampDifferenceInSeconds = 300;
    private readonly string _webhookSecret;

    public DiditWebhookSignatureValidator(
        IOptions<DiditOptions> options
        )
    {
        
        _webhookSecret = options.Value.WebhookSecret;

        if (string.IsNullOrWhiteSpace(_webhookSecret))
        {
            throw new InvalidOperationException("Didit WebhookSecret is not configured.");
        }
    }
    public Task<bool> IsAuthentic(string rawBody, string? signatureV2, string? signatureSimple, string? timestamp)
    {
        if (string.IsNullOrWhiteSpace(rawBody) ||
            string.IsNullOrWhiteSpace(timestamp))
        {
            return Task.FromResult(false);
        }

        if (!TimestampIsFresh(timestamp))
        {
            return Task.FromResult(false);
        }

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(rawBody);
        }
        catch (JsonException)
        {
            return Task.FromResult(false);
        }
        
        using (document)
        {
            if (!string.IsNullOrWhiteSpace(signatureV2) &&
                IsSignatureV2Valid(
                    document.RootElement,
                    signatureV2))
            {
                return Task.FromResult(true);
            }

            if (!string.IsNullOrWhiteSpace(signatureSimple) &&
                IsSignatureSimpleValid(
                    document.RootElement,
                    signatureSimple))
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }
    
    private static bool TimestampIsFresh(string timestamp)
    {
        if (!long.TryParse(timestamp, out var webhookTimestamp))
        {
            return false;
        }

        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var difference = Math.Abs(currentTimestamp - webhookTimestamp);

        return difference <= AllowedTimestampDifferenceInSeconds;
    }
    
    private bool IsSignatureV2Valid(
        JsonElement jsonBody,
        string providedSignature)
    {
        var canonicalJson = BuildCanonicalJson(jsonBody);

        var expectedSignature = ComputeHmacSha256Hex(canonicalJson);

        return FixedTimeEquals(
            expectedSignature,
            providedSignature);
    }

    
    private bool IsSignatureSimpleValid(
        JsonElement jsonBody,
        string providedSignature)
    {
        var bodyTimestamp = GetJsonScalarAsString(jsonBody, "timestamp");
        var sessionId = GetJsonScalarAsString(jsonBody, "session_id");
        var status = GetJsonScalarAsString(jsonBody, "status");
        var webhookType = GetJsonScalarAsString(jsonBody, "webhook_type");

        var canonicalString = string.Join(
            ":",
            bodyTimestamp,
            sessionId,
            status,
            webhookType);

        var expectedSignature = ComputeHmacSha256Hex(canonicalString);

        return FixedTimeEquals(
            expectedSignature,
            providedSignature);
    }

    
    private static string BuildCanonicalJson(JsonElement element)
    {
        var buffer = new ArrayBufferWriter<byte>();

        using var writer = new Utf8JsonWriter(
            buffer,
            new JsonWriterOptions
            {
                Indented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        WriteCanonicalJson(writer, element);
        writer.Flush();

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    
    private static void WriteCanonicalJson(
        Utf8JsonWriter writer,
        JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();

                foreach (var property in element
                             .EnumerateObject()
                             .OrderBy(property => property.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonicalJson(writer, property.Value);
                }

                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();

                foreach (var item in element.EnumerateArray())
                {
                    WriteCanonicalJson(writer, item);
                }

                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;

            case JsonValueKind.Number:
                WriteCanonicalNumber(writer, element);
                break;

            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;

            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;

            default:
                writer.WriteNullValue();
                break;
        }
    }

    
    private static void WriteCanonicalNumber(
        Utf8JsonWriter writer,
        JsonElement element)
    {
        if (element.TryGetInt64(out var int64Value))
        {
            writer.WriteNumberValue(int64Value);
            return;
        }

        if (element.TryGetUInt64(out var uint64Value))
        {
            writer.WriteNumberValue(uint64Value);
            return;
        }

        var doubleValue = element.GetDouble();

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (double.IsFinite(doubleValue) &&
            Math.Truncate(doubleValue) == doubleValue &&
            doubleValue >= long.MinValue &&
            doubleValue <= long.MaxValue)
        {
            writer.WriteNumberValue((long)doubleValue);
            return;
        }

        writer.WriteNumberValue(doubleValue);
    }

   
    private static string GetJsonScalarAsString(
        JsonElement jsonBody,
        string propertyName)
    {
        if (!jsonBody.TryGetProperty(propertyName, out var property))
        {
            return string.Empty;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString() ?? string.Empty,
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => string.Empty
        };
    }

    
    private string ComputeHmacSha256Hex(string message)
    {
        var secretBytes = Encoding.UTF8.GetBytes(_webhookSecret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(secretBytes);

        var hashBytes = hmac.ComputeHash(messageBytes);

        return Convert
            .ToHexString(hashBytes)
            .ToLowerInvariant();
    }
    
    private static bool FixedTimeEquals(
        string expectedSignature,
        string providedSignature)
    {
        var normalizedExpected = expectedSignature.Trim().ToLowerInvariant();
        var normalizedProvided = providedSignature.Trim().ToLowerInvariant();

        var expectedBytes = Encoding.UTF8.GetBytes(normalizedExpected);
        var providedBytes = Encoding.UTF8.GetBytes(normalizedProvided);

        if (expectedBytes.Length != providedBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            expectedBytes,
            providedBytes);
    }
}