using System.ComponentModel.DataAnnotations;

namespace API.Shared.Validation
{
    public class YearRangeAttribute : ValidationAttribute
    {
        private readonly int minimumYear;

        public YearRangeAttribute(int minimumYear)
        {
            this.minimumYear = minimumYear;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not int year)
                return ValidationResult.Success;

            var currentYear = DateTime.Today.Year;

            if (year < minimumYear || year > currentYear)
                return new ValidationResult($"O ano tem de estar entre {minimumYear} e {currentYear}.");

            return ValidationResult.Success;
        }
    }
}
