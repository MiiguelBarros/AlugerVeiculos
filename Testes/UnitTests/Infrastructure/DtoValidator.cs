using System.ComponentModel.DataAnnotations;

namespace UnitTests.Infrastructure
{
    public static class DtoValidator
    {
        public static IList<ValidationResult> Validate(object dto)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);

            return results;
        }

        public static IEnumerable<string> InvalidFields(object dto)
        {
            return Validate(dto).SelectMany(result => result.MemberNames);
        }

        public static void ClearField(object dto, string field)
        {
            dto.GetType().GetProperty(field)!.SetValue(dto, null);
        }
    }
}
