using System.ComponentModel.DataAnnotations;

namespace API.AutenticacaoAutorizacao.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [MaxLength(254, ErrorMessage = "O email não pode exceder 254 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A password é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A password não pode exceder 100 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }
}
