using API.AutenticacaoAutorizacao.DTOs;
using API.AutenticacaoAutorizacao.Interfaces;
using API.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.AutenticacaoAutorizacao.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [ProducesErrorResponseType(typeof(ResponseDTO<string>))]
    public class AuthController : ControllerBase
    {
        private const string RefreshTokenCookieName = "refreshToken";
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [EnableRateLimiting("auth")]
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<Dictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<string>>> Login([FromBody] LoginDTO loginDTO)
        {
            var session = await authService.LoginAsync(loginDTO.Email, loginDTO.Password);

            SetRefreshTokenCookie(session);

            return Ok(ResponseDTO<string>.Ok("Sessão iniciada com sucesso.", session.AccessToken, StatusCodes.Status200OK));
        }

        [EnableRateLimiting("auth")]
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<string>>> Refresh()
        {
            var session = await authService.RefreshAsync(Request.Cookies[RefreshTokenCookieName]);

            SetRefreshTokenCookie(session);

            return Ok(ResponseDTO<string>.Ok("Sessão renovada com sucesso.", session.AccessToken, StatusCodes.Status200OK));
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<string>>> Logout()
        {
            await authService.LogoutAsync(Request.Cookies[RefreshTokenCookieName]);

            Response.Cookies.Delete(RefreshTokenCookieName, BuildRefreshTokenCookieOptions());

            return Ok(ResponseDTO<string>.Ok("Sessão terminada com sucesso.", StatusCodes.Status200OK));
        }

        private void SetRefreshTokenCookie(AuthSessionResult session)
        {
            if (session.RefreshToken == null)
                return;

            var options = BuildRefreshTokenCookieOptions();
            options.Expires = session.RefreshTokenExpiresAt;

            Response.Cookies.Append(RefreshTokenCookieName, session.RefreshToken, options);
        }

        private static CookieOptions BuildRefreshTokenCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth"
            };
        }
    }
}
