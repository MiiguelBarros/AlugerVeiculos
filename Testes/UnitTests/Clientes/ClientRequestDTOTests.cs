using API.Clientes.DTOs;
using UnitTests.Infrastructure;

namespace UnitTests.Clientes
{
    public class ClientRequestDTOTests
    {
        private static ClientRequestDTO CreateValidRequest(string phone = "912345678", string driverLicenseNumber = "P-1234567")
        {
            return new ClientRequestDTO
            {
                FullName = "Ana Ferreira",
                Email = "ana@cliente.com",
                Phone = phone,
                DriverLicenseNumber = driverLicenseNumber
            };
        }

        [Theory]
        [InlineData(nameof(ClientRequestDTO.FullName))]
        [InlineData(nameof(ClientRequestDTO.Email))]
        [InlineData(nameof(ClientRequestDTO.Phone))]
        [InlineData(nameof(ClientRequestDTO.DriverLicenseNumber))]
        public void Validate_MissingRequiredField_ReturnsRequiredError(string field)
        {
            var request = CreateValidRequest();
            DtoValidator.ClearField(request, field);

            Assert.Contains(field, DtoValidator.InvalidFields(request));
        }

        [Theory]
        [InlineData("91234567a")]
        [InlineData("912 345 678")]
        [InlineData("+351912345678")]
        public void Validate_PhoneWithNonDigits_ReturnsError(string phone)
        {
            var request = CreateValidRequest(phone: phone);

            Assert.Contains(nameof(ClientRequestDTO.Phone), DtoValidator.InvalidFields(request));
        }

        [Theory]
        [InlineData("91234567")]
        [InlineData("9123456789")]
        public void Validate_PhoneWithWrongLength_ReturnsError(string phone)
        {
            var request = CreateValidRequest(phone: phone);

            Assert.Contains(nameof(ClientRequestDTO.Phone), DtoValidator.InvalidFields(request));
        }

        [Theory]
        [InlineData("912345678")]
        [InlineData("224567890")]
        public void Validate_ValidPhone_IsValid(string phone)
        {
            var request = CreateValidRequest(phone: phone);

            Assert.Empty(DtoValidator.Validate(request));
        }

        [Fact]
        public void Validate_PhoneNotStartingWith2Or9_ReturnsError()
        {
            var request = CreateValidRequest(phone: "812345678");

            Assert.Contains(nameof(ClientRequestDTO.Phone), DtoValidator.InvalidFields(request));
        }

        [Theory]
        [InlineData("P/123#4")]
        [InlineData("1234567")]
        [InlineData("P-12")]
        public void Validate_InvalidDriverLicenseFormat_ReturnsError(string driverLicenseNumber)
        {
            var request = CreateValidRequest(driverLicenseNumber: driverLicenseNumber);

            Assert.Contains(nameof(ClientRequestDTO.DriverLicenseNumber), DtoValidator.InvalidFields(request));
        }

        [Theory]
        [InlineData("P-1234567")]
        [InlineData("P-123456 7")]
        [InlineData("AB-1234567")]
        public void Validate_ValidDriverLicenseFormat_IsValid(string driverLicenseNumber)
        {
            var request = CreateValidRequest(driverLicenseNumber: driverLicenseNumber);

            Assert.Empty(DtoValidator.Validate(request));
        }
    }
}
