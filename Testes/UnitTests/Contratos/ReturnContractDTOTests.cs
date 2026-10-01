using API.Contratos.DTOs;
using UnitTests.Infrastructure;

namespace UnitTests.Contratos
{
    public class ReturnContractDTOTests
    {
        [Fact]
        public void Validate_ReturnedAtInFuture_ReturnsError()
        {
            var request = new ReturnContractDTO
            {
                ReturnedAt = TestData.Today.AddDays(1),
                EndMileage = 1200
            };

            Assert.Contains(nameof(ReturnContractDTO.ReturnedAt), DtoValidator.InvalidFields(request));
        }
    }
}
