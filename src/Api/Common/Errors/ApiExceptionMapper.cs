using UserAccess.Application.Common.Exceptions;
using UserAccess.Domain.Exceptions;
using UserAccess.Domain.Exceptions.UserAccessExceptions;

namespace Api.Common.Errors;

/// <summary>
/// Maps exceptions to HTTP responses.
/// / Mapeia exceções para respostas HTTP.
/// </summary>
public static class ApiExceptionMapper
{
    public static IResult Map(Exception exception, HttpContext httpContext)
    {
        var requestId = httpContext.TraceIdentifier;

        return exception switch
        {
            
            InvalidCredentialsException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),

            EmailVerificationFailedException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),

            EmailAndCpfConflictException ex => Results.Json(
                statusCode: StatusCodes.Status409Conflict,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),

            EmailOrCpfConflictException ex => Results.Json(
                statusCode: StatusCodes.Status409Conflict,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),

            RegistrationInProgressException ex => Results.Json(
                statusCode: StatusCodes.Status409Conflict,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),

            RefreshTokenNotFoundException ex => Results.Json(
                statusCode: StatusCodes.Status404NotFound,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),
            
            RefreshTokenNotActiveException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),
            
            InvalidGuidIdException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),
            
            WebhookInvalidPayloadException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),
            
            WebhookInvalidSignatureException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),

            RefreshTokenRequiredException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),
            UserEmailMustBeVerifiedToDeleteException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),
            UserMustBeActiveToBecomeProviderException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),

            InvalidUserIdException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),
            
            InvalidUserException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),
            
            
            
            CpfValidationFailedException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),
            
            AddressNotFoundException ex => Results.Json(
                statusCode: StatusCodes.Status404NotFound,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),

            UserNotFoundException ex => Results.Json(
                statusCode: StatusCodes.Status404NotFound,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),

            DatabaseSaveFailedException => Results.Json(
                statusCode: StatusCodes.Status500InternalServerError,
                data: new ApiErrorResponse
                {
                    Code = "DB_SAVE_FAILED",
                    Message = "Database save failed.",
                    RequestId = requestId
                }),

            EmailSendFailedException => Results.Json(
                statusCode: StatusCodes.Status500InternalServerError,
                data: new ApiErrorResponse
                {
                    Code = "EMAIL_SEND_FAILED",
                    Message = "Failed to send email.",
                    RequestId = requestId
                }),

            ArgumentException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = "BAD_REQUEST",
                    Message = ex.Message,
                    RequestId = requestId
                }),

            AppException ex => Results.Json(
                statusCode: StatusCodes.Status400BadRequest,
                data: new ApiErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message,
                    RequestId = requestId
                }),

            _ => Results.Json(
                statusCode: StatusCodes.Status500InternalServerError,
                data: new ApiErrorResponse
                {
                    Code = "INTERNAL_SERVER_ERROR",
                    Message = exception.Message,
                    RequestId = requestId
                })
        };
    }
}