using System.Net;
using Grpc.Core;

namespace Gateway.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RpcException rpcEx)
        {
            _logger.LogError(rpcEx, "gRPC error occurred");

            var (statusCode, title) = MapGrpcStatusToHttp(rpcEx.StatusCode);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = title,
                detail = rpcEx.Status.Detail
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Internal Server Error" });
        }
    }
    
    private static (int httpStatus, string title) MapGrpcStatusToHttp(StatusCode grpcStatus)
    {
        return grpcStatus switch
        {
            StatusCode.NotFound          => (StatusCodes.Status404NotFound, "Not Found"),
            StatusCode.PermissionDenied  => (StatusCodes.Status403Forbidden, "Forbidden"),
            StatusCode.Unauthenticated   => (StatusCodes.Status401Unauthorized, "Unauthenticated"),
            StatusCode.DeadlineExceeded  => (StatusCodes.Status504GatewayTimeout, "Timeout"),
            StatusCode.Unavailable       => (StatusCodes.Status503ServiceUnavailable, "Service Unavailable"),
            StatusCode.InvalidArgument   => (StatusCodes.Status400BadRequest, "Invalid Argument"),
            StatusCode.AlreadyExists     => (StatusCodes.Status409Conflict, "Conflict"),
            StatusCode.FailedPrecondition=> (StatusCodes.Status412PreconditionFailed, "Precondition Failed"),
            _                            => (StatusCodes.Status502BadGateway, "gRPC error")
        };
    }
}