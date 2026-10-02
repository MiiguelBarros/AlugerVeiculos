using API.Contratos.DTOs;
using UnitTests.Infrastructure;

namespace UnitTests.Contratos
{
    public class CreateContractDTOTests
    {
        private static CreateContractDTO CreateValidRequest(int startInDays = 1, int endInDays = 5)
        {
            return new CreateContractDTO
            {
                ClientId = 1,
                VehicleId = 1,
                StartDate = TestData.Today.AddDays(startInDays),
                EndDate = TestData.Today.AddDays(endInDays),
                StartMileage = 1000,
                DailyRate = 50
            };
        }

        [Theory]
        [InlineData(nameof(CreateContractDTO.ClientId))]
        [InlineData(nameof(CreateContractDTO.VehicleId))]
        [InlineData(nameof(CreateContractDTO.StartDate))]
        [InlineData(nameof(CreateContractDTO.EndDate))]
        [InlineData(nameof(CreateContractDTO.StartMileage))]
        [InlineData(nameof(CreateContractDTO.DailyRate))]
        public void Validate_MissingRequiredField_ReturnsRequiredError(string field)
        {
            var request = CreateValidRequest();
            DtoValidator.ClearField(request, field);

            Assert.Contains(field, DtoValidator.InvalidFields(request));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(10000)]
        public void Validate_DailyRateOutOfRange_ReturnsError(decimal dailyRate)
        {
            var request = CreateValidRequest();
            request.DailyRate = dailyRate;

            Assert.Contains(nameof(CreateContractDTO.DailyRate), DtoValidator.InvalidFields(request));
        }

        [Fact]
        public void Validate_StartDateBeforeToday_ReturnsError()
        {
            var request = CreateValidRequest(startInDays: -1, endInDays: 3);

            Assert.Contains(nameof(CreateContractDTO.StartDate), DtoValidator.InvalidFields(request));
        }

        [Fact]
        public void Validate_StartDateToday_IsValid()
        {
            var request = CreateValidRequest(startInDays: 0, endInDays: 2);

            Assert.Empty(DtoValidator.Validate(request));
        }

        [Fact]
        public void Validate_StartDateMoreThanSevenDaysAhead_ReturnsError()
        {
            var request = CreateValidRequest(startInDays: 8, endInDays: 10);

            Assert.Contains(nameof(CreateContractDTO.StartDate), DtoValidator.InvalidFields(request));
        }

        [Fact]
        public void Validate_StartDateSevenDaysAhead_IsValid()
        {
            var request = CreateValidRequest(startInDays: 7, endInDays: 10);

            Assert.Empty(DtoValidator.Validate(request));
        }

        [Fact]
        public void Validate_EndDateEqualToStartDate_ReturnsError()
        {
            var request = CreateValidRequest(startInDays: 2, endInDays: 2);

            Assert.Contains(nameof(CreateContractDTO.EndDate), DtoValidator.InvalidFields(request));
        }

        [Fact]
        public void Validate_EndDateBeforeStartDate_ReturnsError()
        {
            var request = CreateValidRequest(startInDays: 5, endInDays: 2);

            Assert.Contains(nameof(CreateContractDTO.EndDate), DtoValidator.InvalidFields(request));
        }
    }
}
