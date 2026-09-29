namespace API.AutenticacaoAutorizacao.DTOs
{
    public record AuthSessionResult(string AccessToken, string? RefreshToken, DateTime RefreshTokenExpiresAt);
}
