using System.ComponentModel.DataAnnotations;

namespace API.Contratos.DTOs
{
    public class ReturnContractDTO : IValidatableObject
    {
        [Required(ErrorMessage = "A data de devolução é obrigatória.")]
        public DateOnly? ReturnedAt { get; set; }

        [Required(ErrorMessage = "A quilometragem final é obrigatória.")]
        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem final não pode ser negativa.")]
        public int? EndMileage { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ReturnedAt != null && ReturnedAt > DateOnly.FromDateTime(DateTime.Today))
                yield return new ValidationResult("A data de devolução não pode ser posterior à data atual.", new[] { nameof(ReturnedAt) });
        }
    }
}
