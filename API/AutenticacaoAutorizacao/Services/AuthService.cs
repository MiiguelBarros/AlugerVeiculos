using API.AutenticacaoAutorizacao.DTOs;
using API.AutenticacaoAutorizacao.Exceptions;
using API.AutenticacaoAutorizacao.Interfaces;
using API.Data;
using API.Models;
using API.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.AutenticacaoAutorizacao.Services
{
    public class AuthService : IAuthService
    {
        private readonly AlugerVeiculosContext context;
        private readonly IConfiguration config;
        private static readonly string DummyPasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), 12);
        private static readonly TimeSpan ConcurrentRotationGrace = TimeSpan.FromSeconds(30);

        public AuthService(AlugerVeiculosContext context, IConfiguration config)
        {
            this.context = context;
            this.config = config;
        }

        public async Task<AuthSessionResult> LoginAsync(string email, string password)
        {
            var normalizedEmail = NormalizeEmail(email);
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                BCrypt.Net.BCrypt.Verify(password, DummyPasswordHash);
                throw new InvalidCredentialsException("Credenciais inválidas.");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new InvalidCredentialsException("Credenciais inválidas.");

            ValidateUserStatus(user);

            return await GenerateTokensAsync(user);
        }

        public async Task<AuthSessionResult> RefreshAsync(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new InvalidRefreshTokenException("Sessão inválida. Inicie sessão novamente.");

            var tokenHash = HashToken(refreshToken);
            var storedToken = await context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (storedToken == null)
                throw new InvalidRefreshTokenException("Sessão inválida. Inicie sessão novamente.");

            if (storedToken.RevokedAt != null)
            {
                //Quando há dois pedidos em simultâneo
                var concurrent = await ConcurrentRotationAsync(storedToken);
                if (concurrent != null)
                {
                    ValidateUserStatus(concurrent.User);
                    return new AuthSessionResult(GenerateAccessToken(concurrent.User, concurrent.RefreshTokenId), null, concurrent.ExpiresAt);
                }

                //Quando é reutilizado fora da janela, tem de ser revogado
                await RevokeDescendantTokensAsync(storedToken);
                throw new InvalidRefreshTokenException("Sessão inválida. Inicie sessão novamente.");
            }

            if (storedToken.IsExpired)
                throw new InvalidRefreshTokenException("A sessão expirou. Inicie sessão novamente.");

            ValidateUserStatus(storedToken.User);

            var rawNewToken = GenerateRefreshTokenValue();
            var newToken = new RefreshToken
            {
                UserId = storedToken.UserId,
                TokenHash = HashToken(rawNewToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.ReplacedByTokenHash = newToken.TokenHash;

            await context.RefreshTokens.AddAsync(newToken);
            await context.SaveChangesAsync();

            var accessToken = GenerateAccessToken(storedToken.User, newToken.RefreshTokenId);

            return new AuthSessionResult(accessToken, rawNewToken, newToken.ExpiresAt);
        }

        public async Task LogoutAsync(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return;

            var tokenHash = HashToken(refreshToken);
            var storedToken = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (storedToken == null || storedToken.RevokedAt != null)
                return;

            storedToken.RevokedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        public async Task VerifyJwtTokenIdPresenceAsync(TokenValidatedContext context)
        {
            var token = context.Principal?.FindFirst(ClaimTypes.SerialNumber)?.Value;
            if (string.IsNullOrEmpty(token))
            {
                context.Fail($"{ClaimTypes.SerialNumber} not found");
                return;
            }

            if (!long.TryParse(token, out long tokenId))
            {
                context.Fail($"Failed to parse {ClaimTypes.SerialNumber}");
                return;
            }

            if (!await IsSessionActiveAsync(tokenId))
                context.Fail($"{ClaimTypes.SerialNumber} not valid");
        }

        private async Task<bool> IsSessionActiveAsync(long refreshTokenId)
        {
            return await context.RefreshTokens.AnyAsync(rt =>
                rt.RefreshTokenId == refreshTokenId &&
                rt.RevokedAt == null &&
                rt.ExpiresAt > DateTime.UtcNow &&
                rt.User.Status == RecordStatus.Active);
        }

        private async Task<RefreshToken?> ConcurrentRotationAsync(RefreshToken revokedToken)
        {
            if (revokedToken.RevokedAt == null || string.IsNullOrEmpty(revokedToken.ReplacedByTokenHash)
                || DateTime.UtcNow - revokedToken.RevokedAt.Value > ConcurrentRotationGrace)
                return null;

            var replacement = await context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == revokedToken.ReplacedByTokenHash);

            if (replacement == null || replacement.RevokedAt != null || replacement.IsExpired)
                return null;

            return replacement;
        }

        private async Task RevokeDescendantTokensAsync(RefreshToken compromisedToken)
        {
            var currentHash = compromisedToken.ReplacedByTokenHash;

            while (!string.IsNullOrEmpty(currentHash))
            {
                var descendant = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == currentHash);
                if (descendant == null)
                    break;

                if (descendant.RevokedAt == null)
                    descendant.RevokedAt = DateTime.UtcNow;

                currentHash = descendant.ReplacedByTokenHash;
            }

            await context.SaveChangesAsync();
        }

        private static void ValidateUserStatus(User user)
        {
            if (user.Status != RecordStatus.Active)
                throw new InactiveAccountException("A sua conta está inativa. Contacte o gestor.");
        }

        private async Task<AuthSessionResult> GenerateTokensAsync(User user)
        {
            var rawRefreshToken = GenerateRefreshTokenValue();
            var refreshToken = new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = HashToken(rawRefreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await context.RefreshTokens.AddAsync(refreshToken);
            await context.SaveChangesAsync();

            var accessToken = GenerateAccessToken(user, refreshToken.RefreshTokenId);

            return new AuthSessionResult(accessToken, rawRefreshToken, refreshToken.ExpiresAt);
        }

        private string GenerateAccessToken(User user, long refreshTokenId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.SerialNumber, refreshTokenId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["JwtSettings:Issuer"],
                audience: config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        private static string GenerateRefreshTokenValue()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static string HashToken(string token)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
