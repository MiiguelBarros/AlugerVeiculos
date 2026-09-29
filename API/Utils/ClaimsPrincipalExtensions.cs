using API.Shared.Exceptions;
using System.Security.Claims;

namespace API.Utils
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedException("Sessão inválida. Inicie sessão novamente.");

            int userId = int.Parse(userIdClaim);

            return userId;
        }
    }
}
