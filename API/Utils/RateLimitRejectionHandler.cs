using API.Shared.DTOs;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;
using System.Threading.RateLimiting;

namespace API.Utils
{
    public static class RateLimitRejectionHandler
    {
        public static async ValueTask OnRejected(OnRejectedContext context, CancellationToken cancellationToken)
        {
            var httpContext = context.HttpContext;
            var message = "Demasiadas tentativas. Aguarde um minuto e tente novamente.";

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                var seconds = (int)Math.Ceiling(retryAfter.TotalSeconds);
                httpContext.Response.Headers.RetryAfter = seconds.ToString(NumberFormatInfo.InvariantInfo);
                message = $"Demasiadas tentativas. Tente novamente dentro de {seconds} segundos.";
            }

            await httpContext.Response.WriteAsJsonAsync(ResponseDTO<string>.Fail(message, StatusCodes.Status429TooManyRequests), cancellationToken);
        }
    }
}
