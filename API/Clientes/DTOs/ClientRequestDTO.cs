using System.ComponentModel.DataAnnotations;

namespace API.Clientes.DTOs
{
    public class ClientRequestDTO
    {
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [MaxLength(150, ErrorMessage = "O nome completo não pode exceder 150 caracteres.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [MaxLength(254, ErrorMessage = "O email não pode exceder 254 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [RegularExpression(@"^[29]\d{8}$", ErrorMessage = "O telefone tem de ter 9 dígitos e começar por 2 ou 9.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número da carta de condução é obrigatório.")]
        [RegularExpression(@"^[A-Za-z]{1,2}-\d{5,8} ?\d$", ErrorMessage = "Formato inválido (ex. P-123456 7).")]
        public string DriverLicenseNumber { get; set; } = string.Empty;
    }
}
