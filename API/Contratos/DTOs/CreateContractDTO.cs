using System.ComponentModel.DataAnnotations;

namespace API.Contratos.DTOs
{
    public class CreateContractDTO : IValidatableObject
    {
        [Required(ErrorMessage = "O cliente é obrigatório.")]
        public int? ClientId { get; set; }

        [Required(ErrorMessage = "O veículo é obrigatório.")]
        public int? VehicleId { get; set; }

        [Required(ErrorMessage = "A data de início é obrigatória.")]
        public DateOnly? StartDate { get; set; }

        [Required(ErrorMessage = "A data de fim é obrigatória.")]
        public DateOnly? EndDate { get; set; }

        [Required(ErrorMessage = "A quilometragem inicial é obrigatória.")]
        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem inicial não pode ser negativa.")]
        public int? StartMileage { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (StartDate != null && StartDate < today)
                yield return new ValidationResult("A data de início não pode ser anterior à data atual.", new[] { nameof(StartDate) });

            if (StartDate != null && EndDate != null && EndDate <= StartDate)
                yield return new ValidationResult("A data de fim tem de ser posterior à data de início.", new[] { nameof(EndDate) });
        }
    }
}
