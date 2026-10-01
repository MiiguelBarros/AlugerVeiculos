using API.Models.Enums;
using API.Veiculos.DTOs;
using UnitTests.Infrastructure;

namespace UnitTests.Veiculos
{
    public class VehicleRequestDTOTests
    {
        private static VehicleRequestDTO CreateValidRequest(string licensePlate = "AB-12-CD", int? year = 2021)
        {
            return new VehicleRequestDTO
            {
                Brand = "Renault",
                Model = "Clio",
                LicensePlate = licensePlate,
                Year = year,
                FuelType = FuelType.Gasoline
            };
        }

        [Theory]
        [InlineData(nameof(VehicleRequestDTO.Brand))]
        [InlineData(nameof(VehicleRequestDTO.Model))]
        [InlineData(nameof(VehicleRequestDTO.LicensePlate))]
        [InlineData(nameof(VehicleRequestDTO.Year))]
        [InlineData(nameof(VehicleRequestDTO.FuelType))]
        public void Validate_MissingRequiredField_ReturnsRequiredError(string field)
        {
            var request = CreateValidRequest();
            DtoValidator.ClearField(request, field);

            Assert.Contains(field, DtoValidator.InvalidFields(request));
        }

        [Fact]
        public void Validate_YearAfterCurrentYear_ReturnsError()
        {
            var request = CreateValidRequest(year: DateTime.Today.Year + 1);

            Assert.Contains(nameof(VehicleRequestDTO.Year), DtoValidator.InvalidFields(request));
        }

        [Fact]
        public void Validate_CurrentYear_IsValid()
        {
            var request = CreateValidRequest(year: DateTime.Today.Year);

            Assert.Empty(DtoValidator.Validate(request));
        }

        [Fact]
        public void Validate_YearBefore1992_ReturnsError()
        {
            var request = CreateValidRequest(year: 1991);

            Assert.Contains(nameof(VehicleRequestDTO.Year), DtoValidator.InvalidFields(request));
        }

        [Theory]
        [InlineData("AA-00-AA", 2021)]
        [InlineData("aa00aa", 2021)]
        [InlineData("12 34 AB", 2003)]
        [InlineData("45-XZ-12", 2019)]
        public void Validate_ValidLicensePlateFormat_IsValid(string licensePlate, int year)
        {
            var request = CreateValidRequest(licensePlate, year);

            Assert.Empty(DtoValidator.Validate(request));
        }

        [Theory]
        [InlineData("AA-00-00")]
        [InlineData("ABC-123")]
        [InlineData("AB--12CD")]
        public void Validate_InvalidLicensePlateFormat_ReturnsError(string licensePlate)
        {
            var request = CreateValidRequest(licensePlate);

            Assert.Contains(nameof(VehicleRequestDTO.LicensePlate), DtoValidator.InvalidFields(request));
        }

        [Theory]
        [InlineData("12-34-AB", 2006)]
        [InlineData("45-XZ-12", 2021)]
        public void Validate_LicensePlateFormatIncompatibleWithYear_ReturnsError(string licensePlate, int year)
        {
            var request = CreateValidRequest(licensePlate, year);

            Assert.Contains(nameof(VehicleRequestDTO.LicensePlate), DtoValidator.InvalidFields(request));
        }

        [Theory]
        [InlineData("12-34-AB", 2005)]
        [InlineData("45-XZ-12", 2020)]
        [InlineData("AB-12-CD", 1995)]
        public void Validate_LicensePlateFormatCompatibleWithYear_IsValid(string licensePlate, int year)
        {
            var request = CreateValidRequest(licensePlate, year);

            Assert.Empty(DtoValidator.Validate(request));
        }

        [Fact]
        public void Validate_UnknownFuelType_ReturnsError()
        {
            var request = CreateValidRequest();
            request.FuelType = (FuelType)99;

            Assert.Contains(nameof(VehicleRequestDTO.FuelType), DtoValidator.InvalidFields(request));
        }
    }
}
