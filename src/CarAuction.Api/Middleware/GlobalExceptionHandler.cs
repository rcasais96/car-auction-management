using CarAuction.Application.Exceptions;
using CarAuction.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace CarAuction.Api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
    HttpContext context,
    Exception exception,
    CancellationToken cancellationToken)
        {
            var (statusCode, message) = exception switch
            {
                VehicleNotFoundException e =>
                    (StatusCodes.Status404NotFound, e.Message),

                AuctionNotFoundException e =>
                    (StatusCodes.Status404NotFound, e.Message),

                DuplicateVehicleException e =>
                    (StatusCodes.Status409Conflict, e.Message),

                VehicleAlreadyInActiveAuctionException e =>
                    (StatusCodes.Status409Conflict, e.Message),

                InvalidBidException e =>
                    (StatusCodes.Status400BadRequest, e.Message),

                AuctionNotActiveException e =>
                    (StatusCodes.Status400BadRequest, e.Message),

                AuctionNotStartedException e =>
                    (StatusCodes.Status400BadRequest, e.Message),

                AuctionAlreadyCloseException e =>
                    (StatusCodes.Status400BadRequest, e.Message),

                ArgumentOutOfRangeException e =>
                    (StatusCodes.Status400BadRequest, e.Message),
               
                ArgumentException e =>
                    (StatusCodes.Status400BadRequest, e.Message),

                OperationCanceledException => (499, "Request cancelled by client"),

                _ => (StatusCodes.Status500InternalServerError,
                      "An unexpected error occurred. Please try again later.")
            };

            // Log with appropriate level based on status code
            if (statusCode >= 500)
                _logger.LogError(exception, "Server error occurred: {Message}", exception.Message);
            else if (statusCode >= 400)
                _logger.LogWarning(exception, "Client error occurred: {Message}", exception.Message);
            else
                _logger.LogInformation(exception, "Request handled with status {StatusCode}: {Message}",
                    statusCode, exception.Message);

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message
            }, cancellationToken);

            return true;
        }
    }
}
