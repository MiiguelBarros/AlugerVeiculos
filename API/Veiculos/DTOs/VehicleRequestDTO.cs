using API.Models.Enums;
using API.Shared.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace API.Veiculos.DTOs
{
    public class VehicleRequestDTO : IValidatableObject
    {
        [Required(ErrorMessage = "A marca é obrigatória.")]
        [MaxLength(50, ErrorMessage = "A marca não pode exceder 50 caracteres.")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "O modelo é obrigatório.")]
        [MaxLength(50, ErrorMessage = "O modelo não pode exceder 50 caracteres.")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "A matrícula é obrigatória.")]
        [RegularExpression(@"^(?:[A-Za-z]{2}[- ]?\d{2}[- ]?[A-Za-z]{2}|\d{2}[- ]?\d{2}[- ]?[A-Za-z]{2}|\d{2}[- ]?[A-Za-z]{2}[- ]?\d{2})$",
            ErrorMessage = "A matrícula tem de estar num formato válido (AA-00-AA, 00-00-AA ou 00-AA-00).")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ano de fabrico é obrigatório.")]
        [YearRange(1992)]
        public int? Year { get; set; }

        [Required(ErrorMessage = "O tipo de combustível é obrigatório.")]
        [EnumDataType(typeof(FuelType), ErrorMessage = "O tipo de combustível indicado não existe.")]
        public FuelType? FuelType { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Year == null || string.IsNullOrWhiteSpace(LicensePlate))
                yield break;

            var characters = new string(LicensePlate.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

            var (format, lastYear) = characters switch
            {
                _ when Regex.IsMatch(characters, @"^\d{4}[A-Z]{2}$") => ("00-00-AA", 2005),
                _ when Regex.IsMatch(characters, @"^\d{2}[A-Z]{2}\d{2}$") => ("00-AA-00", 2020),
                _ => (string.Empty, int.MaxValue)
            };

            if (Year > lastYear)
                yield return new ValidationResult(
                    $"Uma matrícula no formato {format} só foi atribuída até {lastYear}, não é compatível com um veículo de {Year}.",
                    new[] { nameof(LicensePlate) });
        }
    }
}
