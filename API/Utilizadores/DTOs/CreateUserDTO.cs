using API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Utilizadores.DTOs
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(150, ErrorMessage = "O nome não pode exceder 150 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [MaxLength(254, ErrorMessage = "O email não pode exceder 254 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A password é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A password não pode exceder 100 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "A password tem de ter pelo menos 8 caracteres, com maiúscula, minúscula, número e símbolo (@$!%*?&).")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "A confirmação da password é obrigatória.")]
        [Compare(nameof(Password), ErrorMessage = "As passwords não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "O role é obrigatório.")]
        [EnumDataType(typeof(Role), ErrorMessage = "O role indicado não existe.")]
        public Role? Role { get; set; }
    }
}
