using API.Models.Enums;
using API.Utilizadores.DTOs;
using UnitTests.Infrastructure;

namespace UnitTests.Utilizadores
{
    public class CreateUserDTOTests
    {
        private static CreateUserDTO CreateValidRequest(string password = TestData.Password)
        {
            return new CreateUserDTO
            {
                Name = "Rui Martins",
                Email = "rui@test.com",
                Password = password,
                ConfirmPassword = password,
                Role = Role.Employee
            };
        }

        [Theory]
        [InlineData("strong123@")]
        [InlineData("Strongabc@")]
        [InlineData("Strong1234")]
        [InlineData("St1@")]
        public void Validate_WeakPassword_ReturnsError(string password)
        {
            var request = CreateValidRequest(password);

            Assert.Contains(nameof(CreateUserDTO.Password), DtoValidator.InvalidFields(request));
        }

        [Fact]
        public void Validate_PasswordsDoNotMatch_ReturnsError()
        {
            var request = CreateValidRequest();
            request.ConfirmPassword = "Other123@";

            Assert.Contains(nameof(CreateUserDTO.ConfirmPassword), DtoValidator.InvalidFields(request));
        }

        [Theory]
        [InlineData(99)]
        [InlineData(3)]
        public void Validate_UnknownRole_ReturnsError(int role)
        {
            var request = CreateValidRequest();
            request.Role = (Role)role;

            Assert.Contains(nameof(CreateUserDTO.Role), DtoValidator.InvalidFields(request));
        }
    }
}
