namespace Api.Common.Errors;

public class ApiErrorResponse
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;
    public string RequestId { get; init; } = default!;
}