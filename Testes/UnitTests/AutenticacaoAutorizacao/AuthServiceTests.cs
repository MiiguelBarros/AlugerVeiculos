using API.AutenticacaoAutorizacao.Exceptions;
using API.AutenticacaoAutorizacao.Services;
using API.Models.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UnitTests.Infrastructure;

namespace UnitTests.AutenticacaoAutorizacao
{
    public class AuthServiceTests : IDisposable
    {
        private const string JwtKey = "chave-de-teste-com-pelo-menos-32-caracteres!";
        private const string JwtIssuer = "AlugerVeiculos.Tests";
        private const string JwtAudience = "AlugerVeiculos.Tests";

        private readonly TestDatabase database = new();

        private readonly IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Key"] = JwtKey,
                ["JwtSettings:Issuer"] = JwtIssuer,
                ["JwtSettings:Audience"] = JwtAudience
            })
            .Build();

        private AuthService CreateService() => new(database.CreateContext(), config);

        private static TokenValidatedContext CreateTokenValidatedContext(long refreshTokenId)
        {
            var scheme = new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, null, typeof(JwtBearerHandler));
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.SerialNumber, refreshTokenId.ToString()) }, JwtBearerDefaults.AuthenticationScheme);

            return new TokenValidatedContext(new DefaultHttpContext(), scheme, new JwtBearerOptions())
            {
                Principal = new ClaimsPrincipal(identity)
            };
        }

        public void Dispose() => database.Dispose();

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAccessAndRefreshToken()
        {
            database.Seed(TestData.CreateUser());

            var session = await CreateService().LoginAsync("manager@test.com", TestData.Password);

            Assert.False(string.IsNullOrEmpty(session.AccessToken));
            Assert.False(string.IsNullOrEmpty(session.RefreshToken));
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_AccessTokenContainsUserClaims()
        {
            var user = TestData.CreateUser();
            database.Seed(user);

            var session = await CreateService().LoginAsync("manager@test.com", TestData.Password);

            var principal = new JwtSecurityTokenHandler().ValidateToken(session.AccessToken, new TokenValidationParameters
            {
                ValidIssuer = JwtIssuer,
                ValidAudience = JwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey))
            }, out _);

            using var context = database.CreateContext();
            var refreshTokenId = context.RefreshTokens.Single().RefreshTokenId;

            Assert.Equal(user.UserId.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.Equal(user.Name, principal.FindFirstValue(ClaimTypes.Name));
            Assert.Equal(nameof(Role.Manager), principal.FindFirstValue(ClaimTypes.Role));
            Assert.Equal(refreshTokenId.ToString(), principal.FindFirstValue(ClaimTypes.SerialNumber));
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentialsException()
        {
            database.Seed(TestData.CreateUser());

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => CreateService().LoginAsync("manager@test.com", "Wrong123@"));
        }

        [Fact]
        public async Task LoginAsync_UnknownEmail_ThrowsInvalidCredentialsException()
        {
            database.Seed(TestData.CreateUser());

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => CreateService().LoginAsync("unknown@test.com", TestData.Password));
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_ThrowsInactiveAccountException()
        {
            database.Seed(TestData.CreateUser(status: RecordStatus.Inactive));

            await Assert.ThrowsAsync<InactiveAccountException>(() => CreateService().LoginAsync("manager@test.com", TestData.Password));
        }

        [Fact]
        public async Task RefreshAsync_ValidToken_RotatesRefreshToken()
        {
            database.Seed(TestData.CreateUser());
            var login = await CreateService().LoginAsync("manager@test.com", TestData.Password);

            var refreshed = await CreateService().RefreshAsync(login.RefreshToken);

            using var context = database.CreateContext();
            var oldToken = context.RefreshTokens.Single(rt => rt.ReplacedByTokenHash != null);
            var newToken = context.RefreshTokens.Single(rt => rt.ReplacedByTokenHash == null);

            Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);
            Assert.NotNull(oldToken.RevokedAt);
            Assert.Equal(newToken.TokenHash, oldToken.ReplacedByTokenHash);
            Assert.Null(newToken.RevokedAt);
        }

        [Fact]
        public async Task RefreshAsync_ReusedTokenOutsideGracePeriod_RevokesTokenChain()
        {
            database.Seed(TestData.CreateUser());
            var login = await CreateService().LoginAsync("manager@test.com", TestData.Password);
            await CreateService().RefreshAsync(login.RefreshToken);

            using (var context = database.CreateContext())
            {
                context.RefreshTokens.Single(rt => rt.ReplacedByTokenHash != null).RevokedAt = DateTime.UtcNow.AddMinutes(-1);
                context.SaveChanges();
            }

            await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => CreateService().RefreshAsync(login.RefreshToken));

            using var assertContext = database.CreateContext();
            Assert.All(assertContext.RefreshTokens, rt => Assert.NotNull(rt.RevokedAt));
        }

        [Fact]
        public async Task RefreshAsync_ReusedTokenWithinGracePeriod_ReturnsAccessTokenWithoutNewRefreshToken()
        {
            database.Seed(TestData.CreateUser());
            var login = await CreateService().LoginAsync("manager@test.com", TestData.Password);
            await CreateService().RefreshAsync(login.RefreshToken);

            var concurrent = await CreateService().RefreshAsync(login.RefreshToken);

            Assert.False(string.IsNullOrEmpty(concurrent.AccessToken));
            Assert.Null(concurrent.RefreshToken);
        }

        [Fact]
        public async Task RefreshAsync_ExpiredToken_ThrowsInvalidRefreshTokenException()
        {
            database.Seed(TestData.CreateUser());
            var login = await CreateService().LoginAsync("manager@test.com", TestData.Password);

            using (var context = database.CreateContext())
            {
                context.RefreshTokens.Single().ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
                context.SaveChanges();
            }

            await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => CreateService().RefreshAsync(login.RefreshToken));
        }

        [Fact]
        public async Task RefreshAsync_MissingToken_ThrowsInvalidRefreshTokenException()
        {
            await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => CreateService().RefreshAsync(null));
        }

        [Fact]
        public async Task LogoutAsync_ValidToken_RevokesSession()
        {
            database.Seed(TestData.CreateUser());
            var login = await CreateService().LoginAsync("manager@test.com", TestData.Password);

            await CreateService().LogoutAsync(login.RefreshToken);

            using var context = database.CreateContext();
            Assert.NotNull(context.RefreshTokens.Single().RevokedAt);
        }

        [Fact]
        public async Task VerifyJwtTokenIdPresenceAsync_ActiveSession_Succeeds()
        {
            database.Seed(TestData.CreateUser());
            await CreateService().LoginAsync("manager@test.com", TestData.Password);
            using var context = database.CreateContext();
            var tokenValidatedContext = CreateTokenValidatedContext(context.RefreshTokens.Single().RefreshTokenId);

            await CreateService().VerifyJwtTokenIdPresenceAsync(tokenValidatedContext);

            Assert.Null(tokenValidatedContext.Result);
        }

        [Fact]
        public async Task VerifyJwtTokenIdPresenceAsync_RevokedSession_Fails()
        {
            database.Seed(TestData.CreateUser());
            var login = await CreateService().LoginAsync("manager@test.com", TestData.Password);
            await CreateService().LogoutAsync(login.RefreshToken);
            using var context = database.CreateContext();
            var tokenValidatedContext = CreateTokenValidatedContext(context.RefreshTokens.Single().RefreshTokenId);

            await CreateService().VerifyJwtTokenIdPresenceAsync(tokenValidatedContext);

            Assert.NotNull(tokenValidatedContext.Result?.Failure);
        }

        [Fact]
        public async Task VerifyJwtTokenIdPresenceAsync_InactiveUser_Fails()
        {
            database.Seed(TestData.CreateUser());
            await CreateService().LoginAsync("manager@test.com", TestData.Password);

            using var context = database.CreateContext();
            context.Users.Single().Status = RecordStatus.Inactive;
            context.SaveChanges();
            var tokenValidatedContext = CreateTokenValidatedContext(context.RefreshTokens.Single().RefreshTokenId);

            await CreateService().VerifyJwtTokenIdPresenceAsync(tokenValidatedContext);

            Assert.NotNull(tokenValidatedContext.Result?.Failure);
        }
    }
}
