using API.AutenticacaoAutorizacao.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.AutenticacaoAutorizacao.Interfaces
{
    public interface IAuthService
    {
        public Task<AuthSessionResult> LoginAsync(string email, string password);

        public Task<AuthSessionResult> RefreshAsync(string? refreshToken);

        public Task LogoutAsync(string? refreshToken);

        public Task VerifyJwtTokenIdPresenceAsync(TokenValidatedContext context);
    }
}
