using API.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Utils
{
    public static class JwtBearerEventHandlers
    {
        public static async Task OnChallenge(JwtBearerChallengeContext context)
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(
                ResponseDTO<string>.Fail("Sessão inválida ou expirada. Inicie sessão novamente.", StatusCodes.Status401Unauthorized));
        }

        public static async Task OnForbidden(ForbiddenContext context)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            await context.Response.WriteAsJsonAsync(
                ResponseDTO<string>.Fail("Não tem permissão para realizar esta ação.", StatusCodes.Status403Forbidden));
        }
    }
}
