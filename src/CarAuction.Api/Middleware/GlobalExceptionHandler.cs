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
                VehicleNotFoundException e => (StatusCodes.Status404NotFound, e.Message),
                AuctionNotFoundException e => (StatusCodes.Status404NotFound, e.Message),
                DuplicateVehicleException e => (StatusCodes.Status409Conflict, e.Message),
                AuctionAlreadyActiveException e => (StatusCodes.Status409Conflict, e.Message),
                AuctionNotActiveException e => (StatusCodes.Status400BadRequest, e.Message),
                AuctionNotStartedException e => (StatusCodes.Status400BadRequest, e.Message),
                InvalidBidException e => (StatusCodes.Status400BadRequest, e.Message),
                ArgumentException e => (StatusCodes.Status400BadRequest, e.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
            };

            _logger.LogError(exception, "Exception caught: {Message}", exception.Message);

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
